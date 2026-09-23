using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    public class DataImport
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Průběh importu
        public ImportStatus Status { get; private set; } = ImportStatus.Running;
        public int RecordCount { get; private set; }
        public string? Error { get; private set; }


        // Cizí klíče
        public Guid DataSourceId { get; private set; }


        // Časové údaje
        public DateTimeOffset StartedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? FinishedAt { get; private set; }


        // Navigační vlastnosti
        public DataSource DataSource { get; set; } = null!;

        private DataImport() { }

        public DataImport(Guid dataSourceId)
        {
            DataSourceId = dataSourceId;
            StartedAt = DateTimeOffset.UtcNow;
        }

        public void Finish(ImportStatus status, int recordCount, string? error = null)
        {
            if (recordCount < 0)
                throw new ArgumentException("Počet záznamů nesmí být záporný.", nameof(recordCount));

            Status = status;
            RecordCount = recordCount;
            Error = error;
            FinishedAt = DateTimeOffset.UtcNow;
        }
    }
}
