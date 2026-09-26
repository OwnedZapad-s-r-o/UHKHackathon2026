using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    // Smlouva organizace z Registru smluv (zákon č. 340/2015 Sb.) — jen smlouvy o energiích a provozu budov
    public class FacilityContract
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Údaje z Registru smluv — ContractId je stálé ID smlouvy, VersionId poslední verze (adresa záznamu)
        public string ContractId { get; private set; } = string.Empty;
        public string VersionId { get; private set; } = string.Empty;
        public string? ContractNumber { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string PartnerName { get; private set; } = string.Empty;
        public string? PartnerIco { get; private set; }
        public decimal? AmountWithoutVat { get; private set; }
        public decimal? AmountWithVat { get; private set; }
        public DateOnly? DateSigned { get; private set; }
        public DateOnly PublishedOn { get; private set; }
        public ContractCategory Category { get; private set; }

        // Přílohy smlouvy (PDF) — [{ id, name, url }], stahují se přes API, ne přímo z registru
        public string AttachmentsJson { get; private set; } = "[]";


        // Jednotková cena energie vytažená z textu smlouvy (Kč/MWh) — vstup výpočtu ztráty místo paušální ceny
        public EnergyCommodity? Commodity { get; private set; }
        public double? UnitPriceCzkPerMwh { get; private set; }
        public string? PriceSource { get; private set; }


        // Cizí klíče
        public Guid OrganizationId { get; private set; }


        // Časové údaje
        public DateTimeOffset SyncedAt { get; private set; } = DateTimeOffset.UtcNow;


        // Navigační vlastnosti
        public Organization Organization { get; private set; } = null!;

        private FacilityContract() { }

        public FacilityContract(Guid organizationId, string contractId)
        {
            if (string.IsNullOrWhiteSpace(contractId))
                throw new ArgumentException("ID smlouvy nesmí být prázdné.", nameof(contractId));

            OrganizationId = organizationId;
            ContractId = contractId;
        }

        public void ApplyRegistry(string versionId, string? contractNumber, string title, string partnerName, string? partnerIco, decimal? amountWithoutVat, decimal? amountWithVat, DateOnly? dateSigned, DateOnly publishedOn, ContractCategory category, string attachmentsJson)
        {
            if (string.IsNullOrWhiteSpace(versionId))
                throw new ArgumentException("ID verze smlouvy nesmí být prázdné.", nameof(versionId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Předmět smlouvy nesmí být prázdný.", nameof(title));

            VersionId = versionId;
            ContractNumber = contractNumber;
            Title = title;
            PartnerName = partnerName;
            PartnerIco = partnerIco;
            AmountWithoutVat = amountWithoutVat;
            AmountWithVat = amountWithVat;
            DateSigned = dateSigned;
            PublishedOn = publishedOn;
            Category = category;
            AttachmentsJson = attachmentsJson;
            SyncedAt = DateTimeOffset.UtcNow;
        }

        public void SetEnergyPrice(EnergyCommodity? commodity, double? unitPriceCzkPerMwh, string? priceSource)
        {
            if (unitPriceCzkPerMwh is <= 0)
                throw new ArgumentException("Cena energie musí být kladná.", nameof(unitPriceCzkPerMwh));

            Commodity = commodity;
            UnitPriceCzkPerMwh = unitPriceCzkPerMwh;
            PriceSource = priceSource;
        }
    }
}
