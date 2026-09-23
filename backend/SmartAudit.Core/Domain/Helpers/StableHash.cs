using System.Text;

namespace SmartAudit.Core.Domain.Helpers
{
    // string.GetHashCode() je v .NET náhodný pro každý proces — pro opakovatelnou simulaci potřebujeme stabilní hash
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

        // Rovnoměrné číslo 0–99 pro rozdělení do profilů
        public static int Percent(string value)
        {
            return (int)((uint)Of(value) % 100);
        }
    }
}
