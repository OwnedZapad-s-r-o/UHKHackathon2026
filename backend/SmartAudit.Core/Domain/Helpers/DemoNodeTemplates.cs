using System.Security.Cryptography;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Druh krabičky, kterou jde v demo správě vyrobit: co měří a do jaké místnosti typicky patří
    public record DemoNodeTemplate(string Id, string Label, ZoneType Zone, IReadOnlyList<VirtualSensor> Sensors);

    public static class DemoNodeTemplates
    {
        public const string GatewayModel = "IQD-GW04";

        public static readonly IReadOnlyList<DemoNodeTemplate> All =
        [
            new("ucebna", "Učebna", ZoneType.Classroom, [VirtualGateways.Temperature, VirtualGateways.Co2, VirtualGateways.Humidity]),
            new("kancelar", "Kancelář", ZoneType.Office, [VirtualGateways.Temperature, VirtualGateways.Humidity]),
            new("telocvicna", "Tělocvična", ZoneType.Gym, [VirtualGateways.Temperature, VirtualGateways.Noise]),
            new("chodba", "Chodba", ZoneType.Canteen, [VirtualGateways.Temperature, VirtualGateways.Illuminance, VirtualGateways.Motion]),
            new("co2", "Čidlo CO₂", ZoneType.Classroom, [VirtualGateways.Co2]),
        ];

        public static DemoNodeTemplate? Find(string id) => All.FirstOrDefault(t => t.Id == id);

        public static VirtualNode Node(string templateId, int address, DemoBehavior behavior)
        {
            var template = Find(templateId) ?? All[0];
            return new VirtualNode(address, template.Label, template.Zone, template.Sensors, behavior == DemoBehavior.Overheating);
        }

        // MID nového zařízení — náhodný, první znak nenulový (vypadá jako skutečný MID modulu)
        public static string NewSerial() => $"{RandomNumberGenerator.GetInt32(1, 16):X}{RandomNumberGenerator.GetInt32(0, 1 << 28):X7}";

        // Heslo / kód na nálepku ze znaků bez záměn (0/O, 1/I)
        public static string NewCode()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            return PairingCode.Format(RandomNumberGenerator.GetString(alphabet, 8));
        }
    }
}
