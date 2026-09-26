using System.Globalization;
using System.Text.RegularExpressions;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    public record ExtractedEnergyPrice(double CzkPerMwh, string Snippet);

    public record EffectiveHeatPrice(decimal CzkPerKwh, bool FromContract, Guid? ContractId);

    public record ContractPrice(Guid ContractId, EnergyCommodity? Commodity, double? UnitPriceCzkPerMwh, DateOnly? DateSigned);

    // Jednotková cena energie z textu smlouvy („2 450 Kč/MWh“, „612,40 Kč/GJ“) a výběr ceny tepla pro výpočet ztráty
    public static class EnergyPriceExtractor
    {
        // 1 GJ = 0,2778 MWh → cena za MWh = cena za GJ × 3,6
        private const double GigajoulesPerMwh = 3.6;

        // Účinnost plynového kotle — cena tepla z plynu = cena plynu / účinnost
        private const double GasBoilerEfficiency = 0.9;

        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");

        private static readonly Regex Price = new(
            @"(?<value>\d{1,3}(?:[  .]\d{3})+(?:,\d{1,4})?|\d+(?:,\d{1,4})?)\s*(?:,-\s*)?(?:Kč|CZK)\s*(?:bez\s+DPH\s*)?(?:/|za\s+1?\s*)\s*(?<unit>GJ|MWh|kWh)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // Rozumné rozpětí dnešních cen v Kč/MWh — mimo něj jde nejspíš o jiné číslo (poplatek, jistina, sazba za měřidlo)
        // nebo o starý ceník: smlouvy na teplo mívají v přílohách dodatky od roku 2002 za 200–300 Kč/GJ
        private static readonly Dictionary<EnergyCommodity, (double Min, double Max)> Plausible = new()
        {
            [EnergyCommodity.Heat] = (1400, 9000),
            [EnergyCommodity.Gas] = (400, 6000),
            [EnergyCommodity.Electricity] = (800, 15000),
        };

        // Nejčastější věrohodná cena v textu (cenová ujednání se v textu opakují, jednorázová čísla ne)
        public static ExtractedEnergyPrice? Extract(string text, EnergyCommodity commodity)
        {
            var (min, max) = Plausible[commodity];

            var candidates = Price.Matches(text)
                .Select(m => (Value: ToCzkPerMwh(m.Groups["value"].Value, m.Groups["unit"].Value), Snippet: m.Value))
                .Where(c => c.Value is { } value && value >= min && value <= max)
                .GroupBy(c => c.Value!.Value)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .FirstOrDefault();

            return candidates == null ? null : new ExtractedEnergyPrice(Math.Round(candidates.Key, 2), candidates.First().Snippet.Trim());
        }

        // Cena tepla pro výpočet ztráty: nejnovější smlouva na teplo, jinak na plyn (přes účinnost kotle), jinak paušál.
        // Cena elektřiny se nepoužije — ztráta z přetápění je teplo.
        public static EffectiveHeatPrice SelectHeatPrice(IEnumerable<ContractPrice> contracts, decimal defaultCzkPerKwh)
        {
            var priced = contracts
                .Where(c => c.UnitPriceCzkPerMwh is > 0 && c.Commodity is EnergyCommodity.Heat or EnergyCommodity.Gas)
                .OrderBy(c => c.Commodity == EnergyCommodity.Heat ? 0 : 1)
                .ThenByDescending(c => c.DateSigned)
                .FirstOrDefault();

            if (priced == null)
                return new EffectiveHeatPrice(defaultCzkPerKwh, false, null);

            var perMwh = priced.Commodity == EnergyCommodity.Gas ? priced.UnitPriceCzkPerMwh!.Value / GasBoilerEfficiency : priced.UnitPriceCzkPerMwh!.Value;

            return new EffectiveHeatPrice(Math.Round((decimal)perMwh / 1000m, 3), true, priced.ContractId);
        }

        private static double? ToCzkPerMwh(string value, string unit)
        {
            var normalized = value.Replace(" ", "").Replace(" ", "").Replace(".", "");
            if (!double.TryParse(normalized, NumberStyles.Number, Cs, out var price))
                return null;

            return unit.ToLowerInvariant() switch
            {
                "gj" => price * GigajoulesPerMwh,
                "kwh" => price * 1000,
                _ => price,
            };
        }
    }
}
