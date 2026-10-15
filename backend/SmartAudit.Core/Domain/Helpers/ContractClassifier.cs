using System.Text.RegularExpressions;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Třídění smluv z Registru smluv podle předmětu. Registr hledá podřetězec bez ohledu na velikost písmen,
    // ale s diakritikou — proto kmeny slov (SearchStems) a teprve pak přesné třídění tady.
    public static class ContractClassifier
    {
        // Dotazy do registru na jednu organizaci — široké, šum odfiltruje Classify
        public static readonly string[] SearchStems =
        [
            "energ", "elektřin", "elektrick", "plyn", "tepl", "tepel", "kotl", "kotel", "regulac",
            "vytáp", "topen", "otop", "oken", "okna", "fotovolt", "FVE", "vzduchotechn", "rekuperac",
        ];

        private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        // Slova, která se s energií jen potkávají v názvu (klest „pro energetické využití“, elektronika, odpad, vozidla)
        private static readonly Regex Noise = new(
            @"klest|štěpk|elektronick|elektrocentr|odpad|stravov|vozid|autobus|traktor|pneu",
            Options);

        private static readonly Regex EnergySupply = new(
            @"dodávk\w*\s+(?:a\s+distribuc\w*\s+)?(?:elektrick\w*\s+energi|elektřin|zemního\s+plynu|plynu|tepl|tepeln\w*\s+energi)|sdružen\w*\s+služb\w*\s+dodávk|nákup\w*\s+(?:elektřin|zemního\s+plynu|plynu|tepl)|odběr\w*\s+tepl|smlouv\w*\s+o\s+dodávce\s+(?:tepl|plynu|elektřin)|\bteplárn",
            Options);

        private static readonly Regex Reconstruction = new(
            @"energetick\w*\s+náročnost|zatepl|výměn\w*\s+(?:\w+\s+)?oken|okenní|výplní\s+otvor|fotovolt|\bfve\b|tepeln\w*\s+čerpadl|rekonstrukc\w*\s+(?:\w+\s+)?kotel|výměn\w*\s+(?:\w+\s+)?kotl|rekuperac|vzduchotechni|solární|úsporn\w*\s+osvětlen|\bled\b|epc\b|energetick\w*\s+služb|garantovan\w*\s+úspor",
            Options);

        private static readonly Regex FacilityManagement = new(
            @"kotl|kotel|měření\s+a\s+regulac|\bmar\b|regulac|vytápěn|ústřední\w*\s+topen|topení|\but\b|otopn|teplovod|teplé\s+vody|revize\s+(?:plyn|kotl|tlak|komín)|komín|klimatiz|termostat|předávací\w*\s+stanic|výměník|plynov\w*\s+zaříz|energetick\w*\s+(?:audit|posud|management)|průkaz\w*\s+energetick",
            Options);

        private static readonly Regex Heat = new(@"tepl|tepeln|teplárn", Options);
        private static readonly Regex Gas = new(@"plyn", Options);
        private static readonly Regex Electricity = new(@"elektřin|elektrick\w*\s+energi", Options);

        // null = smlouva s energií a provozem budovy nesouvisí
        public static ContractCategory? Classify(string title)
        {
            if (string.IsNullOrWhiteSpace(title) || Noise.IsMatch(title))
                return null;

            if (EnergySupply.IsMatch(title))
                return ContractCategory.EnergySupply;

            if (Reconstruction.IsMatch(title))
                return ContractCategory.Reconstruction;

            if (FacilityManagement.IsMatch(title))
                return ContractCategory.FacilityManagement;

            return null;
        }

        public static EnergyCommodity? DetectCommodity(string title)
        {
            if (Heat.IsMatch(title))
                return EnergyCommodity.Heat;

            if (Gas.IsMatch(title))
                return EnergyCommodity.Gas;

            if (Electricity.IsMatch(title))
                return EnergyCommodity.Electricity;

            return null;
        }
    }
}
