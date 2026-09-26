using System.Text.RegularExpressions;

namespace SmartAudit.Core.Domain.Helpers
{
    public record ParsedAddress(string? Street, string? HouseNumber, string? Psc, string? Post);

    // Adresa z vyhledávací služby RÚIAN (ČÚZK) ve tvaru „Ulice 1245/2, Část obce, 50003 Pošta“ nebo „č.p. 16, 50327 Radostov“
    public static partial class RuianAddress
    {
        public static ParsedAddress Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new ParsedAddress(null, null, null, null);

            var parts = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            string? psc = null;
            string? post = null;
            if (parts.Length > 1 && PostPattern().Match(parts[^1]) is { Success: true } postMatch)
            {
                psc = postMatch.Groups["psc"].Value.Replace(" ", string.Empty);
                post = postMatch.Groups["post"].Value;
            }

            var first = parts.Length > 0 ? parts[0] : string.Empty;
            var numberMatch = NumberPattern().Match(first);
            if (!numberMatch.Success)
                return new ParsedAddress(first.Length > 0 ? first : null, null, psc, post);

            var street = numberMatch.Groups["street"].Value.Trim();

            return new ParsedAddress(street.Length > 0 ? street : null, numberMatch.Groups["number"].Value, psc, post);
        }

        // PSČ Královéhradeckého kraje: Hradecko a Jičínsko 500–509, Rychnovsko 516–518, Trutnovsko a Náchodsko 541–552.
        // Hrubé síto pro našeptávač (služba RÚIAN neumí omezit hledání na území) — přesně rozhodne až obec z polohy.
        public static bool IsRegionalPostalCode(string? psc)
        {
            if (psc == null || psc.Length < 3 || !int.TryParse(psc[..3], out var prefix))
                return false;

            return prefix is >= 500 and <= 509 or >= 516 and <= 518 or >= 541 and <= 552;
        }

        [GeneratedRegex(@"^(?<psc>\d{3}\s?\d{2})\s+(?<post>.+)$")]
        private static partial Regex PostPattern();

        // „Pivovarské náměstí 1245/2“, „Komenského 63a“, „č.p. 16“, „č.ev. 12“
        [GeneratedRegex(@"^(?<street>.*?)\s*(?<number>(č\.\s?(p|ev)\.\s*)?\d+[a-zA-Z]?(/\d+[a-zA-Z]?)?)$")]
        private static partial Regex NumberPattern();
    }
}
