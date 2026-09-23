using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Import
{
    // Start aplikace: migrace → katalog zdrojů → stažení sad → portfolio → ČÚZK → počasí → demo uživatelé
    public class DataPipelineService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPipelineStateService _state;
        private readonly OpenDataSettings _openDataSettings;
        private readonly SimulationSettings _simulationSettings;
        private readonly ILogger<DataPipelineService> _logger;

        public DataPipelineService(IServiceScopeFactory scopeFactory, IPipelineStateService state, OpenDataSettings openDataSettings, SimulationSettings simulationSettings, ILogger<DataPipelineService> logger)
        {
            _scopeFactory = scopeFactory;
            _state = state;
            _openDataSettings = openDataSettings;
            _simulationSettings = simulationSettings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            try
            {
                await MigrateAsync(ct);

                using var scope = _scopeFactory.CreateScope();
                var provider = scope.ServiceProvider;
                var dbContext = provider.GetRequiredService<SmartAuditDbContext>();
                var importService = provider.GetRequiredService<IOpenDataImportService>();

                _state.SetPhase("Katalog zdrojů dat");
                await importService.SyncDataSourcesAsync(ct);

                var hasPortfolio = await dbContext.Facilities.AnyAsync(ct);

                if (!hasPortfolio || _openDataSettings.RefreshOnStartup)
                {
                    _state.SetPhase("Stahování otevřených dat KHK");
                    await importService.ImportAllAsync(ct);

                    _state.SetPhase("Sestavení portfolia objektů");
                    await provider.GetRequiredService<IPortfolioService>().BuildAsync(ct);
                }

                _state.SetPhase("Karty budov z ČÚZK");
                await provider.GetRequiredService<IBuildingEnrichmentService>().EnrichAsync(_openDataSettings.RefreshOnStartup, ct);

                _state.SetPhase("Počasí a ovzduší z Open-Meteo");
                await provider.GetRequiredService<IWeatherService>().RefreshAsync(_simulationSettings.BackfillDays + 1, ct);

                await importService.RecordExternalImportAsync("msmt-kalendar", ImportStatus.Live, SchoolCalendar.DefinitionCount, null, ct);

                await provider.GetRequiredService<IDemoUserService>().SeedAsync(ct);

                _state.MarkReady();
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Příprava dat selhala");
                _state.SetPhase($"Chyba: {ex.Message}");
            }
        }

        // Databáze v Dockeru může naběhnout později než API — pár pokusů s odstupem
        private async Task MigrateAsync(CancellationToken ct)
        {
            _state.SetPhase("Migrace databáze");

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<SmartAuditDbContext>();

                    await dbContext.Database.MigrateAsync(ct);

                    // Nově vytvořené rozšíření PostGIS musí Npgsql znovu načíst typy
                    var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
                    await connection.OpenAsync(ct);
                    await connection.ReloadTypesAsync(ct);
                    await connection.CloseAsync();

                    return;
                }
                catch (NpgsqlException ex) when (attempt < 10)
                {
                    _logger.LogWarning("Databáze zatím nepřijímá spojení ({Message}), pokus {Attempt}", ex.Message, attempt);
                    await Task.Delay(TimeSpan.FromSeconds(3), ct);
                }
            }
        }
    }
}
