using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    public class Sensor
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        public SensorMetric Metric { get; private set; }

        // Deterministický seed simulace — stejný senzor dává po restartu stejná data
        public int Seed { get; private set; }


        // Cizí klíče
        public Guid FacilityId { get; private set; }
        public Guid? ZoneId { get; private set; }


        // Časové údaje
        public DateTimeOffset InstalledAt { get; private set; } = DateTimeOffset.UtcNow;


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;
        public Zone? Zone { get; set; }

        private Sensor() { }

        public Sensor(Guid facilityId, Guid? zoneId, SensorMetric metric, int seed, DateTimeOffset installedAt)
        {
            if (metric != SensorMetric.Power && zoneId == null)
                throw new ArgumentException("Senzor teploty a CO₂ musí patřit do zóny.", nameof(zoneId));

            FacilityId = facilityId;
            ZoneId = zoneId;
            Metric = metric;
            Seed = seed;
            InstalledAt = installedAt;
        }
    }
}
