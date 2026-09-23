using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    public record ZoneTemplate(string Name, ZoneType Type, int Floor, string Wing, bool IsHotspot);

    // Schematická dispozice pro řez budovou (F21). Otevřená data vnitřní dispozici neznají,
    // takže se generuje z druhu zařízení a odhadu podlaží — v UI se musí označit jako schéma.
    public static class ZoneLayout
    {
        private const int MaxZones = 6;

        public static IReadOnlyList<ZoneTemplate> Create(FacilityCategory category, SimulationProfile profile, string[] kinds, string name, int floors, int seed)
        {
            floors = Math.Clamp(floors, 1, 4);

            var zones = category switch
            {
                FacilityCategory.School => School(kinds, name, floors, seed),
                FacilityCategory.Social => Social(floors),
                FacilityCategory.Health => Health(floors),
                _ => Culture(name, floors),
            };

            zones = zones.Take(MaxZones).ToList();

            return MarkHotspot(zones, category, profile);
        }

        private static List<ZoneTemplate> School(string[] kinds, string name, int floors, int seed)
        {
            var zones = new List<ZoneTemplate>();
            var mainKind = kinds.FirstOrDefault() ?? string.Empty;

            if (mainKind is "Domov mládeže" or "Internát")
            {
                for (var floor = 1; floor <= Math.Max(1, floors - 1); floor++)
                    zones.Add(new ZoneTemplate($"Pokoje {floor}. patro", ZoneType.Dormitory, floor, "hlavní budova", false));

                zones.Add(new ZoneTemplate("Klubovna", ZoneType.CommonRoom, 0, "hlavní budova", false));

                return zones;
            }

            var wings = floors >= 2 ? new[] { "severní křídlo", "jižní křídlo" } : new[] { "hlavní budova" };
            var isKindergarten = mainKind == "Mateřská škola";

            if (!isKindergarten)
                zones.Add(new ZoneTemplate("Jídelna", ZoneType.Canteen, 0, wings[0], false));

            var technical = name.Contains("techn", StringComparison.OrdinalIgnoreCase)
                || name.Contains("průmysl", StringComparison.OrdinalIgnoreCase)
                || name.Contains("řemesl", StringComparison.OrdinalIgnoreCase);

            if (technical)
                zones.Add(new ZoneTemplate("Dílny", ZoneType.Workshop, 0, wings[^1], false));
            else if (!isKindergarten && Math.Abs(seed) % 2 == 0)
                zones.Add(new ZoneTemplate("Tělocvična", ZoneType.Gym, 0, wings[^1], false));

            // Učebny od nejvyššího patra dolů, ať se do limitu zón vejde celý řez
            for (var floor = Math.Max(1, floors - 1); floor >= (floors == 1 ? 0 : 1); floor--)
            {
                foreach (var wing in wings)
                {
                    var room = wing == "jižní křídlo" ? 3 : 1;
                    var label = isKindergarten ? $"Třída {floor + 1}{(wing == "jižní křídlo" ? "B" : "A")}" : $"Učebna {floor}.{room:00}";
                    zones.Add(new ZoneTemplate(label, ZoneType.Classroom, floor, wing, false));
                }
            }

            return zones;
        }

        private static List<ZoneTemplate> Social(int floors)
        {
            var zones = new List<ZoneTemplate>
            {
                new("Společenská místnost", ZoneType.CommonRoom, 0, "hlavní budova", false),
                new("Jídelna", ZoneType.Canteen, 0, "hlavní budova", false),
            };

            for (var floor = 1; floor <= Math.Max(1, floors - 1) && floor <= 3; floor++)
                zones.Add(new ZoneTemplate($"Pokoje {floor}. patro", ZoneType.ResidentRoom, floor, "hlavní budova", false));

            return zones;
        }

        private static List<ZoneTemplate> Health(int floors)
        {
            var zones = new List<ZoneTemplate> { new("Ambulance", ZoneType.Office, 0, "pavilon A", false) };
            var wards = new[] { "A", "B", "C" };

            for (var i = 0; i < Math.Min(3, Math.Max(1, floors - 1)); i++)
                zones.Add(new ZoneTemplate($"Lůžkové oddělení {wards[i]}", ZoneType.Ward, i + 1, "pavilon A", false));

            return zones;
        }

        private static List<ZoneTemplate> Culture(string name, int floors)
        {
            var zones = new List<ZoneTemplate> { new("Kanceláře", ZoneType.Office, 0, "hlavní budova", false) };

            if (name.Contains("divadl", StringComparison.OrdinalIgnoreCase) || name.Contains("filharmon", StringComparison.OrdinalIgnoreCase) || name.Contains("Centrum uměleckých", StringComparison.OrdinalIgnoreCase))
                zones.Add(new ZoneTemplate("Sál", ZoneType.Hall, 0, "hlavní budova", false));

            var hallName = name.Contains("knihovn", StringComparison.OrdinalIgnoreCase) ? "Studovna" : "Expozice";

            for (var floor = 1; floor <= Math.Max(1, floors - 1) && floor <= 2; floor++)
                zones.Add(new ZoneTemplate($"{hallName} {floor}. patro", ZoneType.Exhibition, floor, "hlavní budova", false));

            return zones;
        }

        // Problém profilu se projeví v jedné konkrétní zóně — tak, jak by to ukázal reálný senzor
        private static List<ZoneTemplate> MarkHotspot(List<ZoneTemplate> zones, FacilityCategory category, SimulationProfile profile)
        {
            if (profile != SimulationProfile.PoorVentilation || zones.Count == 0)
                return zones;

            var preferredType = category switch
            {
                FacilityCategory.School => ZoneType.Classroom,
                FacilityCategory.Social => ZoneType.CommonRoom,
                FacilityCategory.Health => ZoneType.Ward,
                _ => ZoneType.Exhibition,
            };

            var hotspot = zones
                .Where(z => z.Type == preferredType || z.Type == ZoneType.Dormitory)
                .OrderByDescending(z => z.Wing == "jižní křídlo")
                .ThenByDescending(z => z.Floor)
                .FirstOrDefault() ?? zones[0];

            return zones.Select(z => z == hotspot ? z with { IsHotspot = true } : z).ToList();
        }
    }
}
