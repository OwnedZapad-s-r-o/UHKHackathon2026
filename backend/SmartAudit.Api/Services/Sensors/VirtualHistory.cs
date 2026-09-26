using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Sensors
{
    // Virtuální zařízení se po přiřazení do místnosti tváří, že v ní měří už týden: dopočítá měření posledních 7 dní
    // ze stejného průběhu, jaký posílá živě. Audit tak ztrátu opraví hned, ne až za týden — v demu to ukazujeme otevřeně.
    // Skutečné čidlo historii nemá; tam se ztráta upřesňuje postupně, jak přibývají měření.
    public static class VirtualHistory
    {
        public const int Days = 7;

        private static readonly TimeSpan Step = TimeSpan.FromMinutes(15);

        public static List<SensorReading> Backfill(SensorDevice device, int nodeAddress, int iqrfType, Guid sensorId, DateTimeOffset now)
        {
            if (!device.IsVirtual)
                return [];

            var node = VirtualGateways.NodesOf(device.GatewayId, device.NodeAddresses).FirstOrDefault(n => n.Address == nodeAddress);
            if (node == null)
                return [];

            var profile = VirtualGateways.ProfileOf(device.GatewayId);

            // Do poslední celé čtvrthodiny — živá měření mají přesný čas, se kterým se dopočítaná nepotkají
            var end = new DateTimeOffset(now.UtcTicks - now.UtcTicks % Step.Ticks, TimeSpan.Zero);
            var readings = new List<SensorReading>();
            for (var timestamp = end.AddDays(-Days); timestamp < end; timestamp += Step)
                readings.Add(new SensorReading(sensorId, timestamp, VirtualGateways.Value(device.GatewayId, profile, nodeAddress, iqrfType, timestamp, node.Overheats)));

            return readings;
        }
    }
}
