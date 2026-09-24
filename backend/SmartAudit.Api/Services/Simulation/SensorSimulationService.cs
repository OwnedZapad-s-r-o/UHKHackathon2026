using SmartAudit.Api.Services.Audit;
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

                    // Simulace má smysl jen na hranici kroku — mezi tím by jen znovu četla databázi a nic nezapsala
                    var currentStep = new DateTimeOffset(now.UtcTicks - now.UtcTicks % step.Ticks, TimeSpan.Zero);
                    if (currentStep != lastStep)
                    {
                        await provider.GetRequiredService<ISensorProvisioningService>().ProvisionAsync(now.AddDays(-_settings.BackfillDays), ct);
                        await provider.GetRequiredService<ISensorDataSource>().SyncAsync(now, ct);

                        // Nový krok přinesl měření všem budovám — přepočet celého kraje (a čekající budovy tím jsou vyřízené)
                        _auditTrigger.DrainFacilities();
                        await audit.EvaluateAsync(null, now, ct);
                        lastStep = currentStep;
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
