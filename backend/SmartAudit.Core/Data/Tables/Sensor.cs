using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    public class Sensor
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        public SensorMetric Metric { get; private set; }
        public SensorSource Source { get; private set; } = SensorSource.Simulated;

        // Neaktivní simulované čidlo už negeneruje data — zónu převzalo skutečné čidlo
        public bool IsActive { get; private set; } = true;

        // Poslední hodnota skutečného čidla (u simulace se nevede)
        public double? LastValue { get; private set; }
        public DateTimeOffset? LastSeenAt { get; private set; }

        // Deterministický seed simulace — stejný senzor dává po restartu stejná data
        public int Seed { get; private set; }


        // Cizí klíče
        public Guid FacilityId { get; private set; }
        public Guid? ZoneId { get; private set; }
        public Guid? DeviceId { get; private set; }


        // Časové údaje
        public DateTimeOffset InstalledAt { get; private set; } = DateTimeOffset.UtcNow;


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;
        public Zone? Zone { get; set; }
        public SensorDevice? Device { get; set; }

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

        // Skutečné čidlo z IQRF gateway — zóna je volitelná (např. elektroměr pro celou budovu)
        public static Sensor Physical(Guid facilityId, Guid? zoneId, SensorMetric metric, Guid deviceId, DateTimeOffset installedAt)
        {
            return new Sensor
            {
                FacilityId = facilityId,
                ZoneId = zoneId,
                Metric = metric,
                DeviceId = deviceId,
                Source = SensorSource.Physical,
                InstalledAt = installedAt,
            };
        }

        public void Assign(Guid? zoneId, SensorMetric metric)
        {
            if (Source != SensorSource.Physical)
                throw new InvalidOperationException("Přiřadit lze jen skutečné čidlo.");

            ZoneId = zoneId;
            Metric = metric;
        }

        public void RecordReading(double value, DateTimeOffset timestamp)
        {
            LastValue = value;
            LastSeenAt = timestamp;
        }

        public void Deactivate() => IsActive = false;

        public void Activate() => IsActive = true;
    }
}
