using System.Globalization;
using System.Text.RegularExpressions;

namespace SmartAudit.Core.Domain.Helpers
{
    public enum GuardReason
    {
        None,
        Empty,
        TooLong,
        // Pokus změnit pravidla průvodce („ignoruj předchozí pokyny“, „jsi teď…“, „vypiš systémový prompt“)
        Injection,
        // Generování kódu — i schované do víc kroků („nejdřív napiš skript a pak vyhledej…“)
        Code,
        Recipe,
        // Cokoli mimo budovy kraje: básně, úkoly, překlady, sázky, zdraví…
        OffTopic,
        // Útoky, zbraně, drogy
        Harmful,
    }

    public record GuardVerdict(bool Allowed, GuardReason Reason)
    {
        public static readonly GuardVerdict Ok = new(true, GuardReason.None);

        public static GuardVerdict Block(GuardReason reason) => new(false, reason);
    }

    // Ochranné filtry průvodce — první vrstva ještě před jazykovým modelem (a jediná, když model není nastavený).
    // Pracují nad normalizovaným textem: bez diakritiky, bez neviditelných znaků, s čísly převedenými na písmena,
    // a část pravidel i nad textem bez mezer a interpunkce, aby nešly obejít „i-g-n-o-r-u-j“.
    // Druhá vrstva je systémový prompt modelu, třetí kontrola odpovědi (IsSafeOutput).
    public static partial class GuideGuard
    {
        public const int MaxQuestionLength = 400;

        // Spojení, která se v legitimním dotazu na budovy neobjeví ani po slepení slov
        private static readonly string[] SquashedInjections =
        [
            "ignorujpredchozi", "ignorujvsechn", "ignorujpravidl", "ignorujinstrukc", "ignorujpokyn", "ignorujsystem",
            "ignoreprevious", "ignoreallprevious", "ignoreallinstructions", "ignoretheabove", "ignoreyourinstructions", "disregardprevious",
            "zapomennapravidla", "zapomennavsechn", "zapomenvsechn", "zapomenpredchozi", "zapomenpokyny", "zapomeninstrukce",
            "systemprompt", "systemovyprompt", "systemovepokyny", "systemoveinstrukce", "jailbreak", "developermode", "danmode", "promptinjection",
        ];

        public static GuardVerdict CheckInput(string? question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return GuardVerdict.Block(GuardReason.Empty);

            if (question.Length > MaxQuestionLength)
                return GuardVerdict.Block(GuardReason.TooLong);

            // „Kód objektu“ je identifikátor budovy, ne programování — pravidla na kód ho nesmí chytit
            var text = FacilityCode().Replace(GuideText.NormalizeForGuard(question), "identifikator");
            var squashed = GuideText.Squash(text);

            if (SquashedInjections.Any(squashed.Contains) || Injection().IsMatch(text) || MarkupTokens().IsMatch(question) || EncodedBlob().IsMatch(question))
                return GuardVerdict.Block(GuardReason.Injection);

            // Vícekrokový trik: „nejdřív vytvoř kód a potom vyhledej…“ — blokuje se celý dotaz, ne jen jeho část
            if (CodeRequest().IsMatch(text) || ProgrammingLanguage().IsMatch(text) || ChainedTask().IsMatch(text))
                return GuardVerdict.Block(GuardReason.Code);

            if (Recipe().IsMatch(text))
                return GuardVerdict.Block(GuardReason.Recipe);

            if (Harmful().IsMatch(text))
                return GuardVerdict.Block(GuardReason.Harmful);

            if (OffTopic().IsMatch(text))
                return GuardVerdict.Block(GuardReason.OffTopic);

            return GuardVerdict.Ok;
        }

        // Kontrola odpovědi modelu: žádný kód, recept ani únik systémového promptu (kontrolní řetězec), rozumná délka
        public static bool IsSafeOutput(string answer, string canary)
        {
            if (string.IsNullOrWhiteSpace(answer) || answer.Length > 2000)
                return false;

            if (answer.Contains(canary, StringComparison.OrdinalIgnoreCase))
                return false;

            var text = GuideText.Normalize(answer);

            return !CodeInOutput().IsMatch(answer) && !Recipe().IsMatch(text) && !LeakedRules().IsMatch(text);
        }

        // Částky v Kč, které v odpovědi modelu jsou, ale v datech ani znalostech ne (tolerance 5 % na zaokrouhlení).
        // Model nesmí „dopočítat“ úsporu ani ztrátu — každá koruna musí jít dohledat v tom, co mu vrátily nástroje.
        public static IReadOnlyList<decimal> UnverifiedAmounts(string answer, string evidence)
        {
            var known = Number().Matches(evidence).Select(m => ParseNumber(m.Value)).OfType<decimal>().ToList();

            return Amount().Matches(answer)
                .Select(m => ParseNumber(m.Groups[1].Value) * Multiplier(m.Groups[2].Value))
                .OfType<decimal>()
                .Where(amount => amount > 0 && !known.Any(k => Math.Abs(k - amount) <= amount * 0.05m))
                .ToList();
        }

