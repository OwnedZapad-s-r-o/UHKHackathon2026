using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.DataSources.GetDataSources
{
    public record GetDataSourcesCommand : IRequest<Response<GetDataSourcesDto>>;

    public record GetDataSourcesDto(int KhkDatasetCount, int ExternalSourceCount, int TotalRecords, IReadOnlyCollection<DataSourceGroupDto> Groups);

    public record DataSourceGroupDto(string Group, IReadOnlyCollection<GetDataSourceDto> Sources);

    public record GetDataSourceDto(string Key, string Title, string Provider, string Group, string Purpose, string? DatasetId, string? PortalUrl, string ServiceUrl, DateTimeOffset? LastImportedAt, int? LastRecordCount, ImportStatus? LastStatus);

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
                .Select(d => new GetDataSourceDto(d.Key, d.Title, d.Provider, d.Group, d.Purpose, d.DatasetId, d.PortalUrl, d.ServiceUrl, d.LastImportedAt, d.LastRecordCount, d.LastStatus))
                .ToListAsync(ct);

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
