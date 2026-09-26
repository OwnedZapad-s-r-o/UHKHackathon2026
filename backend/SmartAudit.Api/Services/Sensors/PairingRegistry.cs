using System.Collections.Concurrent;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Sensors
{
    public record DiscoveredGateway(PairingAnnouncement Announcement, DateTimeOffset LastSeenAt);

    // Nespárované gatewaye, které se právě ohlašují na brokeru (jen v paměti — po restartu se do pár sekund ohlásí znovu),
    // a omezení pokusů o heslo: po 5 špatných pokusech za 10 minut se párování zařízení na čas zablokuje.
    public interface IPairingRegistry
    {
        void Record(PairingAnnouncement announcement, DateTimeOffset now);
        void Forget(string serial);
        IReadOnlyList<DiscoveredGateway> Online(DateTimeOffset now);
        DiscoveredGateway? Find(string serial, DateTimeOffset now);

        // Klíč pokusů: serial gatewaye, u čidel id gatewaye, do jejíž sítě se přidávají
        TimeSpan? BlockedFor(string key, DateTimeOffset now);
        void RecordFailure(string key, DateTimeOffset now);
        void ResetFailures(string key);
    }

    public class PairingRegistry : IPairingRegistry
    {
        // Zařízení se ohlašuje každých 10 s; po třech vynechaných ohlášeních ho považujeme za vypnuté
        private static readonly TimeSpan OnlineWindow = TimeSpan.FromSeconds(30);

        private const int MaxFailures = 5;
        private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(10);

        private readonly ConcurrentDictionary<string, DiscoveredGateway> _gateways = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, List<DateTimeOffset>> _failures = new(StringComparer.OrdinalIgnoreCase);

        public void Record(PairingAnnouncement announcement, DateTimeOffset now) => _gateways[announcement.Serial] = new DiscoveredGateway(announcement, now);

        public void Forget(string serial) => _gateways.TryRemove(serial, out _);

        public IReadOnlyList<DiscoveredGateway> Online(DateTimeOffset now) =>
            _gateways.Values.Where(g => now - g.LastSeenAt < OnlineWindow).OrderBy(g => g.Announcement.Serial).ToList();

        public DiscoveredGateway? Find(string serial, DateTimeOffset now) =>
            _gateways.TryGetValue(serial, out var gateway) && now - gateway.LastSeenAt < OnlineWindow ? gateway : null;

        public TimeSpan? BlockedFor(string key, DateTimeOffset now)
        {
            if (!_failures.TryGetValue(key, out var failures))
                return null;

            lock (failures)
            {
                failures.RemoveAll(f => now - f >= FailureWindow);
                return failures.Count >= MaxFailures ? failures[^MaxFailures] + FailureWindow - now : null;
            }
        }

        public void RecordFailure(string key, DateTimeOffset now)
        {
            var failures = _failures.GetOrAdd(key, _ => []);
            lock (failures)
                failures.Add(now);
        }

        public void ResetFailures(string key) => _failures.TryRemove(key, out _);
    }
}
