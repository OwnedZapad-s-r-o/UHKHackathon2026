using System.Text;

namespace SmartAudit.Core.Domain.Helpers
{
    // string.GetHashCode() je v .NET náhodný pro každý proces — stabilní hash slouží jen jako semínko šumu čidel (±0,1 °C),
    // stav budovy z něj nikdy nevzniká (ten počítá OpenDataPreAudit z otevřených dat)
    public static class StableHash
    {
        public static int Of(string value)
        {
            // FNV-1a 32 bit
            var hash = 2166136261;

            foreach (var b in Encoding.UTF8.GetBytes(value))
            {
                hash ^= b;
                hash *= 16777619;
            }

            return unchecked((int)hash);
        }
    }
}
