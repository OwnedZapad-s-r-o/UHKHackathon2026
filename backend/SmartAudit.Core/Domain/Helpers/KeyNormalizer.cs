namespace SmartAudit.Core.Domain.Helpers
{
    // Sady KHK zapisují identifikátory nejednotně ("    110029283", "060153296" vs "60153296", IČO bez nul)
    public static class KeyNormalizer
    {
        public static string? Izo(string? value) => PadDigits(value, 9);

        public static string? Redizo(string? value) => PadDigits(value, 9);

        public static string? Ico(string? value) => PadDigits(value, 8);

        // Prázdné hodnoty, mezery a "0" (tak sady značí chybějící údaj) převede na null
        public static string? Clean(string? value)
        {
            if (value == null)
                return null;

            var trimmed = value.Trim();

            return trimmed.Length == 0 || trimmed == "0" ? null : trimmed;
        }

        private static string? PadDigits(string? value, int length)
        {
            var cleaned = Clean(value);

            if (cleaned == null)
                return null;

            var digits = new string(cleaned.Where(char.IsDigit).ToArray());

            if (digits.Length == 0 || digits.Length > length)
                return null;

            return digits.PadLeft(length, '0');
        }
    }
}
