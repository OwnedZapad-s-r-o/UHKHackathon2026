namespace SmartAudit.Api.Services.Audit
{
    // Příznak „přišla nová skutečná měření“ — smyčka simulace pak přepočítá audit, i když sama nic nezapsala
    public interface IAuditTrigger
    {
        void Request();
        bool TryConsume();
    }

    public class AuditTrigger : IAuditTrigger
    {
        private int _requested;

        public void Request() => Interlocked.Exchange(ref _requested, 1);

        public bool TryConsume() => Interlocked.Exchange(ref _requested, 0) == 1;
    }
}
