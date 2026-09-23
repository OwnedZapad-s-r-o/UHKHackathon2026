using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.Facilities.GetSection
{
    public record GetSectionCommand(Guid FacilityId, ViewerContext Viewer) : IRequest<Response<GetSectionDto>>;

    public record GetSectionDto(int Floors, bool FloorsEstimated, double Co2Limit, IReadOnlyCollection<string> Wings, IReadOnlyCollection<SectionZoneDto> Zones, DateTimeOffset? UpdatedAt);

    public record SectionZoneDto(Guid Id, string Name, ZoneType Type, int Floor, string Wing, bool IsHotspot, double? Temperature, double? Co2);

    // Schematický řez podlaží × zóny s posledními hodnotami (F21) — dispozice je schéma, ne reálný plán
    public class GetSectionHandler : IRequestHandler<GetSectionCommand, Response<GetSectionDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly AuditSettings _auditSettings;

        public GetSectionHandler(SmartAuditDbContext dbContext, AuditSettings auditSettings)
        {
            _dbContext = dbContext;
            _auditSettings = auditSettings;
        }

        public async Task<Response<GetSectionDto>> Handle(GetSectionCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Select(f => new { f.Id, f.OrganizationId, Floors = f.Building != null ? f.Building.EstimatedFloors : (int?)null })
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId, ct);

            if (facility == null)
                return Response.NotFound<GetSectionDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<GetSectionDto>("K tomuto nemáte oprávnění");

            var zones = await _dbContext.Zones
                .AsNoTracking()
                .Where(z => z.FacilityId == request.FacilityId)
                .OrderBy(z => z.Floor)
                .ThenBy(z => z.Order)
                .Select(z => new { z.Id, z.Name, z.Type, z.Floor, z.Wing, z.IsHotspot })
                .ToListAsync(ct);

            var since = DateTimeOffset.UtcNow.AddHours(-3);

            var latest = await _dbContext.Sensors
                .AsNoTracking()
                .Where(s => s.FacilityId == request.FacilityId && s.ZoneId != null)
                .Select(s => new
                {
                    s.ZoneId,
                    s.Metric,
                    Last = _dbContext.SensorReadings
                        .Where(r => r.SensorId == s.Id && r.Timestamp >= since)
                        .OrderByDescending(r => r.Timestamp)
                        .Select(r => new { r.Timestamp, r.Value })
                        .FirstOrDefault(),
                })
                .ToListAsync(ct);

            double? ValueFor(Guid zoneId, SensorMetric metric) => latest.FirstOrDefault(l => l.ZoneId == zoneId && l.Metric == metric)?.Last?.Value;

            var sectionZones = zones
                .Select(z => new SectionZoneDto(z.Id, z.Name, z.Type, z.Floor, z.Wing, z.IsHotspot, ValueFor(z.Id, SensorMetric.Temperature), ValueFor(z.Id, SensorMetric.Co2)))
                .ToList();

            var floors = Math.Max(facility.Floors ?? 1, zones.Count > 0 ? zones.Max(z => z.Floor) + 1 : 1);
            var updatedAt = latest.Select(l => l.Last?.Timestamp).Max();

            var dto = new GetSectionDto(floors, true, _auditSettings.Co2LimitPpm, zones.Select(z => z.Wing).Distinct().ToList(), sectionZones, updatedAt);

            return Response.Success(dto);
        }
    }
}