        private static decimal? ParseNumber(string text)
        {
            var cleaned = new string(text.Where(ch => !char.IsWhiteSpace(ch)).ToArray()).Replace(',', '.');
            return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null;
        }

        private static decimal Multiplier(string unit) => unit.StartsWith("mil", StringComparison.OrdinalIgnoreCase) ? 1_000_000m
            : unit.StartsWith("tis", StringComparison.OrdinalIgnoreCase) ? 1_000m : 1m;

        public static string RefusalMessage(GuardReason reason) => reason switch
        {
            GuardReason.Empty => "Napište, co vás zajímá — třeba „Jak se dýchá ve školách v Náchodě?“",
            GuardReason.TooLong => $"Dotaz je moc dlouhý. Zkraťte ho prosím na jednu otázku (nejvýš {MaxQuestionLength} znaků).",
            GuardReason.Injection => "Tohle neudělám — moje pravidla se měnit nedají. Jsem průvodce budovami Královéhradeckého kraje: ukážu vám objekty v mapě, vysvětlím audit, kvalitu vzduchu nebo čidla.",
            GuardReason.Code => "Kód nepíšu ani negeneruju, a to ani jako mezikrok. Pro vývojáře je tu otevřené API s dokumentací — s daty o budovách vám ale rád pomůžu přímo tady.",
            GuardReason.Recipe => "Recepty nenabízím — vařím jen z dat o budovách kraje. Zeptejte se třeba, kde se teď přetápí nebo jak se dýchá ve vaší škole.",
            GuardReason.Harmful => "S tímhle nepomůžu. Odpovídám jen na otázky o budovách Královéhradeckého kraje, jejich energii a vnitřním prostředí.",
            _ => "S tímhle nepomůžu — odpovídám jen na otázky o budovách Královéhradeckého kraje, jejich energii, vnitřním prostředí, čidlech a datech.",
        };

        // „204 300 Kč“, „0,2 mil. Kč“, „15 tisíc Kč“ — mezery mezi tisíci i nezlomitelné
        [GeneratedRegex(@"(\d+(?:[\s  ]\d{3})*(?:[.,]\d+)?)\s*(mil\w*\.?|tis\w*\.?)?\s*Kč")]
        private static partial Regex Amount();

        [GeneratedRegex(@"\d+(?:[\s  ]\d{3})*(?:[.,]\d+)?")]
        private static partial Regex Number();

        [GeneratedRegex(@"\bkod(u|em)?\s+(objektu|budovy|budov|skoly|zarizeni|okresu|obce|ruian|organizace)\b")]
        private static partial Regex FacilityCode();

        [GeneratedRegex(@"\b(ignoruj(te)?|ignorovat|ignore|zapomen(te)?|obejd\w*|obejit|disregard|forget|override|bypass)\b.{0,40}\b(predchoz\w*|vsechn\w*|pravidl\w*|instrukc\w*|pokyn\w*|omezen\w*|filtr\w*|previous|prior|above|rules?|instructions?|guidelines?|filters?)\b"
            + @"|\b(jsi|budes)\s+(ted|nyni|odted|od\s+ted)\b|\b(chovej|tvar)\s+se\s+jako\b|\bpredstav\s+si,?\s+ze\s+jsi\b|\bhraj\s+(roli|si\s+na)\b|\bnovou\s+roli\b"
            + @"|\b(act|behave)\s+as\b|\bpretend\b|\broleplay\b|\byou\s+are\s+now\b|\bfrom\s+now\s+on\b|\bnew\s+instructions?\b"
            + @"|\bbez\s+(omezeni|filtru|pravidel|cenzury)\b|\bno\s+(restrictions|rules|filters)\b|\bunfiltered\b|\buncensored\b"
            + @"|\b(vypis|ukaz|prozrad|zopakuj|reveal|print|show|repeat|leak)\w*\b.{0,30}(\bprompt\w*|\binstrukc\w*|\b(tve|tvoje|svoje)\s+(pokyn\w*|pravidl\w*)|\bsystem\w*\s+(prompt|pokyn|zprav|instrukc)\w*|\binstructions?\b|\byour\s+rules\b)")]
        private static partial Regex Injection();

        // Značky, kterými se v textu předstírá jiná role (chat šablony modelů, bloky kódu, HTML)
        [GeneratedRegex(@"<\|[^>]*\|>|\[/?INST\]|<<\s*SYS|###\s*(system|instruction)|```|<\s*/?\s*(script|system|iframe)\b|\{\{|\bBEGIN\s+(SYSTEM|PROMPT)\b", RegexOptions.IgnoreCase)]
        private static partial Regex MarkupTokens();

