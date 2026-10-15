namespace SmartAudit.Core.Domain.Settings
{
    public class OpenDataSettings
    {
        public const string SectionName = "OpenDataSettings";

        // Složka se snapshoty pro start bez sítě
        public string SnapshotPath { get; set; } = "../../seed/opendata";

        // Po úspěšném stažení přepsat snapshot aktuálními daty
        public bool WriteSnapshots { get; set; } = true;

        // Stáhnout data i tehdy, když už v databázi jsou
        public bool RefreshOnStartup { get; set; }

        public int CuzkParallelism { get; set; } = 4;

        public List<DatasetSettings> Datasets { get; set; } = new();
    }

    public class DatasetSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Provider { get; set; } = "Data KHK";
        public string? DatasetId { get; set; }
        public string ServiceUrl { get; set; } = string.Empty;
        public string? PortalUrl { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Where { get; set; } = "1=1";
    }
}
