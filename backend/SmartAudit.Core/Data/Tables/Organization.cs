namespace SmartAudit.Core.Data.Tables
{
    // Právní subjekt — příspěvková organizace kraje, ředitelství školy, poskytovatel služby
    public class Organization
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Identifikace
        public string Ico { get; private set; } = string.Empty;
        public string? Redizo { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? LegalForm { get; private set; }


        // Vztah ke kraji (sada Organizace Královéhradeckého kraje)
        public string? Department { get; private set; }
        public string? RelationType { get; private set; }
        public string? Web { get; private set; }
        public string? PhotoUrl { get; private set; }


        // Kdy se naposledy stáhly smlouvy z Registru smluv (null = ještě nikdy)
        public DateTimeOffset? ContractsSyncedAt { get; private set; }


        // Navigační vlastnosti
        public ICollection<Facility> Facilities { get; private set; } = new List<Facility>();
        public ICollection<FacilityContract> Contracts { get; private set; } = new List<FacilityContract>();

        private Organization() { }

        public Organization(string ico, string name, string? legalForm = null)
        {
            if (string.IsNullOrWhiteSpace(ico))
                throw new ArgumentException("IČO organizace nesmí být prázdné.", nameof(ico));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Název organizace nesmí být prázdný.", nameof(name));

            Ico = ico;
            Name = name;
            LegalForm = legalForm;
        }

        public void ApplySchoolRegistry(string? redizo, string? legalForm)
        {
            Redizo = redizo ?? Redizo;
            LegalForm = legalForm ?? LegalForm;
        }

        public void ApplyRegionRegistry(string name, string? department, string? relationType, string? web, string? photoUrl)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Název organizace nesmí být prázdný.", nameof(name));

            Name = name;
            Department = department;
            RelationType = relationType;
            Web = web ?? Web;
            PhotoUrl = photoUrl;
        }

        public void MarkContractsSynced(DateTimeOffset syncedAt)
        {
            ContractsSyncedAt = syncedAt;
        }
    }
}
