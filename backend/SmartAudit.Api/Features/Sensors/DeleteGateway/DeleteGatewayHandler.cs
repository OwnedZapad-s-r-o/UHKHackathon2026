using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Sensors.DeleteGateway
{
    public record DeleteGatewayCommand(Guid Id, ViewerContext Viewer) : IRequest<Response<bool>>;

    // Odebrání gatewaye smaže její kanály, skutečná čidla i jejich měření; místnosti znovu převezme simulace
    public class DeleteGatewayHandler : IRequestHandler<DeleteGatewayCommand, Response<bool>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IAuditTrigger _auditTrigger;

        public DeleteGatewayHandler(SmartAuditDbContext dbContext, IAuditTrigger auditTrigger)
        {
            _dbContext = dbContext;
            _auditTrigger = auditTrigger;
        }

        public async Task<Response<bool>> Handle(DeleteGatewayCommand request, CancellationToken ct)
        {
            var device = await _dbContext.SensorDevices
                .Include(d => d.Facility)
                .Include(d => d.Sensors)
                .FirstOrDefaultAsync(d => d.Id == request.Id, ct);

            if (device == null)
                return Response.NotFound<bool>("Gateway s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && device.Facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<bool>("K tomuto nemáte oprávnění");

            foreach (var sensor in device.Sensors)
                await SimulationHandover.ReleaseAsync(_dbContext, device.FacilityId, sensor.ZoneId, sensor.Metric, sensor.Id, ct);

            _dbContext.SensorDevices.Remove(device);

            await _dbContext.SaveChangesAsync(ct);

            _auditTrigger.RequestFacility(device.FacilityId);

            return Response.Success();
        }
    }
}
