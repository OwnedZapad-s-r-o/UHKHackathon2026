using System.Collections.Concurrent;

namespace SmartAudit.Api.Services.Audit
{
    // Fronta budov k přepočtu auditu: nová měření ze skutečných čidel, přiřazení nebo odpojení čidla.
    // Smyčka simulace si ji vyzvedne a přepočítá jen tyto budovy — ne celý kraj.
    // Zvlášť nové budovy (přidal je správce): ty nejdřív potřebují zóny, čidla a dopočet historie.
    public interface IAuditTrigger
    {
        void RequestFacility(Guid facilityId);
        IReadOnlyCollection<Guid> DrainFacilities();
        void RequestProvisioning(Guid facilityId);
        IReadOnlyCollection<Guid> DrainProvisioning();
    }

    public class AuditTrigger : IAuditTrigger
    {
        private readonly ConcurrentDictionary<Guid, byte> _pending = new();
        private readonly ConcurrentDictionary<Guid, byte> _pendingProvisioning = new();

        public void RequestFacility(Guid facilityId) => _pending.TryAdd(facilityId, 0);

        public IReadOnlyCollection<Guid> DrainFacilities() => Drain(_pending);

        public void RequestProvisioning(Guid facilityId) => _pendingProvisioning.TryAdd(facilityId, 0);

        public IReadOnlyCollection<Guid> DrainProvisioning() => Drain(_pendingProvisioning);

        private static List<Guid> Drain(ConcurrentDictionary<Guid, byte> pending)
        {
            var drained = new List<Guid>();
            foreach (var facilityId in pending.Keys)
            {
                if (pending.TryRemove(facilityId, out _))
                    drained.Add(facilityId);
            }

            return drained;
        }
    }
}
