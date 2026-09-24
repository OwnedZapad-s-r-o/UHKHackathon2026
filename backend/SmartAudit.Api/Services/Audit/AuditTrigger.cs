using System.Collections.Concurrent;

namespace SmartAudit.Api.Services.Audit
{
    // Fronta budov k přepočtu auditu: nová měření ze skutečných čidel, přiřazení nebo odpojení čidla.
    // Smyčka simulace si ji vyzvedne a přepočítá jen tyto budovy — ne celý kraj.
    public interface IAuditTrigger
    {
        void RequestFacility(Guid facilityId);
        IReadOnlyCollection<Guid> DrainFacilities();
    }

    public class AuditTrigger : IAuditTrigger
    {
        private readonly ConcurrentDictionary<Guid, byte> _pending = new();

        public void RequestFacility(Guid facilityId) => _pending.TryAdd(facilityId, 0);

        public IReadOnlyCollection<Guid> DrainFacilities()
        {
            var drained = new List<Guid>();
            foreach (var facilityId in _pending.Keys)
            {
                if (_pending.TryRemove(facilityId, out _))
                    drained.Add(facilityId);
            }

            return drained;
        }
    }
}
