namespace SmartAudit.Core.Domain.Helpers
{
    // Klimatická náročnost lokality: denostupně topné sezóny (D = Σ (19 °C − t_e) za dny s t_e < 13 °C) podle nadmořské výšky.
    // Výšku vrací Open-Meteo Elevation API pro těžiště ORP; bez sítě se použije výška správního centra ORP.
    // Kalibrace: Hradec Králové (~235 m) ≈ 3 400 K·den, Vrchlabí (~480 m) ≈ 4 250 K·den (ČSN 38 3350, TNI 73 0331) —
    // na každých 100 m výšky ≈ +0,55 °C chladněji a delší topná sezóna ≈ +340 K·den.
    public static class ClimateModel
    {
        public const double ReferenceDegreeDays = 3400;
        private const double ReferenceElevation = 235;
        private const double DegreeDaysPer100m = 340;

        // Výška správního centra ORP (m n. m.) — náhrada, když Open-Meteo není dostupné
        private static readonly Dictionary<string, double> OrpCentreElevation = new()
        {
            ["5201"] = 400, // Broumov
            ["5202"] = 290, // Dobruška
            ["5203"] = 300, // Dvůr Králové nad Labem
            ["5204"] = 315, // Hořice
            ["5205"] = 235, // Hradec Králové
            ["5206"] = 255, // Jaroměř
            ["5207"] = 285, // Jičín
            ["5208"] = 290, // Kostelec nad Orlicí
            ["5209"] = 345, // Náchod
            ["5210"] = 430, // Nová Paka
            ["5211"] = 340, // Nové Město nad Metují
            ["5212"] = 235, // Nový Bydžov
            ["5213"] = 320, // Rychnov nad Kněžnou
            ["5214"] = 415, // Trutnov
            ["5215"] = 480, // Vrchlabí
        };

        public static double? FallbackElevation(string orpCode) => OrpCentreElevation.TryGetValue(orpCode, out var elevation) ? elevation : null;

        public static double DegreeDays(double elevation)
        {
            return Math.Round(ReferenceDegreeDays + (elevation - ReferenceElevation) / 100 * DegreeDaysPer100m);
        }

        // Roční potřeba tepla budovy v kWh: H [W/K] × D [K·den] × 24 h / 1000
        public static double AnnualHeatDemandKwh(double heatLossWattsPerKelvin, double degreeDays)
        {
            return heatLossWattsPerKelvin * degreeDays * 24 / 1000;
        }
    }
}
