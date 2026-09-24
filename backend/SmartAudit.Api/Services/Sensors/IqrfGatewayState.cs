using System.Collections.Concurrent;

namespace SmartAudit.Api.Services.Sensors
{
    public enum GatewayMessageDirection
    {
        Request,
        Response,
    }

    public record GatewayMessage(DateTimeOffset Timestamp, GatewayMessageDirection Direction, string MessageType, string Payload);

    public record ChannelSample(DateTimeOffset Timestamp, double Value);

    public record UnknownGateway(string GatewayId, DateTimeOffset LastSeenAt, int MessageCount);

    // Živý stav IQRF komunikace v paměti — log zpráv a průběh hodnot pro průvodce připojením.
    // Do databáze se ukládají jen hodnoty kanálů přiřazených do místnosti.
    public interface IIqrfGatewayState
    {
        bool BrokerConnected { get; }
        string? BrokerError { get; }
        void SetBrokerStatus(bool connected, string? error);

        void RecordMessage(string gatewayId, GatewayMessageDirection direction, string messageType, string payload);
        IReadOnlyList<GatewayMessage> GetMessages(string gatewayId);

        void RecordSample(string gatewayId, string channelKey, ChannelSample sample);
        IReadOnlyList<ChannelSample> GetSamples(string gatewayId, string channelKey);

        void RecordUnknown(string gatewayId, DateTimeOffset timestamp);
        void ForgetUnknown(string gatewayId);
        IReadOnlyList<UnknownGateway> GetUnknown();
    }

    public class IqrfGatewayState : IIqrfGatewayState
    {
        private const int MaxMessages = 50;
        private const int MaxSamples = 240;

        private readonly ConcurrentDictionary<string, LinkedList<GatewayMessage>> _messages = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<(string, string), LinkedList<ChannelSample>> _samples = new();
        private readonly ConcurrentDictionary<string, UnknownGateway> _unknown = new(StringComparer.OrdinalIgnoreCase);

        public bool BrokerConnected { get; private set; }
        public string? BrokerError { get; private set; }

        public void SetBrokerStatus(bool connected, string? error)
        {
            BrokerConnected = connected;
            BrokerError = error;
        }

        public void RecordMessage(string gatewayId, GatewayMessageDirection direction, string messageType, string payload)
        {
            Append(_messages.GetOrAdd(gatewayId, _ => new()), new GatewayMessage(DateTimeOffset.UtcNow, direction, messageType, payload), MaxMessages);
        }

        public IReadOnlyList<GatewayMessage> GetMessages(string gatewayId)
        {
            return _messages.TryGetValue(gatewayId, out var list) ? Snapshot(list).AsEnumerable().Reverse().ToList() : [];
        }

        public void RecordSample(string gatewayId, string channelKey, ChannelSample sample)
        {
            Append(_samples.GetOrAdd((gatewayId.ToLowerInvariant(), channelKey), _ => new()), sample, MaxSamples);
        }

        public IReadOnlyList<ChannelSample> GetSamples(string gatewayId, string channelKey)
        {
            return _samples.TryGetValue((gatewayId.ToLowerInvariant(), channelKey), out var list) ? Snapshot(list) : [];
        }

        public void RecordUnknown(string gatewayId, DateTimeOffset timestamp)
        {
            _unknown.AddOrUpdate(gatewayId, id => new UnknownGateway(id, timestamp, 1), (_, current) => current with { LastSeenAt = timestamp, MessageCount = current.MessageCount + 1 });
        }

        public void ForgetUnknown(string gatewayId) => _unknown.TryRemove(gatewayId, out _);

        public IReadOnlyList<UnknownGateway> GetUnknown() => _unknown.Values.OrderByDescending(g => g.LastSeenAt).ToList();

        private static void Append<T>(LinkedList<T> list, T item, int max)
        {
            lock (list)
            {
                list.AddLast(item);
                while (list.Count > max)
                    list.RemoveFirst();
            }
        }

        private static List<T> Snapshot<T>(LinkedList<T> list)
        {
            lock (list)
                return list.ToList();
        }
    }
}
