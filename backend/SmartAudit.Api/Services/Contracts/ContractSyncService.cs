using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services.Import;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace SmartAudit.Api.Services.Contracts
{
    public interface IContractSyncService
    {
        Task<int> SyncAsync(CancellationToken ct);
    }

    // Smlouvy organizací kraje z Registru smluv: nejdřív snímek ze seed/opendata (hned po startu, i bez sítě),
    // pak živě u organizací starších než RefreshAfterDays. Na jednu organizaci ~18 lehkých dotazů + detail
    // každé relevantní smlouvy; z PDF se čte jen cena u smluv na dodávku energie.
    public class ContractSyncService : IContractSyncService
    {
        public const string SourceKey = "registr-smluv";
        private const string SnapshotFile = "registr-smluv.json";

        // Stačí první strany — cenové ujednání bývá v těle smlouvy nebo v první příloze
        private const int MaxPdfPages = 30;
        private const int MaxPriceAttachments = 3;

        // Druh smlouvy a komodita jako text — snímek v seed/opendata je čitelný a nezávisí na pořadí hodnot enumu
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

        private static readonly Regex XmlTag = new(@"<[^>]+>", RegexOptions.Compiled);

        private record SnapshotContract(RegistryContract Contract, ContractCategory Category, EnergyCommodity? Commodity, double? UnitPriceCzkPerMwh, string? PriceSource);

        private record SnapshotOrganization(DateTimeOffset SyncedAt, List<SnapshotContract> Contracts);

        private readonly SmartAuditDbContext _dbContext;
        private readonly IContractRegisterService _registry;
        private readonly IOpenDataImportService _importService;
        private readonly SnapshotStore _snapshots;
        private readonly ContractRegistrySettings _settings;
        private readonly OpenDataSettings _openDataSettings;
        private readonly ILogger<ContractSyncService> _logger;

        public ContractSyncService(SmartAuditDbContext dbContext, IContractRegisterService registry, IOpenDataImportService importService, SnapshotStore snapshots, ContractRegistrySettings settings, OpenDataSettings openDataSettings, ILogger<ContractSyncService> logger)
        {
            _dbContext = dbContext;
            _registry = registry;
            _importService = importService;
            _snapshots = snapshots;
            _settings = settings;
            _openDataSettings = openDataSettings;
            _logger = logger;
        }

        public async Task<int> SyncAsync(CancellationToken ct)
        {
            var snapshot = await ReadSnapshotAsync(ct);

            var organizations = await _dbContext.Organizations
                .Include(o => o.Contracts)
                .OrderBy(o => o.Ico)
                .ToListAsync(ct);

            foreach (var organization in organizations.Where(o => o.ContractsSyncedAt == null))
            {
                if (!snapshot.TryGetValue(organization.Ico, out var entry))
                    continue;

                Apply(organization, entry.Contracts);
                organization.MarkContractsSynced(entry.SyncedAt);
            }

            await _dbContext.SaveChangesAsync(ct);

            if (!_settings.Enabled)
            {
                await RecordAsync(snapshot.Count > 0 ? ImportStatus.Snapshot : ImportStatus.Failed, "Stahování z registru je vypnuté", ct);
                return 0;
            }

            var now = DateTimeOffset.UtcNow;
            var stale = organizations
                .Where(o => o.ContractsSyncedAt == null || o.ContractsSyncedAt < now.AddDays(-_settings.RefreshAfterDays))
                .ToList();

            var synced = 0;
            var failures = 0;
            var consecutiveFailures = 0;
            string? lastError = null;

            foreach (var organization in stale)
            {
                try
                {
                    var contracts = await FetchOrganizationAsync(organization.Ico, ct);

                    Apply(organization, contracts);
                    organization.MarkContractsSynced(DateTimeOffset.UtcNow);
                    await _dbContext.SaveChangesAsync(ct);

                    snapshot[organization.Ico] = new SnapshotOrganization(DateTimeOffset.UtcNow, contracts);
                    synced++;
                    consecutiveFailures = 0;

                    _logger.LogInformation("Registr smluv: {Organization} ({Ico}) — {Count} smluv o energiích a budově", organization.Name, organization.Ico, contracts.Count);

                    if (synced % 10 == 0)
                        await WriteSnapshotAsync(snapshot, ct);
                }
                catch (Exception ex) when (ex is HttpRequestException or JsonException || (ex is TaskCanceledException && !ct.IsCancellationRequested))
                {
                    failures++;
                    consecutiveFailures++;
                    lastError = ex.Message;
                    _logger.LogWarning(ex, "Registr smluv nevrátil smlouvy pro IČO {Ico}", organization.Ico);

                    if (consecutiveFailures >= _settings.MaxConsecutiveFailures)
                    {
                        _logger.LogWarning("Registr smluv: {Failures} selhání za sebou, stahování přerušeno", consecutiveFailures);
                        break;
                    }
                }
            }

            if (synced > 0)
                await WriteSnapshotAsync(snapshot, ct);

            var status = stale.Count == 0 || synced > 0 ? ImportStatus.Live : snapshot.Count > 0 ? ImportStatus.Snapshot : ImportStatus.Failed;
            await RecordAsync(status, failures > 0 ? $"{failures} organizací selhalo: {lastError}" : null, ct);

            _logger.LogInformation("Registr smluv: aktualizováno {Synced} z {Stale} organizací ({Failures} chyb)", synced, stale.Count, failures);

            return synced;
        }

        private async Task<List<SnapshotContract>> FetchOrganizationAsync(string ico, CancellationToken ct)
        {
            // Registr hledá podřetězec předmětu — víc úzkých dotazů je pro něj lehčí než stažení všech smluv organizace
            var candidates = new Dictionary<string, RegistrySearchRow>();

            foreach (var stem in ContractClassifier.SearchStems)
            {
                var rows = await _registry.SearchAsync(ico, stem, ct);
                foreach (var row in rows.Where(r => ContractClassifier.Classify(r.Title) != null))
                    candidates.TryAdd(row.VersionId, row);

                await Task.Delay(_settings.RequestDelayMs, ct);
            }

            var contracts = new List<SnapshotContract>();

            foreach (var versionId in candidates.Keys)
            {
                var contract = await _registry.GetContractAsync(versionId, ct);
                await Task.Delay(_settings.RequestDelayMs, ct);

                if (contract == null || !ContractRegistryParser.SameIco(contract.PublisherIco, ico))
                    continue;

                if (ContractClassifier.Classify(contract.Title) is not { } category)
                    continue;

                var commodity = category == ContractCategory.EnergySupply ? ContractClassifier.DetectCommodity(contract.Title) : null;
                var price = commodity == null ? null : await ExtractPriceAsync(contract, commodity.Value, ct);

                contracts.Add(new SnapshotContract(contract, category, commodity, price?.Price.CzkPerMwh, price == null ? null : $"„{price.Value.Price.Snippet}“ v příloze {price.Value.FileName}"));
            }

            return contracts;
        }

        private async Task<(ExtractedEnergyPrice Price, string FileName)?> ExtractPriceAsync(RegistryContract contract, EnergyCommodity commodity, CancellationToken ct)
        {
            // Dodatky s aktuální cenou bývají ve Wordu, původní smlouva ve skenu PDF
            var documents = contract.Attachments
                .Where(a => a.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || a.Name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                .Take(MaxPriceAttachments);

            foreach (var attachment in documents)
            {
                var text = await ReadAttachmentTextAsync(attachment, ct);
                await Task.Delay(_settings.RequestDelayMs, ct);

                if (text != null && EnergyPriceExtractor.Extract(text, commodity) is { } price)
                    return (price, attachment.Name);
            }

            return null;
        }

        // Sken bez textové vrstvy vrátí prázdný text — cena se pak nenajde a zůstane paušál
        private async Task<string?> ReadAttachmentTextAsync(RegistryAttachment attachment, CancellationToken ct)
        {
            await using var stream = await _registry.DownloadAttachmentAsync(attachment.Url, ct);
            if (stream == null)
                return null;

            var limit = _settings.MaxPdfMegabytes * 1024L * 1024L;
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;

            while ((read = await stream.ReadAsync(chunk, ct)) > 0)
            {
                if (buffer.Length + read > limit)
                    return null;

                buffer.Write(chunk, 0, read);
            }

            try
            {
                buffer.Position = 0;

                return attachment.Name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ? DocxText(buffer) : PdfText(buffer);
            }
            // Poškozený nebo zašifrovaný soubor z registru — přeskočit, smlouva zůstane bez ceny
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogDebug(ex, "Přílohu {Name} nejde přečíst", attachment.Name);
                return null;
            }
        }

        private static string PdfText(MemoryStream buffer)
        {
            using var document = PdfDocument.Open(buffer.ToArray());
            var text = new StringBuilder();

            foreach (var page in document.GetPages().Take(MaxPdfPages))
                text.AppendLine(ContentOrderTextExtractor.GetText(page));

            return text.ToString();
        }

        // .docx je zip — text je ve word/document.xml, odstavec končí </w:p>
        private static string DocxText(MemoryStream buffer)
        {
            using var archive = new ZipArchive(buffer, ZipArchiveMode.Read);
            var entry = archive.GetEntry("word/document.xml");
            if (entry == null)
                return string.Empty;

            using var reader = new StreamReader(entry.Open());
            var xml = reader.ReadToEnd().Replace("</w:p>", "\n");

            return System.Net.WebUtility.HtmlDecode(XmlTag.Replace(xml, string.Empty));
        }

        private void Apply(Organization organization, IReadOnlyCollection<SnapshotContract> contracts)
        {
            var existing = organization.Contracts.ToDictionary(c => c.ContractId);
            var current = contracts.Select(c => c.Contract.ContractId).ToHashSet();

            foreach (var item in contracts)
            {
                var source = item.Contract;

                if (!existing.TryGetValue(source.ContractId, out var contract))
                {
                    contract = new FacilityContract(organization.Id, source.ContractId);
                    _dbContext.FacilityContracts.Add(contract);
                    existing[source.ContractId] = contract;
                }

                contract.ApplyRegistry(
                    source.VersionId,
                    source.ContractNumber,
                    source.Title,
                    source.PartnerName,
                    source.PartnerIco,
                    source.AmountWithoutVat,
                    source.AmountWithVat,
                    source.DateSigned,
                    source.PublishedOn,
                    item.Category,
                    JsonSerializer.Serialize(source.Attachments, JsonOptions));

                contract.SetEnergyPrice(item.Commodity, item.UnitPriceCzkPerMwh, item.PriceSource);
            }

            // Smlouva zneplatněná v registru (nebo už nesouvisí s energií) z přehledu zmizí
            foreach (var removed in organization.Contracts.Where(c => !current.Contains(c.ContractId)).ToList())
                _dbContext.FacilityContracts.Remove(removed);
        }

        private async Task RecordAsync(ImportStatus status, string? error, CancellationToken ct)
        {
            var count = await _dbContext.FacilityContracts.CountAsync(ct);

            await _importService.RecordExternalImportAsync(SourceKey, status, count, error, ct);
        }

        private async Task<Dictionary<string, SnapshotOrganization>> ReadSnapshotAsync(CancellationToken ct)
        {
            var json = await _snapshots.ReadAsync(SnapshotFile, ct);

            return json == null ? new() : JsonSerializer.Deserialize<Dictionary<string, SnapshotOrganization>>(json, JsonOptions) ?? new();
        }

        private async Task WriteSnapshotAsync(Dictionary<string, SnapshotOrganization> snapshot, CancellationToken ct)
        {
            if (!_openDataSettings.WriteSnapshots)
                return;

            await _snapshots.WriteAsync(SnapshotFile, JsonSerializer.Serialize(snapshot, new JsonSerializerOptions(JsonOptions) { WriteIndented = true }), ct);
        }
    }
}
