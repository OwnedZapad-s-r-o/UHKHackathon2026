using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Features.Sensors.AssignChannel
{
    // ZoneId null = čidlo pro celý objekt (např. elektroměr)
    public record AssignChannelRequest(Guid? ZoneId, SensorMetric Metric);

    public record AssignChannelCommand(ViewerContext Viewer, Guid DeviceId, Guid ChannelId, Guid? ZoneId, SensorMetric Metric) : IRequest<Response<AssignChannelDto>>;

    public record AssignChannelDto(Guid SensorId);

    // Přiřazení objeveného kanálu do místnosti: vznikne skutečné čidlo, jeho hodnoty se začnou ukládat
    // a simulované čidlo té místnosti a veličiny přestane generovat data
    public class AssignChannelHandler : IRequestHandler<AssignChannelCommand, Response<AssignChannelDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IAuditTrigger _auditTrigger;

        public AssignChannelHandler(SmartAuditDbContext dbContext, IAuditTrigger auditTrigger)
        {
            _dbContext = dbContext;
            _auditTrigger = auditTrigger;
        }

        public async Task<Response<AssignChannelDto>> Handle(AssignChannelCommand request, CancellationToken ct)
        {
            var channel = await _dbContext.SensorChannels
                .Include(c => c.Device)
                .ThenInclude(d => d.Facility)
                .FirstOrDefaultAsync(c => c.Id == request.ChannelId && c.DeviceId == request.DeviceId, ct);

            if (channel == null)
                return Response.NotFound<AssignChannelDto>("Kanál s tímto id neexistuje");

            var facility = channel.Device.Facility;
            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<AssignChannelDto>("K tomuto nemáte oprávnění");

            if (request.ZoneId != null && !await _dbContext.Zones.AnyAsync(z => z.Id == request.ZoneId && z.FacilityId == facility.Id, ct))
                return Response.ValidationFailed<AssignChannelDto>("Místnost nepatří k objektu této gatewaye");

            Sensor sensor;
            if (channel.SensorId is { } sensorId && await _dbContext.Sensors.FirstOrDefaultAsync(s => s.Id == sensorId, ct) is { } existing)
            {
                // Přesun do jiné místnosti — původní místnost vrátí simulaci, pokud ji neměří jiné čidlo
                await SimulationHandover.ReleaseAsync(_dbContext, facility.Id, existing.ZoneId, existing.Metric, existing.Id, ct);
                existing.Assign(request.ZoneId, request.Metric);
                sensor = existing;
            }
            else
            {
                sensor = Sensor.Physical(facility.Id, request.ZoneId, request.Metric, channel.DeviceId, DateTimeOffset.UtcNow);
                _dbContext.Sensors.Add(sensor);
            }

            channel.LinkSensor(sensor.Id, request.Metric);
            await SimulationHandover.TakeOverAsync(_dbContext, facility.Id, request.ZoneId, request.Metric, ct);

            await _dbContext.SaveChangesAsync(ct);

            _auditTrigger.RequestFacility(facility.Id);

            return Response.Success(new AssignChannelDto(sensor.Id));
        }
    }
}
