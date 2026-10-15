using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Api.Services.Import;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.OpenData
{
    public record CatalogStatusDto(string Url, bool? Available, int? DatasetCount, string? ETag, string? LastModified, int? ResponseMs, string? Error);

    public record SourceSyncDto(
        string Key,
        string Title,
        string Provider,
        string Category,
        string Group,
        string CatalogUrl,
        string LiveApiUrl,
        string? DcatRecordUrl,
        string SyncMode,
        string Status,
        bool? LiveAvailable,
        int? LiveRecordCount,
        int? LocalRecordCount,
        string? Version,
        int? ResponseMs,
        string? Error,
        bool Changed,
        DateTimeOffset? LastSyncedAt,
        DateTimeOffset? LastImportedAt,
        ImportStatus? LastImportStatus);

    public record SyncSummaryDto(int Total, int Live, int Cache, int Changed);

    public record OpenDataSyncDto(
        DateTimeOffset? SyncedAt,
        int? DurationMs,
        string SyncMode,
        CatalogStatusDto Catalog,
        SyncSummaryDto Summary,
        bool ReimportRunning,
        IReadOnlyList<string> ReimportedKeys,
        IReadOnlyList<SourceSyncDto> Sources);

    public interface IOpenDataSyncService
    {
        Task<OpenDataSyncDto> SyncAsync(CancellationToken ct);
        Task<OpenDataSyncDto> StatusAsync(CancellationToken ct);
    }

    // Sdílený stav synchronizace mezi požadavky — běží nejvýš jedna kontrola a jeden reimport najednou
    public class OpenDataSyncState
    {
        public SemaphoreSlim Gate { get; } = new(1, 1);
        public DateTimeOffset? LastSyncAt { get; set; }
        public int? LastDurationMs { get; set; }
        public CatalogStatusDto? Catalog { get; set; }
        public HashSet<string> Changed { get; } = new();
        public volatile bool ReimportRunning;
        public List<string> ReimportedKeys { get; } = new();
    }

    // Živá synchronizace s poskytovateli (Data KHK ArcGIS REST, ČÚZK WFS, Open-Meteo, Registr smluv):
    // každý zdroj se zkontroluje s timeoutem 5 s — dostupnost, počet záznamů a značka změny (ETag, Last-Modified,
    // datum poslední editace vrstvy). Sady, které se u poskytovatele změnily, se na pozadí znovu stáhnou a přepočítá se
    // portfolio i pre-audit. Když server neodpoví (univerzitní Wi-Fi), aplikace jede dál z lokální cache seed/opendata.
    public class OpenDataSyncService : IOpenDataSyncService
    {
        public const string SyncMode = "Live API + Local Snapshot Cache";
        private const string KhkProvider = "Data KHK";
        private static readonly TimeSpan PerSourceTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(15);

        // Sady, ze kterých se skládá portfolio — jejich změna znamená přestavbu objektů
        private static readonly HashSet<string> PortfolioKeys =
        [
            "skoly", "pravni-subjekty-skol", "organizace-khk", "socialni-sluzby", "nemocnice", "nasledna-pece", "lecebne-ustavy",
            "zaci-ss-kraj", "zaci-ss-historie", "studenti-vos-kraj", "ubytovani-dm", "prijimacky-ss", "prijimacky-vos", "hsoo",
            "dotace-orp", "dotace-orp-individualni", "dotace-orp-programove", "pamatkove-zony", "pamatkove-rezervace", "nkp-plosne",
            "nkp-bodove", "hranice-okresu", "hranice-orp", "hranice-kraje", "adresar-obci",
        ];

        private readonly HttpClient _httpClient;
        private readonly SmartAuditDbContext _dbContext;
        private readonly OpenDataSettings _settings;
        private readonly OpenDataSyncState _state;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OpenDataSyncService> _logger;

        public OpenDataSyncService(HttpClient httpClient, SmartAuditDbContext dbContext, OpenDataSettings settings, OpenDataSyncState state, IServiceScopeFactory scopeFactory, ILogger<OpenDataSyncService> logger)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _settings = settings;
            _state = state;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        private sealed record CheckResult(string Key, bool Available, int? RecordCount, string? Version, int ResponseMs, string? Error);

        public async Task<OpenDataSyncDto> SyncAsync(CancellationToken ct)
        {
            // Opakované kliknutí během pár sekund nebo souběžná kontrola — vrátí poslední stav, zdroje se nezahlcují
            if (!await _state.Gate.WaitAsync(TimeSpan.Zero, ct))
                return await StatusAsync(ct);

            try
            {
                if (_state.LastSyncAt is { } last && DateTimeOffset.UtcNow - last < MinInterval)
                    return await StatusAsync(ct);

                var watch = Stopwatch.StartNew();

                var datasets = _settings.Datasets.Where(d => d.Key != "msmt-kalendar").ToList();
                var checks = datasets.Select(d => CheckAsync(d, ct)).ToList();
                var catalogTask = CheckCatalogAsync(ct);

                var results = await Task.WhenAll(checks);
                _state.Catalog = await catalogTask;

                var sources = await _dbContext.DataSources.ToDictionaryAsync(d => d.Key, ct);
                var now = DateTimeOffset.UtcNow;
                var changed = new List<string>();

                foreach (var result in results)
                {
                    if (!sources.TryGetValue(result.Key, out var source))
                        continue;

                    // Změna u poskytovatele: jiný počet záznamů než poslední import, nebo nová verze vrstvy
                    var dataset = datasets.First(d => d.Key == result.Key);
                    var isKhk = dataset.Provider == KhkProvider;
                    var countChanged = isKhk && result.RecordCount != null && source.LastRecordCount != null && result.RecordCount != source.LastRecordCount;
                    var versionChanged = isKhk && result.Version != null && source.LiveVersion != null && result.Version != source.LiveVersion;

                    if (result.Available && (countChanged || versionChanged))
                        changed.Add(result.Key);

                    source.MarkLiveCheck(result.Available, result.RecordCount, result.Version, result.ResponseMs, result.Error, now);
                }

                await _dbContext.SaveChangesAsync(ct);

                lock (_state.Changed)
                {
                    _state.Changed.Clear();
                    foreach (var key in changed)
                        _state.Changed.Add(key);
                }

                _state.LastSyncAt = now;
                _state.LastDurationMs = (int)watch.ElapsedMilliseconds;

                if (changed.Count > 0)
                    StartReimport(changed);

                _logger.LogInformation("Živá synchronizace: {Live}/{Total} zdrojů odpovědělo, změněno {Changed}, {Ms} ms",
                    results.Count(r => r.Available), results.Length, changed.Count, watch.ElapsedMilliseconds);

                return await StatusAsync(ct);
            }
            finally
            {
                _state.Gate.Release();
            }
        }

        public async Task<OpenDataSyncDto> StatusAsync(CancellationToken ct)
        {
            var sources = await _dbContext.DataSources.AsNoTracking().ToDictionaryAsync(d => d.Key, ct);
            HashSet<string> changed;
            lock (_state.Changed)
                changed = [.. _state.Changed];

            var rows = _settings.Datasets
                .Where(d => sources.ContainsKey(d.Key))
                .Select(d =>
                {
                    var s = sources[d.Key];
                    var isChanged = changed.Contains(d.Key);
                    var status = d.Key == "msmt-kalendar"
                        ? "Aktuální (pravidla v aplikaci)"
                        : s.LiveAvailable switch
                        {
                            true when isChanged && _state.ReimportRunning => "Změna u poskytovatele — stahuji nová data",
                            true when isChanged => "Nová data načtena z Live API",
                            true => "Aktuální (Live API)",
                            false => "Lokální cache (seed/opendata) — API neodpovědělo",
                            null => s.LastStatus == ImportStatus.Snapshot ? "Lokální cache (seed/opendata)" : s.LastStatus == ImportStatus.Live ? "Načteno z Live API při startu" : "Čeká na synchronizaci",
                        };

                    return new SourceSyncDto(
                        d.Key,
                        d.Title,
                        d.Provider,
                        OpenDataLinks.CategoryOf(d),
                        d.Group,
                        OpenDataLinks.CatalogUrl(d),
                        OpenDataLinks.LiveApiUrl(d),
                        OpenDataLinks.DcatRecordUrl(d),
                        SyncMode,
                        status,
                        s.LiveAvailable,
                        s.LiveRecordCount,
                        s.LastRecordCount,
                        s.LiveVersion,
                        s.LiveResponseMs,
                        s.LiveError,
                        isChanged,
                        s.LastSyncedAt,
                        s.LastImportedAt,
                        s.LastStatus);
                })
                .ToList();

            var summary = new SyncSummaryDto(
                rows.Count,
                rows.Count(r => r.LiveAvailable == true),
                rows.Count(r => r.LiveAvailable == false),
                rows.Count(r => r.Changed));

            List<string> reimported;
            lock (_state.ReimportedKeys)
                reimported = [.. _state.ReimportedKeys];

            return new OpenDataSyncDto(
                _state.LastSyncAt ?? rows.Max(r => r.LastSyncedAt),
                _state.LastDurationMs,
                SyncMode,
                _state.Catalog ?? new CatalogStatusDto(OpenDataLinks.KhkCatalogUrl, null, null, null, null, null, null),
                summary,
                _state.ReimportRunning,
                reimported,
                rows);
        }

        #region Kontroly zdrojů

        private async Task<CheckResult> CheckAsync(DatasetSettings dataset, CancellationToken ct)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(PerSourceTimeout);
            var watch = Stopwatch.StartNew();

            try
            {
                if (dataset.Provider == KhkProvider)
                {
                    // Metadata vrstvy (datum poslední editace dat) a počet záznamů — dva lehké dotazy na ArcGIS REST
                    var layerTask = GetJsonAsync($"{dataset.ServiceUrl}?f=json", timeout.Token);
                    var countTask = GetJsonAsync($"{dataset.ServiceUrl}/query?where=1%3D1&returnCountOnly=true&f=json", timeout.Token);

                    using var layer = await layerTask;
                    using var count = await countTask;

                    if (count.RootElement.TryGetProperty("error", out var error))
                        throw new InvalidOperationException(error.GetRawText());

                    string? version = null;
                    if (layer.RootElement.TryGetProperty("editingInfo", out var editing))
                    {
                        var edit = editing.TryGetProperty("dataLastEditDate", out var dataEdit) ? dataEdit : editing.TryGetProperty("lastEditDate", out var lastEdit) ? lastEdit : default;
                        if (edit.ValueKind == JsonValueKind.Number)
                            version = DateTimeOffset.FromUnixTimeMilliseconds(edit.GetInt64()).ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
                    }

                    var records = count.RootElement.TryGetProperty("count", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : (int?)null;

                    return new CheckResult(dataset.Key, true, records, version, (int)watch.ElapsedMilliseconds, null);
                }

                // Externí zdroje: dostupnost živého endpointu a značka verze z hlaviček
                using var request = new HttpRequestMessage(HttpMethod.Get, OpenDataLinks.LiveApiUrl(dataset));
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                response.EnsureSuccessStatusCode();

                var tag = response.Headers.ETag?.Tag
                    ?? response.Content.Headers.LastModified?.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture)
                    ?? response.Headers.Date?.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);

                return new CheckResult(dataset.Key, true, null, tag, (int)watch.ElapsedMilliseconds, null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException or JsonException or InvalidOperationException)
            {
                if (ct.IsCancellationRequested)
                    throw;

                var message = ex is TaskCanceledException or OperationCanceledException ? $"Bez odpovědi do {PerSourceTimeout.TotalSeconds:0} s" : ex.Message;

                return new CheckResult(dataset.Key, false, null, null, (int)watch.ElapsedMilliseconds, message);
            }
        }

        // Katalog Data KHK (DCAT-AP-CZ JSON-LD) — ETag, Last-Modified a počet sad
        private async Task<CatalogStatusDto> CheckCatalogAsync(CancellationToken ct)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(PerSourceTimeout);
            var watch = Stopwatch.StartNew();

            try
            {
                using var response = await _httpClient.GetAsync(OpenDataLinks.KhkCatalogUrl, timeout.Token);
                response.EnsureSuccessStatusCode();

                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
                var datasets = document.RootElement.TryGetProperty("datová_sada", out var list) && list.ValueKind == JsonValueKind.Array ? list.GetArrayLength() : (int?)null;

                return new CatalogStatusDto(
                    OpenDataLinks.KhkCatalogUrl,
                    true,
                    datasets,
                    response.Headers.ETag?.Tag,
                    response.Content.Headers.LastModified?.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture),
                    (int)watch.ElapsedMilliseconds,
                    null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException or JsonException)
            {
                if (ct.IsCancellationRequested)
                    throw;

                return new CatalogStatusDto(OpenDataLinks.KhkCatalogUrl, false, null, null, null, (int)watch.ElapsedMilliseconds, ex is JsonException ? ex.Message : $"Bez odpovědi do {PerSourceTimeout.TotalSeconds:0} s");
            }
        }

        private async Task<JsonDocument> GetJsonAsync(string url, CancellationToken ct)
        {
            using var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            return JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        }

        #endregion

        // Změněné sady se stáhnou znovu (s pádem na snapshot), pak přestavba portfolia a pre-audit — na pozadí,
        // odpověď na tlačítko nečeká. Běží nejvýš jeden reimport.
        private void StartReimport(List<string> keys)
        {
            if (_state.ReimportRunning)
                return;

            _state.ReimportRunning = true;
            lock (_state.ReimportedKeys)
                _state.ReimportedKeys.Clear();

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var provider = scope.ServiceProvider;
                    var importService = provider.GetRequiredService<IOpenDataImportService>();

                    foreach (var key in keys.Where(k => _settings.Datasets.Any(d => d.Key == k && d.Provider == KhkProvider)))
                    {
                        await importService.ImportDatasetAsync(key, CancellationToken.None);
                        lock (_state.ReimportedKeys)
                            _state.ReimportedKeys.Add(key);
                    }

                    if (keys.Any(PortfolioKeys.Contains))
                        await provider.GetRequiredService<IPortfolioService>().BuildAsync(CancellationToken.None);

                    await provider.GetRequiredService<IPreAuditService>().ApplyAsync(null, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Reimport změněných sad selhal — aplikace jede dál z předchozích dat");
                }
                finally
                {
                    _state.ReimportRunning = false;
                }
            });
        }
    }
}
