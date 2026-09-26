using System.Collections.Concurrent;

namespace SmartAudit.Api.Services.Sensors
{
    // Zpráva, kterou za handler odešle spojení backendu s brokerem. GatewayId a typ zprávy jen pro log komunikace gatewaye.
    public record OutgoingMessage(string Topic, string Payload, string? GatewayId = null, string? MessageType = null);

    // Dotazy z HTTP požadavků (přidat čidlo, párovat gatewayi) přes jediné MQTT spojení backendu (IqrfGatewayService):
    // handler zprávu zařadí a počká na odpověď se stejným klíčem — u IQRF dotazu msgId, u párování serial gatewaye.
    public interface IIqrfCommandBus
    {
        Task<string?> RequestAsync(OutgoingMessage message, string replyKey, TimeSpan timeout, CancellationToken ct);

        bool TryDequeue(out OutgoingMessage message);

        // Počká, až bude co odeslat (nejdéle max) — smyčka spojení tak odešle dotaz hned, ne až po své pauze
        Task WaitForMessagesAsync(TimeSpan max, CancellationToken ct);

        void Complete(string replyKey, string payload);
    }

    public class IqrfCommandBus : IIqrfCommandBus
    {
        private readonly ConcurrentQueue<OutgoingMessage> _outgoing = new();
        private readonly SemaphoreSlim _signal = new(0);
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pending = new(StringComparer.OrdinalIgnoreCase);

        public static string MessageKey(string messageId) => $"msg:{messageId}";

        public static string PairingKey(string serial) => $"pairing:{serial}";

        public async Task<string?> RequestAsync(OutgoingMessage message, string replyKey, TimeSpan timeout, CancellationToken ct)
        {
            var reply = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Na jeden klíč čeká jen jeden dotaz (dvě párování téže gatewaye naráz by si odpovědi pletla)
            if (!_pending.TryAdd(replyKey, reply))
                return null;

            try
            {
                _outgoing.Enqueue(message);
                _signal.Release();

                return await reply.Task.WaitAsync(timeout, ct);
            }
            catch (TimeoutException)
            {
                return null;
            }
            finally
            {
                _pending.TryRemove(new KeyValuePair<string, TaskCompletionSource<string>>(replyKey, reply));
            }
        }

        public bool TryDequeue(out OutgoingMessage message) => _outgoing.TryDequeue(out message!);

        public async Task WaitForMessagesAsync(TimeSpan max, CancellationToken ct)
        {
            if (!_outgoing.IsEmpty)
                return;

            await _signal.WaitAsync(max, ct);
        }

        public void Complete(string replyKey, string payload)
        {
            if (_pending.TryGetValue(replyKey, out var reply))
                reply.TrySetResult(payload);
        }
    }
}
