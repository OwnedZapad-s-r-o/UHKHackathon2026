using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services.OpenData;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.DataSources.GetDataSources
{
    public record GetDataSourcesCommand : IRequest<Response<GetDataSourcesDto>>;

    public record GetDataSourcesDto(int KhkDatasetCount, int ExternalSourceCount, int TotalRecords, IReadOnlyCollection<DataSourceGroupDto> Groups);

    public record DataSourceGroupDto(string Group, IReadOnlyCollection<GetDataSourceDto> Sources);

    // CatalogUrl = záznam v katalogu poskytovatele (Čl. 7.3), LiveApiUrl = živý endpoint, ze kterého aplikace čte;
    // LastSyncedAt / LiveAvailable / LiveRecordCount = poslední živá kontrola (POST /api/opendata/sync)
    public record GetDataSourceDto(string Key, string Title, string Provider, string Group, string Purpose, string? DatasetId, string? PortalUrl, string ServiceUrl, DateTimeOffset? LastImportedAt, int? LastRecordCount, ImportStatus? LastStatus,
        string Category = "", string CatalogUrl = "", string LiveApiUrl = "", string? DcatRecordUrl = null, DateTimeOffset? LastSyncedAt = null, bool? LiveAvailable = null, int? LiveRecordCount = null);

    // Panel „Zdroje dat“ — pravidla hackathonu vyžadují odkaz na použitá otevřená data (F37)
    public class GetDataSourcesHandler : IRequestHandler<GetDataSourcesCommand, Response<GetDataSourcesDto>>
    {
        private const string KhkProvider = "Data KHK";

        private readonly SmartAuditDbContext _dbContext;
        private readonly OpenDataSettings _settings;

        public GetDataSourcesHandler(SmartAuditDbContext dbContext, OpenDataSettings settings)
        {
            _dbContext = dbContext;
            _settings = settings;
        }

        public async Task<Response<GetDataSourcesDto>> Handle(GetDataSourcesCommand request, CancellationToken ct)
        {
            var loaded = await _dbContext.DataSources
                .AsNoTracking()
                .Select(d => new GetDataSourceDto(d.Key, d.Title, d.Provider, d.Group, d.Purpose, d.DatasetId, d.PortalUrl, d.ServiceUrl, d.LastImportedAt, d.LastRecordCount, d.LastStatus,
                    "", "", "", null, d.LastSyncedAt, d.LiveAvailable, d.LiveRecordCount))
                .ToListAsync(ct);

            var configured = _settings.Datasets.GroupBy(d => d.Key).ToDictionary(g => g.Key, g => g.First());
            loaded = loaded
                .Select(s => configured.TryGetValue(s.Key, out var d)
                    ? s with { Category = OpenDataLinks.CategoryOf(d), CatalogUrl = OpenDataLinks.CatalogUrl(d), LiveApiUrl = OpenDataLinks.LiveApiUrl(d), DcatRecordUrl = OpenDataLinks.DcatRecordUrl(d) }
                    : s with { CatalogUrl = s.PortalUrl ?? s.ServiceUrl, LiveApiUrl = s.ServiceUrl })
                .ToList();

            // Pořadí jako v konfiguraci — od portfolia objektů po externí zdroje
            var order = _settings.Datasets.Select(d => d.Key).ToList();
            var sources = loaded.OrderBy(s => ConfigOrder(order, s.Key)).ToList();

            var groups = sources
                .GroupBy(s => s.Group)
                .Select(g => new DataSourceGroupDto(g.Key, g.ToList()))
                .ToList();

            var dto = new GetDataSourcesDto(
                sources.Count(s => s.Provider == KhkProvider),
                sources.Count(s => s.Provider != KhkProvider),
                sources.Where(s => s.Provider == KhkProvider).Sum(s => s.LastRecordCount ?? 0),
                groups);

            return Response.Success(dto);
        }

        private static int ConfigOrder(List<string> order, string key)
        {
            var index = order.IndexOf(key);

            return index < 0 ? int.MaxValue : index;
        }
    }
}
