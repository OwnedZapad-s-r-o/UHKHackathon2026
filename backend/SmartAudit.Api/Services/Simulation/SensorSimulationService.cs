using SmartAudit.Api.Services.Audit;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Simulation
{
    // Po importu založí senzory, dopočítá historii a pak průběžně přidává „živá“ měření a přepočítává audit
    public class SensorSimulationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPipelineStateService _state;
        private readonly SimulationSettings _settings;
        private readonly IAuditTrigger _auditTrigger;
        private readonly ILogger<SensorSimulationService> _logger;

        public SensorSimulationService(IServiceScopeFactory scopeFactory, IPipelineStateService state, SimulationSettings settings, IAuditTrigger auditTrigger, ILogger<SensorSimulationService> logger)
        {
            _scopeFactory = scopeFactory;
            _state = state;
            _settings = settings;
            _auditTrigger = auditTrigger;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            if (!_settings.Enabled)
                return;

            try
            {
                await _state.WaitUntilReadyAsync(ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            var lastWeatherRefresh = DateTimeOffset.UtcNow;
            var firstRun = true;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var provider = scope.ServiceProvider;
                    var now = DateTimeOffset.UtcNow;

                    if (now - lastWeatherRefresh > TimeSpan.FromHours(1))
                    {
                        await provider.GetRequiredService<IWeatherService>().RefreshAsync(2, ct);
                        lastWeatherRefresh = now;
                    }

                    await provider.GetRequiredService<ISensorProvisioningService>().ProvisionAsync(now.AddDays(-_settings.BackfillDays), ct);

                    var written = await provider.GetRequiredService<ISensorDataSource>().SyncAsync(now, ct);

                    // Přepočet i po nových měřeních ze skutečných čidel (IQRF), ne jen po simulaci
                    if (written > 0 || firstRun || _auditTrigger.TryConsume())
                        await provider.GetRequiredService<IFacilityAuditService>().EvaluateAllAsync(now, ct);

                    firstRun = false;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Krok simulace selhal");
                }

                await Task.Delay(TimeSpan.FromSeconds(_settings.LiveCheckSeconds), ct);
            }
        }
    }
}
