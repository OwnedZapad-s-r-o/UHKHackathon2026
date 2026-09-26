using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Senzor virtuálního uzlu: typ IQRF Standard Sensor, id a název jako ve skutečném daemonu
    public record VirtualSensor(int Type, string Id, string Name, string Unit);

    // Uzel IQMESH: adresa, co měří a do jakého druhu místnosti patří (pro předem přiřazené sítě).
    // Overheats = místnost se topí i mimo provoz (demo zařízení s chováním „přetápí“), jinak drží útlum.
    public record VirtualNode(int Address, string Label, ZoneType PreferredZone, IReadOnlyList<VirtualSensor> Sensors, bool Overheats = false);

    // Provoz budovy určuje průběh hodnot: škola (vyučovací hodiny a přestávky), dílna (směny, hluk)
    public enum VirtualProfile
    {
        School,
        Workshop,
    }

    public record VirtualGatewayPreset(string GatewayId, string FacilityCode, string Name, VirtualProfile Profile, bool Preassigned, IReadOnlyList<VirtualNode> Nodes);

    // Testovací síť: pět virtuálních IQRF gatewayí v pěti okresech kraje. Čtyři jsou předem přiřazené do místností
    // (živá data hned v mapě), rychnovská je nová a nepřiřazená — na ní si uživatel vyzkouší přiřazení uzlů.
    // Stejné rozložení má Python cluster tools/iqrf-emulator/multi_emulator.py.
    public static class VirtualGateways
    {
        public static readonly VirtualSensor Temperature = new(1, "TEMPERATURE", "Temperature", "°C");
        public static readonly VirtualSensor Co2 = new(2, "CARBON_DIOXIDE", "Carbon dioxide", "ppm");
        public static readonly VirtualSensor Voc = new(3, "VOLATILE_ORGANIC_COMPOUND", "Volatile organic compounds", "ppm");
        public static readonly VirtualSensor Illuminance = new(11, "ILLUMINANCE", "Illuminance", "lx");
        public static readonly VirtualSensor Noise = new(19, "SOUND_PRESSURE_LEVEL", "Sound pressure level", "dB");
        public static readonly VirtualSensor Humidity = new(128, "RELATIVE_HUMIDITY", "Relative humidity", "%");
        public static readonly VirtualSensor Motion = new(129, "BINARYDATA7", "Binary data7", "");

        public static readonly IReadOnlyList<VirtualGatewayPreset> Network =
        [
            new("gw-hk-tyl", "SKO-5205-12", "Gymnázium J. K. Tyla — testovací gateway", VirtualProfile.School, true,
            [
                new(1, "Učebna", ZoneType.Classroom, [Temperature, Co2, Humidity]),
                new(2, "Společné prostory", ZoneType.Gym, [Temperature, Illuminance, Motion]),
            ]),
            new("gw-jc-moa", "SKO-5207-04", "Masarykova obchodní akademie Jičín — testovací gateway", VirtualProfile.School, true,
            [
                new(1, "Počítačová učebna", ZoneType.Classroom, [Co2, Temperature]),
                new(2, "Chodba", ZoneType.Canteen, [Noise, Motion]),
            ]),
            new("gw-tu-gym", "SKO-5214-06", "Gymnázium Trutnov — testovací gateway", VirtualProfile.School, true,
            [
                new(1, "Laboratoř", ZoneType.Classroom, [Voc, Co2]),
                new(2, "Sborovna", ZoneType.Office, [Temperature, Humidity]),
            ]),
            new("gw-na-jir", "SKO-5209-04", "Jiráskovo gymnázium Náchod — testovací gateway", VirtualProfile.School, true,
            [
                new(1, "Knihovna", ZoneType.Classroom, [Humidity, Temperature, Illuminance]),
                new(2, "Jídelna", ZoneType.Canteen, [Co2, Temperature]),
            ]),
            new("gw-rk-sps", "SKO-5213-05", "VOŠ a SPŠ Rychnov nad Kněžnou — testovací gateway", VirtualProfile.Workshop, false,
            [
                new(1, "Strojní dílna", ZoneType.Workshop, [Temperature, Noise]),
                new(2, "Učebna automatizace", ZoneType.Classroom, [Temperature, Co2, Humidity]),
            ]),
        ];

        // „Virtuální učebna“ pro jednoklikové demo ve formuláři připojení — nepřiřazená, přiřazení si uživatel zkusí sám
        public static readonly IReadOnlyList<VirtualNode> Classroom =
        [
            new(1, "Učebna", ZoneType.Classroom, [Temperature, Co2, Humidity]),
            new(2, "Chodba", ZoneType.Canteen, [Temperature, Illuminance, Motion]),
        ];

        public static VirtualGatewayPreset? Find(string gatewayId) => Network.FirstOrDefault(p => p.GatewayId == gatewayId);

        // Uzly virtuální gatewaye: testovací síť podle katalogu, spárovaná gateway z výroby podle svého katalogu,
        // ostatní (jednoklikové demo) jako učebna. K nim čidla z dosahu, která uživatel přidal do sítě (bonded = adresy v síti).
        public static IReadOnlyList<VirtualNode> NodesOf(string gatewayId, IReadOnlyCollection<int>? bonded = null)
        {
            var nodes = Find(gatewayId)?.Nodes ?? PairableDevices.FindByGatewayId(gatewayId)?.Nodes ?? Classroom;
            if (bonded == null || bonded.Count == 0)
                return nodes;

            return [.. nodes, .. PairableDevices.NearbyOf(gatewayId).Where(n => bonded.Contains(n.Node.Address)).Select(n => n.Node)];
        }

        public static VirtualProfile ProfileOf(string gatewayId) => Find(gatewayId)?.Profile ?? VirtualProfile.School;

        // Hodnota senzoru v čase: plynulé průběhy podle provozu budovy + mírný šum, deterministické podle gatewaye a uzlu,
        // aby každá budova vypadala jinak. Čas je místní (vyučování, noční útlum).
        public static double Value(string gatewayId, VirtualProfile profile, int node, int sensorType, DateTimeOffset now, bool overheats = false)
        {
            var local = SchoolCalendar.ToLocal(now);
            var seed = StableHash.Of($"{gatewayId}/{node}");
            var phase = seed % 1000 / 1000.0 * 2 * Math.PI;
            var minutes = local.TimeOfDay.TotalMinutes;
            var workday = !SchoolCalendar.IsFreeDay(FacilityCategory.School, DateOnly.FromDateTime(local));
            var occupied = workday && local.Hour is >= 7 and < 16;
            var noise = Math.Sin(now.ToUnixTimeSeconds() / 37.0 + phase) * 0.5 + Math.Sin(now.ToUnixTimeSeconds() / 11.0 + phase * 2) * 0.3;

            // Vyučovací hodina 45 min, přestávka 10 min — CO₂ ve třídě roste, o přestávce se vyvětrá
            var lessonMinute = (minutes - 8 * 60 + 1000 * 55) % 55;
            var inLesson = occupied && lessonMinute < 45;

            // Dílna: dvě směny s pauzou na oběd
            var shift = profile == VirtualProfile.Workshop && workday && (local.Hour is >= 6 and < 11 || local.Hour is >= 12 and < 15);

            return sensorType switch
            {
                // Teplota: přes den komfort, v noci a o víkendu útlum — nebo bez útlumu, když místnost přetápí
                1 => Math.Round((occupied ? 21.2 : overheats ? 22.3 : 18.4) + 0.6 * Math.Sin(minutes / 180.0 + phase) + 0.15 * noise, 2),
                2 => Math.Round(occupied ? (inLesson ? 620 + lessonMinute * 24 : 560) + 40 * noise : 430 + 20 * noise),
                3 => Math.Round((occupied ? 0.45 : 0.12) + 0.08 * noise, 2),
                11 => Math.Round(Math.Max(0, (occupied ? 380 : 20) + 60 * Math.Sin(minutes / 90.0 + phase) + 10 * noise)),
                19 => Math.Round((shift ? 78 : occupied ? 55 : 32) + 4 * noise, 1),
                128 => Math.Round(43 + 4 * Math.Sin(minutes / 240.0 + phase) + noise, 1),
                129 => occupied && noise > -0.2 ? 1 : 0,
                _ => 0,
            };
        }
    }
}
