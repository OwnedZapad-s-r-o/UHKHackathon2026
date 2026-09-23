using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services;
using SmartAudit.Api.Services.Import;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Features.DataSources.ImportDataSource
{
    public record ImportDataSourceCommand(string Key) : IRequest<Response<ImportDataSourceDto>>;

    public record ImportDataSourceDto(string Key, ImportStatus Status, int RecordCount, string? Error);

    // Ruční obnovení jedné sady — po stažení sady KHK se znovu sestaví portfolio
    public class ImportDataSourceHandler : IRequestHandler<ImportDataSourceCommand, Response<ImportDataSourceDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IOpenDataImportService _importService;
        private readonly IPortfolioService _portfolioService;
        private readonly IBuildingEnrichmentService _buildingService;
        private readonly IWeatherService _weatherService;

        public ImportDataSourceHandler(SmartAuditDbContext dbContext, IOpenDataImportService importService, IPortfolioService portfolioService, IBuildingEnrichmentService buildingService, IWeatherService weatherService)
        {
            _dbContext = dbContext;
            _importService = importService;
            _portfolioService = portfolioService;
            _buildingService = buildingService;
            _weatherService = weatherService;
        }

        public async Task<Response<ImportDataSourceDto>> Handle(ImportDataSourceCommand request, CancellationToken ct)
        {
            var source = await _dbContext.DataSources
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Key == request.Key, ct);

            if (source == null)
                return Response.NotFound<ImportDataSourceDto>("Zdroj dat s tímto klíčem neexistuje");

            if (source.Provider == OpenDataImportService.KhkProvider)
            {
                var result = await _importService.ImportDatasetAsync(request.Key, ct);

                if (result.Status != ImportStatus.Failed)
                    await _portfolioService.BuildAsync(ct);

                return Response.Success(new ImportDataSourceDto(result.Key, result.Status, result.RecordCount, result.Error));
            }

            switch (request.Key)
            {
                case BuildingEnrichmentService.SourceKey:
                    var found = await _buildingService.EnrichAsync(true, ct);
                    return Response.Success(new ImportDataSourceDto(request.Key, ImportStatus.Live, found, null));

                case WeatherService.TemperatureSourceKey:
                case WeatherService.AirQualitySourceKey:
                    var rows = await _weatherService.RefreshAsync(2, ct);
                    return Response.Success(new ImportDataSourceDto(request.Key, rows > 0 ? ImportStatus.Live : ImportStatus.Failed, rows, null));

                default:
                    return Response.ValidationFailed<ImportDataSourceDto>("Tento zdroj je statický a nejde znovu stáhnout.");
            }
        }
    }
}
