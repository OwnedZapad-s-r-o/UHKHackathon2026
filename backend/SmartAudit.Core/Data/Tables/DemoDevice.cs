using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    // Zařízení vyrobené v demo správě: virtuální gateway z výroby nebo čidlo v dosahu gatewaye.
    // Hraje je aplikace (VirtualGatewayService) stejně jako pevný katalog PairableDevices — párují se heslem / kódem z nálepky.
    public class DemoDevice
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        public DemoDeviceKind Kind { get; private set; }

        // MID modulu IQRF — 8 hexadecimálních znaků, na nálepce
        public string Serial { get; private set; } = string.Empty;
        public string Model { get; private set; } = string.Empty;

        // Heslo gatewaye nebo kód čidla pro SmartConnect, ve tvaru XXXX-XXXX (v demu je vidět na nálepce)
        public string Code { get; private set; } = string.Empty;

        // Šablony uzlů (DemoNodeTemplates) — gateway jich má 1–3, čidlo právě jednu
        public string[] Templates { get; private set; } = [];

        public DemoBehavior Behavior { get; private set; }

        // Jen u čidla: gateway (id v MQTT topicu), v jejímž dosahu je, a adresa, kterou mu koordinátor přidělí
        public string? ParentGatewayId { get; private set; }
        public int? NodeAddress { get; private set; }


        // Cizí klíče
        // Organizace správce, který zařízení vyrobil — kraj (null) vidí všechna
        public Guid? OrganizationId { get; private set; }


        // Časové údaje
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        private DemoDevice() { }

        public static DemoDevice Gateway(string serial, string model, string password, string[] templates, DemoBehavior behavior, Guid? organizationId)
        {
            if (templates.Length is < 1 or > 3)
                throw new ArgumentException("Gateway musí mít 1 až 3 uzly.", nameof(templates));

            return new DemoDevice
            {
                Kind = DemoDeviceKind.Gateway,
                Serial = ValidSerial(serial),
                Model = model,
                Code = password,
                Templates = templates,
                Behavior = behavior,
                OrganizationId = organizationId,
            };
        }

        public static DemoDevice Node(string serial, string code, string template, DemoBehavior behavior, string parentGatewayId, int nodeAddress, Guid? organizationId)
        {
            if (nodeAddress is < 1 or > 239)
                throw new ArgumentException("Adresa uzlu musí být 1–239.", nameof(nodeAddress));

            if (string.IsNullOrWhiteSpace(parentGatewayId))
                throw new ArgumentException("Čidlo musí být v dosahu gatewaye.", nameof(parentGatewayId));

            return new DemoDevice
            {
                Kind = DemoDeviceKind.Node,
                Serial = ValidSerial(serial),
                Model = "IQRF sensor",
                Code = code,
                Templates = [template],
                Behavior = behavior,
                ParentGatewayId = parentGatewayId,
                NodeAddress = nodeAddress,
                OrganizationId = organizationId,
            };
        }

        private static string ValidSerial(string serial)
        {
            if (serial.Length != 8 || !serial.All(Uri.IsHexDigit))
                throw new ArgumentException("Serial musí mít 8 hexadecimálních znaků.", nameof(serial));

            return serial.ToUpperInvariant();
        }
    }
}
