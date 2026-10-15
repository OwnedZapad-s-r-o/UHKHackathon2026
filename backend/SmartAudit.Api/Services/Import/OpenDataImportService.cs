using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Import
{
    public record DatasetImportResult(string Key, ImportStatus Status, int RecordCount, string? Error);

    public interface IOpenDataImportService
    {
        Task SyncDataSourcesAsync(CancellationToken ct);
        Task<DatasetImportResult> ImportDatasetAsync(string key, CancellationToken ct);
        Task<IReadOnlyList<DatasetImportResult>> ImportAllAsync(CancellationToken ct);
        Task<IReadOnlyList<DatasetImportResult>> ImportMissingAsync(CancellationToken ct);
        Task<IReadOnlyList<RawFeature>> LoadAsync(string key, CancellationToken ct);
        Task RecordExternalImportAsync(string key, ImportStatus status, int recordCount, string? error, CancellationToken ct);
    }
    public class OpenDataImportService : IOpenDataImportService
    {
        public const string KhkProvider = "Data KHK";

        private readonly SmartAuditDbContext _dbContext;
        private readonly IKhkOpenDataService _openDataService;
        private readonly OpenDataSettings _settings;
        private readonly SnapshotStore _snapshots;
        private readonly ILogger<OpenDataImportService> _logger;

        public OpenDataImportService(SmartAuditDbContext dbContext, IKhkOpenDataService openDataService, OpenDataSettings settings, SnapshotStore snapshots, ILogger<OpenDataImportService> logger)
        {
            _dbContext = dbContext;
            _openDataService = openDataService;
            _settings = settings;
            _snapshots = snapshots;
            _logger = logger;
        }

        // Katalog zdrojů v DB = konfigurace; panel „Zdroje dat“ se generuje z něj
        public async Task SyncDataSourcesAsync(CancellationToken ct)
        {
            var existing = await _dbContext.DataSources.ToDictionaryAsync(d => d.Key, ct);

            foreach (var dataset in _settings.Datasets)
            {
                if (existing.TryGetValue(dataset.Key, out var source))
                {
                    source.Update(dataset.Title, dataset.Provider, dataset.ServiceUrl, dataset.Group, dataset.Purpose, dataset.DatasetId, dataset.PortalUrl);
                    continue;
                }

                _dbContext.DataSources.Add(new DataSource(dataset.Key, dataset.Title, dataset.Provider, dataset.ServiceUrl, dataset.Group, dataset.Purpose, dataset.DatasetId, dataset.PortalUrl));
            }

            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<DatasetImportResult>> ImportAllAsync(CancellationToken ct)
        {
            var results = new List<DatasetImportResult>();

            foreach (var dataset in _settings.Datasets.Where(d => d.Provider == KhkProvider))
                results.Add(await ImportDatasetAsync(dataset.Key, ct));

            return results;
        }

        // Sady Data KHK, které ještě nikdy importované nebyly (nové v konfiguraci)
        public async Task<IReadOnlyList<DatasetImportResult>> ImportMissingAsync(CancellationToken ct)
        {
            var imported = await _dbContext.DataSources
                .Where(d => d.LastImportedAt != null)
                .Select(d => d.Key)
                .ToListAsync(ct);

            var results = new List<DatasetImportResult>();

            foreach (var dataset in _settings.Datasets.Where(d => d.Provider == KhkProvider && !imported.Contains(d.Key)))
                results.Add(await ImportDatasetAsync(dataset.Key, ct));

            return results;
        }

        public async Task<DatasetImportResult> ImportDatasetAsync(string key, CancellationToken ct)
        {
            var dataset = _settings.Datasets.FirstOrDefault(d => d.Key == key && d.Provider == KhkProvider)
                ?? throw new ArgumentException($"Datová sada {key} není v konfiguraci.", nameof(key));

            var source = await _dbContext.DataSources.FirstAsync(d => d.Key == key, ct);

            var import = new DataImport(source.Id);
            _dbContext.DataImports.Add(import);
            await _dbContext.SaveChangesAsync(ct);

            IReadOnlyList<OpenDataRecord> records;
            ImportStatus status;
            string? error = null;

            try
            {
                var download = await _openDataService.DownloadAsync(dataset.ServiceUrl, ct);
                records = download.Records;
                status = ImportStatus.Live;

                if (_settings.WriteSnapshots && records.Count > 0)
                    await _snapshots.WriteAsync($"{key}.geojson", download.FeatureCollectionJson, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException or JsonException)
            {
                _logger.LogWarning(ex, "Sadu {Key} nešlo stáhnout, zkouším snapshot", key);

                var snapshot = await _snapshots.ReadAsync($"{key}.geojson", ct);
                records = snapshot == null ? [] : _openDataService.Parse(snapshot);
                status = snapshot == null ? ImportStatus.Failed : ImportStatus.Snapshot;
                error = ex.Message;
            }

            if (status != ImportStatus.Failed)
            {
                await _dbContext.OpenDataFeatures.Where(f => f.DataSourceId == source.Id).ExecuteDeleteAsync(ct);

                _dbContext.OpenDataFeatures.AddRange(records.Select(r => new OpenDataFeature(source.Id, r.ExternalId, r.Properties, r.Geometry)));
            }

            import.Finish(status, records.Count, error);
            source.MarkImported(status, records.Count);

            await _dbContext.SaveChangesAsync(ct);

            _dbContext.ChangeTracker.Clear();

            _logger.LogInformation("Sada {Key}: {Count} záznamů ({Status})", key, records.Count, status);

            return new DatasetImportResult(key, status, records.Count, error);
        }

        public async Task<IReadOnlyList<RawFeature>> LoadAsync(string key, CancellationToken ct)
        {
            var features = await _dbContext.OpenDataFeatures
                .AsNoTracking()
                .Where(f => f.DataSource.Key == key)
                .Select(f => new { f.Properties, f.Geometry })
                .ToListAsync(ct);

            return features
                .Select(f =>
                {
                    using var document = JsonDocument.Parse(f.Properties);
                    return new RawFeature(document.RootElement.Clone(), f.Geometry);
                })
                .ToList();
        }

        // Externí zdroje (ČÚZK, Open-Meteo) nemají surovou vrstvu, ale do panelu zdrojů patří taky
        public async Task RecordExternalImportAsync(string key, ImportStatus status, int recordCount, string? error, CancellationToken ct)
        {
            var source = await _dbContext.DataSources.FirstOrDefaultAsync(d => d.Key == key, ct);
            if (source == null)
                return;

            var import = new DataImport(source.Id);
            import.Finish(status, recordCount, error);

            _dbContext.DataImports.Add(import);
            source.MarkImported(status, recordCount);

            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
