using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Protocol;
using SmartAudit.Api.Services.Simulation;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Sensors
{
    // Virtuální IQRF gatewaye (testovací síť kraje, jednoklikové demo): každá se k brokeru připojí vlastním účtem
    // jako skutečná gateway, odebírá své dotazy a odpovídá jako IQRF Gateway Daemon. Data tak jdou celou skutečnou cestou —
    // přihlášení a práva na brokeru, MQTT, parser, uložení, audit — jen hardware je nahrazený.
    // Když za gateway odpovídá i někdo jiný (hardware, Python emulátor), virtuální odpovědi se na čas vypnou.
    public class VirtualGatewayService : BackgroundService
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPipelineStateService _pipeline;
        private readonly IIqrfGatewayState _state;
        private readonly IqrfSettings _settings;
        private readonly ILogger<VirtualGatewayService> _logger;

        private readonly Dictionary<string, IMqttClient> _clients = new(StringComparer.OrdinalIgnoreCase);

        public VirtualGatewayService(IServiceScopeFactory scopeFactory, IPipelineStateService pipeline, IIqrfGatewayState state, IqrfSettings settings, ILogger<VirtualGatewayService> logger)
        {
            _scopeFactory = scopeFactory;
            _pipeline = pipeline;
            _state = state;
            _settings = settings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            if (!_settings.Enabled || string.IsNullOrEmpty(_settings.DeviceSecret))
                return;

            try
            {
                await _pipeline.WaitUntilReadyAsync(ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            var factory = new MqttClientFactory();

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await SyncClientsAsync(factory, ct);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogWarning(ex, "Virtuální gatewaye se nepodařilo obnovit");
                    }

                    await Task.Delay(RefreshInterval, ct);
                }
            }
            catch (OperationCanceledException)
            {
                // vypínání aplikace
            }
            finally
            {
                foreach (var client in _clients.Values)
                    client.Dispose();
            }
        }

        // Klient pro každou virtuální gateway v databázi; odebraná gateway se odpojí, odpojená se připojí znovu
        // (účet na brokeru založí IqrfGatewayService až po uložení gatewaye, první pokusy tak mohou selhat)
        private async Task SyncClientsAsync(MqttClientFactory factory, CancellationToken ct)
        {
            List<string> gatewayIds;
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SmartAuditDbContext>();
                gatewayIds = await dbContext.SensorDevices.AsNoTracking().Where(d => d.IsVirtual).Select(d => d.GatewayId).ToListAsync(ct);
            }

            foreach (var removed in _clients.Keys.Where(id => !gatewayIds.Contains(id, StringComparer.OrdinalIgnoreCase)).ToList())
            {
                await DisconnectAsync(_clients[removed], ct);
                _clients.Remove(removed);
            }

            foreach (var gatewayId in gatewayIds)
            {
                if (!_clients.TryGetValue(gatewayId, out var client))
                {
                    client = factory.CreateMqttClient();
                    client.ApplicationMessageReceivedAsync += e => RespondAsync(client, gatewayId, e.ApplicationMessage.ConvertPayloadToString());
                    _clients[gatewayId] = client;
                }

                if (client.IsConnected)
                    continue;

                try
                {
                    await ConnectAsync(factory, client, gatewayId, ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogDebug("Virtuální gateway {GatewayId} se zatím nepřipojila: {Error}", gatewayId, ex.Message);
                }
            }
        }

        private async Task ConnectAsync(MqttClientFactory factory, IMqttClient client, string gatewayId, CancellationToken ct)
        {
            var credentials = GatewayCredentials.For(_settings.DeviceSecret, gatewayId);
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_settings.Host, _settings.Port)
                .WithClientId($"virtual-{gatewayId}")
                .WithCredentials(credentials.Username, credentials.Password)
                .WithCleanSession();

            if (_settings.UseTls)
                options = options.WithTlsOptions(tls => tls.UseTls());

            await client.ConnectAsync(options.Build(), ct);

            var subscription = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(filter => filter.WithTopic(string.Format(_settings.RequestTopicTemplate, gatewayId)))
                .Build();
            await client.SubscribeAsync(subscription, ct);

            _logger.LogInformation("Virtuální gateway {GatewayId} připojena k brokeru", gatewayId);
        }

        private async Task RespondAsync(IMqttClient client, string gatewayId, string request)
        {
            var now = DateTimeOffset.UtcNow;
            if (_state.HasExternalResponder(gatewayId, now))
                return;

            var response = VirtualGatewayResponder.Respond(gatewayId, request, now);
            if (response == null)
                return;

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(string.Format(_settings.ResponseTopicTemplate, gatewayId))
                .WithPayload(response)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            try
            {
                await client.PublishAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Virtuální gateway {GatewayId} neodeslala odpověď: {Error}", gatewayId, ex.Message);
            }
        }

        private static async Task DisconnectAsync(IMqttClient client, CancellationToken ct)
        {
            try
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(cancellationToken: ct);
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
