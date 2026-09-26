using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services.Sensors
{
    // Předávka zóny mezi simulací a skutečným čidlem: když zónu a veličinu měří skutečné čidlo,
    // simulované čidlo stejné zóny a veličiny přestane generovat data; když skutečné zmizí, simulace naváže.
    // První skutečné čidlo teploty si zapamatuje odhad ztráty „před čidlem“ — z něj je vidět, jak měření ztrátu opravilo.
    public static class SimulationHandover
    {
        public static async Task TakeOverAsync(SmartAuditDbContext dbContext, Guid facilityId, Guid? zoneId, SensorMetric metric, CancellationToken ct)
        {
            var simulated = await dbContext.Sensors
                .Where(s => s.FacilityId == facilityId && s.ZoneId == zoneId && s.Metric == metric && s.Source == SensorSource.Simulated && s.IsActive)
                .ToListAsync(ct);

            foreach (var sensor in simulated)
                sensor.Deactivate();

            // Ztrátu počítá audit jen z teploty — čidlo CO₂ nebo hluku na ní nic nemění
            if (metric == SensorMetric.Temperature && await dbContext.FacilityAssessments.FirstOrDefaultAsync(a => a.FacilityId == facilityId, ct) is { } assessment)
                assessment.MarkSensorsInstalled(DateTimeOffset.UtcNow);
        }

        public static async Task ReleaseAsync(SmartAuditDbContext dbContext, Guid facilityId, Guid? zoneId, SensorMetric metric, Guid releasedSensorId, CancellationToken ct)
        {
            var stillCovered = await dbContext.Sensors.AnyAsync(
                s => s.Id != releasedSensorId && s.FacilityId == facilityId && s.ZoneId == zoneId && s.Metric == metric && s.Source == SensorSource.Physical && s.IsActive,
                ct);

            if (stillCovered)
                return;

            var simulated = await dbContext.Sensors
                .Where(s => s.FacilityId == facilityId && s.ZoneId == zoneId && s.Metric == metric && s.Source == SensorSource.Simulated && !s.IsActive)
                .ToListAsync(ct);

            foreach (var sensor in simulated)
                sensor.Activate();
        }
    }
}
