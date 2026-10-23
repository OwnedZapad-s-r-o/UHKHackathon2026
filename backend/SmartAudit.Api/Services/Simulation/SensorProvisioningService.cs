using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Simulation
{
    public interface ISensorProvisioningService
    {
        Task<int> ProvisionAsync(DateTimeOffset installedAt, CancellationToken ct);
    }

    // Každý objekt portfolia dostane zóny pro řez budovou a v každé zóně teplotu + CO₂, na objekt jeden měřič příkonu
    public class SensorProvisioningService : ISensorProvisioningService
    {
        private readonly SmartAuditDbContext _dbContext;

        public SensorProvisioningService(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> ProvisionAsync(DateTimeOffset installedAt, CancellationToken ct)
        {
            var facilities = await _dbContext.Facilities
                .Include(f => f.Building)
                .Where(f => !f.Sensors.Any(s => s.Source == SensorSource.Simulated))
                // Referenční objekt má jen skutečná čidla — simulace pro něj nevzniká
                .Where(f => !f.SourceKey.StartsWith(Facility.ReferenceSourcePrefix))
                .ToListAsync(ct);

            foreach (var facility in facilities)
            {
                var floors = facility.Building?.EstimatedFloors ?? BuildingEstimator.EstimateFloors(null, facility.Category);
                var templates = ZoneLayout.Create(facility.Category, facility.SimulationProfile, facility.Kinds, facility.Name, floors, StableHash.Of(facility.SourceKey));

                var order = 0;
                foreach (var template in templates)
                {
                    var zone = new Zone(facility.Id, template.Name, template.Type, template.Floor, template.Wing, order++, template.IsHotspot);
                    _dbContext.Zones.Add(zone);

                    _dbContext.Sensors.Add(new Sensor(facility.Id, zone.Id, SensorMetric.Temperature, StableHash.Of($"{facility.SourceKey}|{zone.Name}|T"), installedAt));
                    _dbContext.Sensors.Add(new Sensor(facility.Id, zone.Id, SensorMetric.Co2, StableHash.Of($"{facility.SourceKey}|{zone.Name}|CO2"), installedAt));
                }

                _dbContext.Sensors.Add(new Sensor(facility.Id, null, SensorMetric.Power, StableHash.Of($"{facility.SourceKey}|P"), installedAt));
            }

            await _dbContext.SaveChangesAsync(ct);

            return facilities.Count;
        }
    }
}