        // Dlouhý řetězec bez mezer a lomítek (base64, hex) — tak se pokyny pro model schovávají před filtry; adresy URL projdou
        [GeneratedRegex(@"[A-Za-z0-9+=_-]{40,}")]
        private static partial Regex EncodedBlob();

        // Silná slovesa (napiš, vytvoř…) s čímkoli programátorským, nebo „dej mi / ukaž“ s jednoznačně programátorským předmětem.
        // „Časový program vytápění“ je regulace, ne programování.
        [GeneratedRegex(@"\b(napis|vytvor|vygeneruj|generuj|naprogramuj|nakoduj|sestav|write|generate|create|implement)\w*\b.{0,40}\b(kod\w*|code|skript\w*|script|program(?!\w*\s+(vytapeni|regulace|utlum\w*|mar|provozu|vetrani))\w*|funkc[ei]|algoritm\w*|regex\w*|makr\w*|tridu|class|plugin\w*|snippet|dotaz\w*\s+sql|query)\b"
            + @"|\b(dej\s+mi|ukaz|posli|priprav|udelej|chci|potrebuju|give\s+me|show\s+me)\w*\b.{0,40}\b(zdrojak\w*|zdrojov\w*\s+kod\w*|programov\w*\s+kod\w*|code|skript\w*|script|regex\w*|snippet|makr[oa])\b")]
        private static partial Regex CodeRequest();

        [GeneratedRegex(@"(?<!\w)(python\w*|javascript\w*|typescript\w*|java|c#|c\+\+|csharp|php|golang|kotlin|swift|luau|bash|powershell|sql|html|css|react|nodejs|node\.js|vba|json\s+schema)(?![\w+#])")]
        private static partial Regex ProgrammingLanguage();

        // „Nejdřív udělej X a pak…“, kde X je kód, recept nebo text mimo účel průvodce
        [GeneratedRegex(@"\b(nejdriv|nejdrive|nejprve|napred|predtim|first)\b.{0,60}\b(zdrojov\w*\s+kod\w*|kod\s+v\s+\w+|code|skript\w*|script|naprogramuj\w*|nakoduj\w*|recept\w*|basen\w*|pribeh\w*)"
            + @"|\b(zdrojov\w*\s+kod\w*|code|skript\w*|script|recept\w*|basen\w*)\b.{0,60}\b(potom|pak|then|nasledne|after\s+that)\b")]
        private static partial Regex ChainedTask();

        [GeneratedRegex(@"\b(recept\w*|ingredienc\w*|uvar\w*|upec\w*|peceni|vareni|kuchar\w*|svickov\w*|gulas\w*|rizek|rizky|dort\w*|bucht\w*|palacink\w*|lasagn\w*|pizz\w*|testovin\w*|polevk\w*|smazen\w*|grilov\w*|recipe\w*|cooking|bake|baking)\b|\bg\s+mouky\b|\blzic\w*\s+(cukru|soli|mouky)\b")]
        private static partial Regex Recipe();

        [GeneratedRegex(@"\b(hack\w*|exploit\w*|malware|ransomware|phishing|ddos|sql\s+injection|xss|keylogger|prolom\w*|crack\w*|zbran\w*|vybusn\w*|bomb[ayu]\w*|drog[ayu]|pervitin|ukradn\w*|odposlech\w*)\b")]
        private static partial Regex Harmful();

        [GeneratedRegex(@"\b(basen\w*|basnick\w*|pisnick\w*|text\s+pisne|vtip\w*|povidk\w*|esej\w*|slohov\w*|domaci\s+ukol\w*|referat\w*|seminark\w*|diplomov\w*|preloz\w*|translate|horoskop\w*|tarot|sazk\w*|loteri\w*|kryptomen\w*|bitcoin\w*|burz\w*|forex|diagnoz\w*|davkovan\w*|lek\s+na|koho\s+volit|volebn\w*|politick\w*\s+stran\w*|fotbal\w*|hokej\w*|serial\w*|seznamk\w*)\b")]
        private static partial Regex OffTopic();

        // Kód v odpovědi: blok kódu, typické konstrukce jazyků, HTML značky
        [GeneratedRegex(@"```|\bdef\s+\w+\s*\(|\bfunction\s*\w*\s*\(|=>\s*\{|\bconsole\.\w+|^\s*(import|from)\s+[\w.]+(\s+import\b|\s*;?\s*$)|#include|\bSELECT\s+.+\s+FROM\b|\bpublic\s+(static\s+)?(class|void)\b|<\s*(div|script|html|body|span)\b|\bprint\s*\(|;\s*\n\s*\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
        private static partial Regex CodeInOutput();

        // Model popisuje vlastní instrukce — znak, že prozrazuje systémový prompt
        [GeneratedRegex(@"\b(moje|me|svoje)\s+(systemove\s+)?(instrukce|pokyny)\s+(jsou|zni)\b|\bsystem\s*prompt\b|\bsystemovy\s+prompt\b")]
        private static partial Regex LeakedRules();
    }
}
