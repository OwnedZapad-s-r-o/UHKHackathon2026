using SmartAudit.Api.Services.Audit;
using SmartAudit.Api.Services.Reference;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Simulation
{
    // Po importu založí senzory, dopočítá historii a pak průběžně přidává „živá“ měření a přepočítává audit:
    // nový krok simulace (každých 15 min) = přepočet celého kraje, nová měření ze skutečných čidel = jen dotčené budovy.
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
            var step = TimeSpan.FromMinutes(_settings.StepMinutes);
            var lastStep = DateTimeOffset.MinValue;

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

                    var audit = provider.GetRequiredService<IFacilityAuditService>();

                    // Skutečná měření MICRORISC pokračují v čase (nová hodnota každé 4 minuty) — budova jde do fronty auditu
                    await provider.GetRequiredService<IMicroriscReferenceService>().SyncTelemetryAsync(now, ct);

                    // Simulace má smysl jen na hranici kroku — mezi tím by jen znovu četla databázi a nic nezapsala.
                    // Výjimka: nová budova od správce dostane čidla a týden historie hned, ne až za čtvrt hodiny.
                    var currentStep = new DateTimeOffset(now.UtcTicks - now.UtcTicks % step.Ticks, TimeSpan.Zero);
                    var newFacilities = _auditTrigger.DrainProvisioning();
                    if (currentStep != lastStep)
                    {
                        await provider.GetRequiredService<ISensorProvisioningService>().ProvisionAsync(now.AddDays(-_settings.BackfillDays), ct);
                        await provider.GetRequiredService<ISensorDataSource>().SyncAsync(now, ct);

                        // Nový krok přinesl měření všem budovám — přepočet celého kraje (a čekající budovy tím jsou vyřízené)
                        _auditTrigger.DrainFacilities();
                        await audit.EvaluateAsync(null, now, ct);
                        lastStep = currentStep;
                    }
                    else if (newFacilities.Count > 0)
                    {
                        await provider.GetRequiredService<ISensorProvisioningService>().ProvisionAsync(now.AddDays(-_settings.BackfillDays), ct);
                        await provider.GetRequiredService<ISensorDataSource>().SyncAsync(now, ct);
                        await audit.EvaluateAsync([.. newFacilities, .. _auditTrigger.DrainFacilities()], now, ct);
                    }
                    else
                    {
                        // Mezi kroky jen budovy, pro které přišla skutečná měření nebo se změnila čidla
                        await audit.EvaluateAsync(_auditTrigger.DrainFacilities(), now, ct);
                    }
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
