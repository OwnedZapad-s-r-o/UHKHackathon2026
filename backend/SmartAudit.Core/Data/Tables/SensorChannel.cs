using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    // Kanál objevený v odpovědi gatewaye: jeden senzor na jednom uzlu (např. teplota na uzlu 2).
    // Dokud ho uživatel nepřiřadí do místnosti (SensorId), hodnoty se jen ukazují, neukládají.
    public class SensorChannel
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // n{uzel}/t{typ}-{pořadí} — stabilní klíč napříč odpověďmi
        public string ChannelKey { get; private set; } = string.Empty;

        public int NodeAddress { get; private set; }
        public int IqrfType { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Unit { get; private set; } = string.Empty;
        public SensorMetric Metric { get; private set; }

        public double? LastValue { get; private set; }
        public DateTimeOffset? LastSeenAt { get; private set; }


        // Cizí klíče
        public Guid DeviceId { get; private set; }
        public Guid? SensorId { get; private set; }


        // Navigační vlastnosti
        public SensorDevice Device { get; set; } = null!;
        public Sensor? Sensor { get; set; }

        private SensorChannel() { }

        public SensorChannel(Guid deviceId, string channelKey, int nodeAddress, int iqrfType, string name, string unit, SensorMetric metric)
        {
            if (string.IsNullOrWhiteSpace(channelKey))
                throw new ArgumentException("Klíč kanálu nesmí být prázdný.", nameof(channelKey));

            DeviceId = deviceId;
            ChannelKey = channelKey;
            NodeAddress = nodeAddress;
            IqrfType = iqrfType;
            Name = string.IsNullOrWhiteSpace(name) ? $"Senzor typu {iqrfType}" : name;
            Unit = unit;
            Metric = metric;
        }

        public void RecordValue(double value, DateTimeOffset timestamp)
        {
            LastValue = value;
            LastSeenAt = timestamp;
        }

        public void LinkSensor(Guid sensorId, SensorMetric metric)
        {
            SensorId = sensorId;
            Metric = metric;
        }
    }
}
