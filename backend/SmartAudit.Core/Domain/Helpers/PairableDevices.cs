using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Gateway z výroby: zapnutá a online, ale v žádné síti. Na nálepce má serial (MID) a párovací heslo.
    public record PairableGateway(string Serial, string Model, string Password, IReadOnlyList<VirtualNode> Nodes)
    {
        public string GatewayId => PairableDevices.GatewayIdOf(Serial);
    }

    // Čidlo (uzel IQMESH) v dosahu gatewaye, zatím mimo její síť. Na nálepce má MID a kód pro IQRF SmartConnect.
    // Adresa uzlu = ta, kterou mu koordinátor přidělí (první volná za uzly gatewaye).
    public record NearbyNode(string Mid, string Code, VirtualNode Node);

    // Emulovaná zařízení k párování — stejný katalog má Python emulátor (tools/iqrf-emulator/multi_emulator.py).
    // K pevnému katalogu se přidávají zařízení vyrobená v demo správě (tabulka DemoDevices) — ta hraje jen aplikace.
    // Hesla a kódy jsou veřejné schválně: v demu je uživatel „přečte z nálepky“ na obrazovce.
    public static class PairableDevices
    {
        private sealed record DemoCatalog(IReadOnlyList<PairableGateway> Gateways, IReadOnlyDictionary<string, NearbyNode[]> Nearby);

        // Snímek zařízení z demo správy — celý se vymění při každém načtení z databáze, čtení tak nepotřebuje zámek
        private static volatile DemoCatalog _demo = new([], new Dictionary<string, NearbyNode[]>());

        public static readonly IReadOnlyList<PairableGateway> Gateways =
        [
            new("81A4F2C0", "IQD-GW04", "7Q4M-2PXA",
            [
                new(1, "Učebna", ZoneType.Classroom, [VirtualGateways.Temperature, VirtualGateways.Co2, VirtualGateways.Humidity]),
                new(2, "Tělocvična", ZoneType.Gym, [VirtualGateways.Temperature, VirtualGateways.Noise]),
            ]),
            new("81A4F3D9", "IQD-GW04", "H3TR-9KLC",
            [
                new(1, "Knihovna", ZoneType.Classroom, [VirtualGateways.Temperature, VirtualGateways.Humidity, VirtualGateways.Illuminance]),
            ]),
        ];

        // Čidla v dosahu gatewayí testovací sítě
        private static readonly Dictionary<string, NearbyNode[]> Nearby = new(StringComparer.OrdinalIgnoreCase)
        {
            ["gw-rk-sps"] = [new("5C0E91A7", "RK2B-X8QD", new(3, "Učebna 2.B", ZoneType.Classroom, [VirtualGateways.Co2, VirtualGateways.Temperature]))],
            ["gw-hk-tyl"] = [new("5C0E92F3", "HK7N-4WZE", new(3, "Chodba", ZoneType.Canteen, [VirtualGateways.Noise]))],
        };

        // Znaky kódů bez záměn (0/O, 1/I)
        private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        // Id gatewaye po spárování — stejné v aplikaci i v emulátoru, aby emulátor po restartu poznal, že už je připojená
        public static string GatewayIdOf(string serial) => $"iqrf-{serial.ToLowerInvariant()}";

        // Pevný katalog i zařízení z demo správy
        public static IReadOnlyList<PairableGateway> All => [.. Gateways, .. _demo.Gateways];

        public static void UseDemoDevices(IReadOnlyList<PairableGateway> gateways, IReadOnlyDictionary<string, NearbyNode[]> nearby) =>
            _demo = new DemoCatalog(gateways, new Dictionary<string, NearbyNode[]>(nearby, StringComparer.OrdinalIgnoreCase));

        public static PairableGateway? FindGateway(string serial) => All.FirstOrDefault(g => string.Equals(g.Serial, serial, StringComparison.OrdinalIgnoreCase));

        public static PairableGateway? FindByGatewayId(string gatewayId) => All.FirstOrDefault(g => string.Equals(g.GatewayId, gatewayId, StringComparison.OrdinalIgnoreCase));

        // Čidla v dosahu: pevný katalog + čidla z demo správy. Virtuální učebna z jednoklikového dema má navíc jedno čidlo,
        // jehož MID a kód se odvodí z id gatewaye.
        public static IReadOnlyList<NearbyNode> NearbyOf(string gatewayId)
        {
            var demo = _demo.Nearby.TryGetValue(gatewayId, out var added) ? added : [];

            if (Nearby.TryGetValue(gatewayId, out var nodes))
                return [.. nodes, .. demo];

            if (!gatewayId.StartsWith("virtualni-", StringComparison.OrdinalIgnoreCase))
                return demo;

            var mid = ((uint)StableHash.Of($"{gatewayId}/mid")).ToString("X8");
            var code = DerivedCode($"{gatewayId}/code");
            return [new(mid, code, new(3, "Učebna vedle", ZoneType.Classroom, [VirtualGateways.Co2, VirtualGateways.Humidity])), .. demo];
        }

        private static string DerivedCode(string seed)
        {
            var hash = (uint)StableHash.Of(seed);
            var extra = (uint)StableHash.Of(seed + "/2");
            var chars = new char[8];
            for (var i = 0; i < 8; i++)
            {
                var source = i < 4 ? hash >> (i * 8) : extra >> ((i - 4) * 8);
                chars[i] = CodeAlphabet[(int)(source & 0xFF) % CodeAlphabet.Length];
            }

            return PairingCode.Format(new string(chars));
        }
    }

    // Párovací heslo a kód čidla: na nálepce „XXXX-XXXX“, uživatel ho může opsat s mezerami nebo malými písmeny
    public static class PairingCode
    {
        public static string Normalize(string? code) => new((code ?? string.Empty).Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

        public static string Format(string code)
        {
            var normalized = Normalize(code);
            return normalized.Length == 8 ? $"{normalized[..4]}-{normalized[4..]}" : normalized;
        }

        public static bool Matches(string expected, string? entered) => Normalize(expected) == Normalize(entered);
    }
}
