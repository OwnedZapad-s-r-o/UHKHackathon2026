using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Protocol;
using SmartAudit.Api.Services.Simulation;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Sensors
{
    // Most mezi aplikací a IQRF Gateway Daemonem přes MQTT:
    // - odebírá gateway/+/iqrf/responses a každou odpověď předá SensorIngestService,
    // - každé gatewayi v databázi posílá v jejím intervalu dotazy na hodnoty: hromadně příkazem FRC (jedna veličina
    //   ze všech uzlů v jednom rádiovém cyklu IQMESH), občas a u nových uzlů čtení po uzlech kvůli objevení senzorů,
    //   a dokud uzly nezná, dotaz na spárované uzly koordinátoru,
    // - na brokeru drží účty gatewayí (Dynamic Security): gateway v databázi = účet s právy jen na její topicy.
    public class IqrfGatewayService : BackgroundService
    {
        private static readonly TimeSpan DeviceRefresh = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DiscoveryInterval = TimeSpan.FromSeconds(5);

        // Jak často znovu zjistit uzly a jejich senzory (nově spárované čidlo, výměna čidla) — mezi tím jen FRC
        private static readonly TimeSpan RediscoveryInterval = TimeSpan.FromMinutes(10);

        // Pevné id klienta + trvalá relace: odpovědi s QoS 1, které dorazí během restartu backendu, broker podrží
        private const string ClientId = "smartaudit-backend";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPipelineStateService _pipeline;
        private readonly IIqrfGatewayState _state;
        private readonly IqrfSettings _settings;
        private readonly ILogger<IqrfGatewayService> _logger;

        private readonly Dictionary<Guid, DateTimeOffset> _nextPoll = [];
        private readonly Dictionary<Guid, DateTimeOffset> _nextDiscovery = [];

        // Gatewaye, kterým už broker v tomto připojení založil účet
        private readonly HashSet<string> _provisioned = [];

        public IqrfGatewayService(IServiceScopeFactory scopeFactory, IPipelineStateService pipeline, IIqrfGatewayState state, IqrfSettings settings, ILogger<IqrfGatewayService> logger)
        {
            _scopeFactory = scopeFactory;
            _pipeline = pipeline;
            _state = state;
            _settings = settings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            if (!_settings.Enabled)
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
            using var client = factory.CreateMqttClient();
            client.ApplicationMessageReceivedAsync += e => HandleMessageAsync(e.ApplicationMessage.Topic, e.ApplicationMessage.ConvertPayloadToString());

            var devices = new List<PolledDevice>();
            var devicesLoadedAt = DateTimeOffset.MinValue;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (!client.IsConnected)
                        await ConnectAsync(factory, client, ct);

                    var now = DateTimeOffset.UtcNow;
                    if (now - devicesLoadedAt > DeviceRefresh)
                    {
                        devices = await LoadDevicesAsync(ct);
                        devicesLoadedAt = now;
                        await SyncAccountsAsync(client, devices, ct);
                    }

                    foreach (var device in devices.Where(d => _nextPoll.GetValueOrDefault(d.Id) <= now))
                        await PollAsync(client, device, now, ct);

                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    if (_state.BrokerConnected || _state.BrokerError != ex.Message)
                        _logger.LogWarning("MQTT broker {Host}:{Port} není dostupný: {Error}", _settings.Host, _settings.Port, ex.Message);

                    _state.SetBrokerStatus(false, ex.Message);
                    await Task.Delay(ReconnectDelay, ct);
                }
            }
        }

        private async Task ConnectAsync(MqttClientFactory factory, IMqttClient client, CancellationToken ct)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_settings.Host, _settings.Port)
                .WithClientId(ClientId)
                .WithCleanSession(false);

            if (!string.IsNullOrEmpty(_settings.Username))
                options = options.WithCredentials(_settings.Username, _settings.Password);

            if (_settings.UseTls)
                options = options.WithTlsOptions(tls => tls.UseTls());

            await client.ConnectAsync(options.Build(), ct);

            var subscription = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(filter => filter.WithTopic(_settings.ResponseTopicFilter).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                .WithTopicFilter(filter => filter.WithTopic(DynamicSecurity.ResponseTopic))
                .Build();
            await client.SubscribeAsync(subscription, ct);

            // Po (novém) připojení broker nemusí účty znát — např. po změně hesla backendu se konfigurace založí znovu
            _provisioned.Clear();
            await client.PublishStringAsync(DynamicSecurity.CommandTopic, DynamicSecurity.GrantBackendRequests(string.Format(_settings.RequestTopicTemplate, "+")), cancellationToken: ct);

            _state.SetBrokerStatus(true, null);
            _logger.LogInformation("Připojeno k MQTT brokeru {Host}:{Port}, odebírám {Topic}", _settings.Host, _settings.Port, _settings.ResponseTopicFilter);
        }

        // Účty na brokeru kopírují gatewaye v databázi: nová gateway dostane účet, odebraná o něj přijde
        private async Task SyncAccountsAsync(IMqttClient client, List<PolledDevice> devices, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_settings.DeviceSecret))
                return;

            foreach (var device in devices.Where(d => !_provisioned.Contains(d.GatewayId)))
            {
                var credentials = GatewayCredentials.For(_settings.DeviceSecret, device.GatewayId);
                var payload = DynamicSecurity.ProvisionGateway(
                    credentials,
                    string.Format(_settings.RequestTopicTemplate, device.GatewayId),
                    string.Format(_settings.ResponseTopicTemplate, device.GatewayId));

                await client.PublishStringAsync(DynamicSecurity.CommandTopic, payload, cancellationToken: ct);
                _provisioned.Add(device.GatewayId);
            }

            var current = devices.Select(d => d.GatewayId).ToHashSet();
            foreach (var removed in _provisioned.Where(id => !current.Contains(id)).ToList())
            {
                await client.PublishStringAsync(DynamicSecurity.CommandTopic, DynamicSecurity.RemoveGateway(removed), cancellationToken: ct);
                _provisioned.Remove(removed);
            }
        }

        private async Task<List<PolledDevice>> LoadDevicesAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartAuditDbContext>();

            var devices = await dbContext.SensorDevices
                .AsNoTracking()
                .Select(d => new
                {
                    d.Id,
                    d.GatewayId,
                    d.PollSeconds,
                    d.NodeAddresses,
                    Channels = d.Channels.Select(c => new { c.NodeAddress, c.IqrfType, c.ChannelKey }).ToList(),
                })
                .ToListAsync(ct);

            return devices
                .Select(d => new PolledDevice(
                    d.Id,
                    d.GatewayId,
                    d.PollSeconds,
                    d.NodeAddresses,
                    d.Channels.Select(c => new PolledChannel(c.NodeAddress, c.IqrfType, IqrfResponseParser.ChannelOrdinal(c.ChannelKey))).ToList()))
                .ToList();
        }

        private async Task PollAsync(IMqttClient client, PolledDevice device, DateTimeOffset now, CancellationToken ct)
        {
            var topic = string.Format(_settings.RequestTopicTemplate, device.GatewayId);

            if (device.NodeAddresses.Length == 0)
            {
                await PublishAsync(client, device.GatewayId, topic, IqrfResponseParser.BondedDevices, IqrfResponseParser.BondedDevicesRequest(MessageId()), ct);
                _nextPoll[device.Id] = now + DiscoveryInterval;
                return;
            }

            // Objevení senzorů: čtení po uzlech u uzlů bez známých kanálů, a celé síti jednou za čas (nové čidlo, výměna)
            var knownNodes = device.Channels.Select(c => c.NodeAddress).ToHashSet();
            var rediscover = _nextDiscovery.GetValueOrDefault(device.Id) <= now;
            var readNodes = rediscover ? device.NodeAddresses : device.NodeAddresses.Where(n => !knownNodes.Contains(n)).ToArray();

            if (rediscover)
            {
                await PublishAsync(client, device.GatewayId, topic, IqrfResponseParser.BondedDevices, IqrfResponseParser.BondedDevicesRequest(MessageId()), ct);
                _nextDiscovery[device.Id] = now + RediscoveryInterval;
            }

            foreach (var node in readNodes)
                await PublishAsync(client, device.GatewayId, topic, IqrfResponseParser.ReadSensorsWithTypes, IqrfResponseParser.ReadSensorsRequest(node, MessageId()), ct);

            // Známé kanály hromadně: jeden FRC na každou veličinu (typ + pořadí senzoru toho typu) za celou síť.
            // U 30 uzlů se 3 veličinami 3 dotazy místo 30 — koordinátor není zahlcený a vyčtení trvá sekundy.
            if (!rediscover)
            {
                var polled = readNodes.ToHashSet();
                var frcGroups = device.Channels
                    .Where(c => !polled.Contains(c.NodeAddress) && IqrfResponseParser.SupportsFrc(c.IqrfType))
                    .Select(c => (c.IqrfType, c.Ordinal))
                    .Distinct();

                foreach (var (type, ordinal) in frcGroups)
                    await PublishAsync(client, device.GatewayId, topic, IqrfResponseParser.SensorFrc, IqrfResponseParser.FrcRequest(type, ordinal, MessageId()), ct);

                // Senzory, které FRC neumí (bloky dat), dál po uzlech
                foreach (var node in device.Channels.Where(c => !polled.Contains(c.NodeAddress) && !IqrfResponseParser.SupportsFrc(c.IqrfType)).Select(c => c.NodeAddress).Distinct())
                    await PublishAsync(client, device.GatewayId, topic, IqrfResponseParser.ReadSensorsWithTypes, IqrfResponseParser.ReadSensorsRequest(node, MessageId()), ct);
            }

            _nextPoll[device.Id] = now + TimeSpan.FromSeconds(device.PollSeconds);
        }

        private async Task PublishAsync(IMqttClient client, string gatewayId, string topic, string messageType, string payload, CancellationToken ct)
        {
            await client.PublishStringAsync(topic, payload, cancellationToken: ct);
            _state.RecordMessage(gatewayId, GatewayMessageDirection.Request, messageType, payload);
        }

        // gateway/{id}/iqrf/responses → id gatewaye je druhý segment
        private async Task HandleMessageAsync(string topic, string payload)
        {
            if (topic == DynamicSecurity.ResponseTopic)
            {
                foreach (var error in DynamicSecurity.Errors(payload))
                    _logger.LogWarning("Dynamic Security na brokeru hlásí chybu: {Error}", error);
                return;
            }

            var segments = topic.Split('/');
            if (segments.Length < 2 || string.IsNullOrWhiteSpace(segments[1]))
                return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<ISensorIngestService>().IngestAsync(segments[1], payload, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Zprávu z gatewaye {Topic} se nepodařilo zpracovat", topic);
            }
        }

        private static string MessageId() => $"sa-{Guid.NewGuid():N}"[..16];

        private record PolledDevice(Guid Id, string GatewayId, int PollSeconds, int[] NodeAddresses, List<PolledChannel> Channels);

        private record PolledChannel(int NodeAddress, int IqrfType, int Ordinal);
    }
}
