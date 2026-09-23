using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    public class DataSource
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Popis zdroje
        public string Key { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public string Provider { get; private set; } = string.Empty;
        public string? DatasetId { get; private set; }
        public string ServiceUrl { get; private set; } = string.Empty;
        public string? PortalUrl { get; private set; }
        public string Group { get; private set; } = string.Empty;
        public string Purpose { get; private set; } = string.Empty;


        // Poslední import
        public DateTimeOffset? LastImportedAt { get; private set; }
        public int? LastRecordCount { get; private set; }
        public ImportStatus? LastStatus { get; private set; }


        // Navigační vlastnosti
        public ICollection<DataImport> Imports { get; private set; } = new List<DataImport>();

        private DataSource() { }

        public DataSource(string key, string title, string provider, string serviceUrl, string group, string purpose, string? datasetId = null, string? portalUrl = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Klíč zdroje dat nesmí být prázdný.", nameof(key));

            Key = key;
            Update(title, provider, serviceUrl, group, purpose, datasetId, portalUrl);
        }

        public void Update(string title, string provider, string serviceUrl, string group, string purpose, string? datasetId = null, string? portalUrl = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Název zdroje dat nesmí být prázdný.", nameof(title));

            Title = title;
            Provider = provider;
            ServiceUrl = serviceUrl;
            Group = group;
            Purpose = purpose;
            DatasetId = datasetId;
            PortalUrl = portalUrl;
        }

        public void MarkImported(ImportStatus status, int recordCount)
        {
            LastStatus = status;
            LastRecordCount = recordCount;
            LastImportedAt = DateTimeOffset.UtcNow;
        }
    }
}
