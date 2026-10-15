using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    // Část budovy pro schematický řez podlaží × zóny (F21)
    public class Zone
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        public string Name { get; private set; } = string.Empty;
        public ZoneType Type { get; private set; }
        public int Floor { get; private set; }
        public string Wing { get; private set; } = string.Empty;
        public int Order { get; private set; }

        // Místo, kde se v simulaci projevuje problém profilu (např. špatně větraná učebna)
        public bool IsHotspot { get; private set; }


        // Cizí klíče
        public Guid FacilityId { get; private set; }


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;
        public ICollection<Sensor> Sensors { get; private set; } = new List<Sensor>();

        private Zone() { }

        public Zone(Guid facilityId, string name, ZoneType type, int floor, string wing, int order, bool isHotspot = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Název zóny nesmí být prázdný.", nameof(name));

            if (floor < 0)
                throw new ArgumentException("Podlaží nesmí být záporné.", nameof(floor));

            FacilityId = facilityId;
            Name = name;
            Type = type;
            Floor = floor;
            Wing = wing;
            Order = order;
            IsHotspot = isHotspot;
        }

        public void SetHotspot(bool isHotspot)
        {
            IsHotspot = isHotspot;
        }
    }
}
