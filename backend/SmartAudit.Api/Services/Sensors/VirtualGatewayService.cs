using System.Collections.Concurrent;
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
    // Navíc hraje nespárované gatewaye z výroby (PairableDevices včetně zařízení z demo správy): ohlašují se na brokeru
    // a čekají, až je někdo připojí heslem.
    public class VirtualGatewayService : BackgroundService
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPipelineStateService _pipeline;
        private readonly IIqrfGatewayState _state;
        private readonly IqrfSettings _settings;
        private readonly ILogger<VirtualGatewayService> _logger;

        private readonly Dictionary<string, IMqttClient> _clients = new(StringComparer.OrdinalIgnoreCase);

        // Uzly v síti každé virtuální gatewaye — z databáze a hned i čidla přidaná kódem (než se uloží do databáze)
        private readonly ConcurrentDictionary<string, HashSet<int>> _bonded = new(StringComparer.OrdinalIgnoreCase);

        // Spárované gatewaye z výroby (mají záznam v databázi) — ty už se neohlašují
        private volatile HashSet<string> _claimed = new(StringComparer.OrdinalIgnoreCase);

        private IMqttClient? _pairingClient;

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

                    try
                    {
                        await AnnounceUnclaimedAsync(factory, ct);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogDebug("Nespárované virtuální gatewaye se neohlásily: {Error}", ex.Message);
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
                _pairingClient?.Dispose();
            }
        }

        // Klient pro každou virtuální gateway v databázi; odebraná gateway se odpojí, odpojená se připojí znovu
        // (účet na brokeru založí IqrfGatewayService až po uložení gatewaye, první pokusy tak mohou selhat)
        private async Task SyncClientsAsync(MqttClientFactory factory, CancellationToken ct)
        {
            List<(string GatewayId, int[] Nodes)> virtualDevices;
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SmartAuditDbContext>();

                // Zařízení z demo správy dřív, než se virtuální gatewaye připojí — jinak by po restartu odpovídaly jako učebna
                await DemoDeviceCatalog.ReloadAsync(dbContext, ct);
                var pairable = PairableDevices.All.Select(g => g.GatewayId).ToList();

                var devices = await dbContext.SensorDevices.AsNoTracking().Where(d => d.IsVirtual).Select(d => new { d.GatewayId, d.NodeAddresses }).ToListAsync(ct);
                virtualDevices = devices.Select(d => (d.GatewayId, d.NodeAddresses)).ToList();
                _claimed = (await dbContext.SensorDevices.AsNoTracking().Where(d => pairable.Contains(d.GatewayId)).Select(d => d.GatewayId).ToListAsync(ct))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            var gatewayIds = virtualDevices.Select(d => d.GatewayId).ToList();
            foreach (var (gatewayId, nodes) in virtualDevices)
            {
                var bonded = _bonded.GetOrAdd(gatewayId, _ => []);
                lock (bonded)
                    bonded.UnionWith(nodes);
            }

            foreach (var removed in _clients.Keys.Where(id => !gatewayIds.Contains(id, StringComparer.OrdinalIgnoreCase)).ToList())
            {
                await DisconnectAsync(_clients[removed], ct);
                _clients.Remove(removed);
                _bonded.TryRemove(removed, out _);
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

            var bonded = _bonded.GetOrAdd(gatewayId, _ => []);
            int[] snapshot;
            lock (bonded)
                snapshot = [.. bonded];

            var response = VirtualGatewayResponder.Respond(gatewayId, request, now, snapshot);
            if (response == null)
                return;

            // Čidlo přidané kódem je v síti hned — i než backend uloží novou adresu uzlu
            if (IqrfResponseParser.Parse(response) is { IsOk: true, AssignedAddress: { } assigned })
            {
                lock (bonded)
                    bonded.Add(assigned);
            }

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

        // Nespárované gatewaye z výroby: jeden klient se sdíleným účtem „provisioning“ ohlašuje všechny, které ještě nikdo nepřipojil,
        // a na nabídku odpoví podle toho, jestli jde rozšifrovat heslem z nálepky
        private async Task AnnounceUnclaimedAsync(MqttClientFactory factory, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_settings.ProvisioningPassword))
                return;

            var unclaimed = PairableDevices.All.Where(g => !_claimed.Contains(g.GatewayId)).ToList();
            if (unclaimed.Count == 0)
                return;

            if (_pairingClient == null)
            {
                var created = factory.CreateMqttClient();
                created.ApplicationMessageReceivedAsync += e => AnswerOfferAsync(created, e.ApplicationMessage.Topic, e.ApplicationMessage.ConvertPayloadToString());
                _pairingClient = created;
            }

            if (!_pairingClient.IsConnected)
            {
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_settings.Host, _settings.Port)
                    .WithClientId("virtual-pairing")
                    .WithCredentials(PairingProtocol.AccountName, _settings.ProvisioningPassword)
                    .WithCleanSession();

                if (_settings.UseTls)
                    options = options.WithTlsOptions(tls => tls.UseTls());

                await _pairingClient.ConnectAsync(options.Build(), ct);
                await _pairingClient.SubscribeAsync(factory.CreateSubscribeOptionsBuilder().WithTopicFilter(filter => filter.WithTopic(PairingProtocol.OfferFilter)).Build(), ct);
            }

            var now = DateTimeOffset.UtcNow;
            foreach (var gateway in unclaimed)
            {
                // Síla signálu jen pro obrázek v aplikaci — každá krabička jiná, ale stálá
                var rssi = -52 - Math.Abs(StableHash.Of(gateway.Serial) % 4) * 6;
                await _pairingClient.PublishStringAsync(PairingProtocol.AnnounceTopic(gateway.Serial), PairingProtocol.Announcement(gateway, rssi, now), cancellationToken: ct);
            }
        }

        private async Task AnswerOfferAsync(IMqttClient client, string topic, string offer)
        {
            var gateway = PairingProtocol.OfferSerial(topic) is { } serial ? PairableDevices.FindGateway(serial) : null;
            if (gateway == null || _claimed.Contains(gateway.GatewayId))
                return;

            var accepted = PairingProtocol.OpenOffer(gateway.Password, gateway.Serial, offer) != null;

            try
            {
                if (accepted)
                {
                    // Spárovaná gateway se přestane ohlašovat hned; kdyby ji aplikace nakonec neuložila, další obnova ji z databáze vrátí
                    _claimed = new HashSet<string>(_claimed, StringComparer.OrdinalIgnoreCase) { gateway.GatewayId };
                    await client.PublishStringAsync(PairingProtocol.AnnounceTopic(gateway.Serial), string.Empty);
                }

                await client.PublishStringAsync(PairingProtocol.ResultTopic(gateway.Serial), PairingProtocol.Result(accepted));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Virtuální gateway {Serial} neodpověděla na nabídku: {Error}", gateway.Serial, ex.Message);
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
