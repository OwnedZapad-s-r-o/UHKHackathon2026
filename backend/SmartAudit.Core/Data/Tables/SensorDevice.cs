namespace SmartAudit.Core.Data.Tables
{
    // IQRF gateway v objektu — komunikuje přes MQTT, v topicu se ohlašuje svým GatewayId
    public class SensorDevice
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        public string Name { get; private set; } = string.Empty;

        // Id gatewaye v MQTT topicu gateway/{GatewayId}/iqrf/...
        public string GatewayId { get; private set; } = string.Empty;

        // Jak často se uzly dotazují na hodnoty
        public int PollSeconds { get; private set; } = 15;

        // Adresy spárovaných uzlů IQMESH (zjistí se z koordinátoru, nebo je zadá uživatel)
        public int[] NodeAddresses { get; private set; } = [];

        // Virtuální gateway (testovací síť, jednoklikové demo): odpovídá za ni aplikace sama přes MQTT, jinak se chová jako skutečná
        public bool IsVirtual { get; private set; }


        // Cizí klíče
        public Guid FacilityId { get; private set; }


        // Časové údaje
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastSeenAt { get; private set; }


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;
        public ICollection<SensorChannel> Channels { get; private set; } = new List<SensorChannel>();
        public ICollection<Sensor> Sensors { get; private set; } = new List<Sensor>();

        private SensorDevice() { }

        public SensorDevice(Guid facilityId, string name, string gatewayId, int pollSeconds, int[] nodeAddresses, bool isVirtual = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Název gatewaye nesmí být prázdný.", nameof(name));

            if (string.IsNullOrWhiteSpace(gatewayId))
                throw new ArgumentException("Id gatewaye nesmí být prázdné.", nameof(gatewayId));

            if (pollSeconds < 5)
                throw new ArgumentException("Interval dotazování musí být alespoň 5 s.", nameof(pollSeconds));

            FacilityId = facilityId;
            Name = name.Trim();
            GatewayId = gatewayId.Trim();
            PollSeconds = pollSeconds;
            IsVirtual = isVirtual;
            SetNodes(nodeAddresses);
        }

        public void MarkSeen(DateTimeOffset timestamp) => LastSeenAt = timestamp;

        // Adresa 0 je koordinátor — senzory jsou jen na uzlech 1–239
        public void SetNodes(IEnumerable<int> nodeAddresses) => NodeAddresses = nodeAddresses.Where(n => n is > 0 and < 240).Distinct().Order().ToArray();
    }
}
