using System.Text.RegularExpressions;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    public record ContractRecord(string ContractId, string Title, ContractCategory Category, DateOnly? DateSigned);

    // Co o budově říkají smlouvy organizace v Registru smluv: proběhlo zateplení / výměna oken, modernizace zdroje tepla,
    // instalace vzduchotechniky s rekuperací, a má organizace platnou servisní smlouvu na měření a regulaci (MaR) a kotelnu?
    // Hledá se v předmětu smlouvy — u každého příznaku si pamatujeme smlouvu, kterou lze v UI proklikem ověřit.
    public record ContractEvidence(
        bool RegistryKnown,
        ContractRecord? EnvelopeRetrofit,
        ContractRecord? HeatingModernization,
        ContractRecord? Ventilation,
        ContractRecord? ControlService)
    {
        private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        private static readonly Regex Envelope = new(@"zatepl|výměn\w*\s+(?:\w+\s+)?oken|okenní|výplní\s+otvor|energetick\w*\s+náročnost|snížení\s+energetick|fasád", Options);
        private static readonly Regex Heating = new(@"(?:rekonstrukc|výměn|moderniz|instalac)\w*\s+(?:\w+\s+){0,2}(?:kotel|kotl|zdroj\w*\s+tepla|předávací)|tepeln\w*\s+čerpadl|\bepc\b|energetick\w*\s+služb|garantovan\w*\s+úspor|kogenerac", Options);
        private static readonly Regex Air = new(@"vzduchotechn|rekuperac|\bvzt\b|větrac\w*\s+jednot", Options);
        // Provozní smlouva na vytápění: MaR, servis / revize / opravy kotelny a otopné soustavy, energetický management
        // (ContractClassifier už do kategorie FacilityManagement pouští jen smlouvy o vytápění a technice budovy)
        private static readonly Regex Control = new(@"měření\s+a\s+regulac|\bmar\b|regulac|servis|revize|oprav|údržb|kotl|kotel|vytáp|ústřední|otopn|energetick\w*\s+management|termostat|dálkov\w*\s+(?:správ|monitor)", Options);

        // Starší smlouvy než 15 let o současném stavu techniky nic nevypovídají (servisní smlouvy jen 6 let)
        private const int RetrofitYears = 15;
        private const int ServiceYears = 6;

        public static ContractEvidence From(IReadOnlyCollection<ContractRecord> contracts, bool registryKnown, DateOnly today)
        {
            ContractRecord? Latest(Regex pattern, int years, params ContractCategory[] categories) => contracts
                .Where(c => categories.Contains(c.Category) && pattern.IsMatch(c.Title))
                .Where(c => c.DateSigned == null || c.DateSigned >= today.AddYears(-years))
                .OrderByDescending(c => c.DateSigned)
                .FirstOrDefault();

            return new ContractEvidence(
                registryKnown || contracts.Count > 0,
                Latest(Envelope, RetrofitYears, ContractCategory.Reconstruction, ContractCategory.Other),
                Latest(Heating, RetrofitYears, ContractCategory.Reconstruction, ContractCategory.FacilityManagement),
                Latest(Air, RetrofitYears, ContractCategory.Reconstruction, ContractCategory.FacilityManagement),
                Latest(Control, ServiceYears, ContractCategory.FacilityManagement, ContractCategory.Reconstruction));
        }
    }
}
