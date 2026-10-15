using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services.Sensors
{
    // Count = místnosti, kde teplotu měří skutečné čidlo; Virtual = aspoň jedno z nich je virtuální (dodalo historii)
    public record MeasuredZoneCount(int Count, bool Virtual);

    // Kolik místností budovy měří skutečná čidla teploty — jen ta mění odhad ztráty
    public static class MeasuredZones
    {
        public static async Task<Dictionary<Guid, MeasuredZoneCount>> QueryAsync(SmartAuditDbContext dbContext, IReadOnlyCollection<Guid> facilityIds, CancellationToken ct)
        {
            var sensors = await dbContext.Sensors
                .AsNoTracking()
                .Where(s => facilityIds.Contains(s.FacilityId) && s.Source == SensorSource.Physical && s.IsActive && s.Metric == SensorMetric.Temperature && s.ZoneId != null)
                .Select(s => new { s.FacilityId, s.ZoneId, IsVirtual = s.Device != null && s.Device.IsVirtual })
                .ToListAsync(ct);

            return sensors
                .GroupBy(s => s.FacilityId)
                .ToDictionary(g => g.Key, g => new MeasuredZoneCount(g.Select(s => s.ZoneId).Distinct().Count(), g.Any(s => s.IsVirtual)));
        }
    }
}
