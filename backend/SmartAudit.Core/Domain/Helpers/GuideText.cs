using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartAudit.Core.Domain.Helpers
{
    // Normalizace textu pro průvodce — vyhledávání ve znalostech i ochranné filtry pracují nad stejnou podobou textu:
    // bez diakritiky, malými písmeny, bez neviditelných znaků a s jednou mezerou mezi slovy
    public static partial class GuideText
    {
        // Slova bez obsahu, která by ve vyhledávání jen přidávala šum
        private static readonly HashSet<string> StopWords =
        [
            "a", "i", "o", "u", "v", "ve", "na", "do", "z", "ze", "s", "se", "k", "ke", "je", "jsou", "to", "ten", "ta", "ty", "co", "jak",
            "kde", "kdy", "proc", "ktery", "ktera", "ktere", "mi", "me", "mne", "si", "by", "aby", "pro", "po", "pri", "za", "od", "ale",
            "nebo", "jaky", "jaka", "jake", "tady", "tam", "jen", "uz", "jeste", "byt", "mam", "ma", "maji", "muze", "muzu", "lze", "prosim",
        ];

        public static string Normalize(string text)
        {
            var decomposed = text.Normalize(NormalizationForm.FormKD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var ch in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);

                // Diakritika a neviditelné znaky (zero-width, řídicí) — obojí se dá použít k obejití filtrů
                if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.Format or UnicodeCategory.Control)
                    continue;

                builder.Append(char.ToLowerInvariant(ch));
            }

            return Whitespace().Replace(builder.ToString(), " ").Trim();
        }

        // Varianta pro ochranné filtry: čísla a znaky místo písmen („1gn0ruj“, „k@d“) se převedou zpět na písmena
        public static string NormalizeForGuard(string text)
        {
            var normalized = Normalize(text);
            var builder = new StringBuilder(normalized.Length);

            foreach (var ch in normalized)
            {
                builder.Append(ch switch
                {
                    '0' => 'o',
                    '1' or '!' or '|' => 'i',
                    '3' => 'e',
                    '4' or '@' => 'a',
                    '5' or '$' => 's',
                    '7' => 't',
                    _ => ch,
                });
            }

            return builder.ToString();
        }

        // Jen písmena za sebou — „i g n o r u j“ nebo „ign.oruj“ se pak chytí stejně jako „ignoruj“
        public static string Squash(string normalized) => NonLetters().Replace(normalized, string.Empty);

        // Slova pro vyhledávání: bez stop slov a zkrácená na kmen (čeština skloňuje, „školy“ a „škole“ mají najít totéž)
        public static IReadOnlyList<string> Terms(string text)
        {
            return WordSplit().Split(Normalize(text))
                .Where(w => w.Length > 1 && !StopWords.Contains(w))
                .Select(Stem)
                .ToList();
        }

        // Hrubý kmen: prvních 5 znaků delších slov — na krátkou znalostní bázi stačí a nepotřebuje slovník
        public static string Stem(string word) => word.Length > 6 ? word[..5] : word;

        [GeneratedRegex(@"\s+")]
        private static partial Regex Whitespace();

        [GeneratedRegex(@"[^\p{L}]+")]
        private static partial Regex NonLetters();

        [GeneratedRegex(@"[^\p{L}\p{Nd}₂]+")]
        private static partial Regex WordSplit();
    }
}
