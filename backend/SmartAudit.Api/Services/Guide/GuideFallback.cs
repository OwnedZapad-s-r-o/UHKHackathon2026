using System.Globalization;
using System.Text.RegularExpressions;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Guide
{
    public interface IGuideFallback
    {
        Task<GuideDraft> AskAsync(string question, GuideAskContext context, CancellationToken ct);
    }

    // Průvodce bez jazykového modelu — když AI není nastavená, neodpoví nebo její odpověď neprojde kontrolou.
    // Záměr pozná z klíčových slov (okres, obec, druh, „nejhorší vzduch“, „ztráta“, „tady“), data hledá stejnými funkcemi
    // jako model a vysvětlení vezme ze znalostní báze. Odpovědi jsou strožší, ale mapa i karty fungují stejně.
    public partial class GuideFallback : IGuideFallback
    {
        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");

        private const int MaxKnowledgeLength = 420;

        // Slova, která říkají, co se hledá, ale nejsou součástí názvu objektu
        private static readonly HashSet<string> Filler =
        [
            "najdi", "najit", "ukaz", "ukazat", "ukazte", "chci", "vedet", "zobraz", "budova", "budovy", "budov", "budove", "objekt", "objekty", "objektu",
            "objektech", "zarizeni", "kraj", "kraje", "kraji", "mapa", "mape", "mapu", "nejaky", "nejaka", "nejake", "jsou", "ted", "aktualne", "prave",
            "dnes", "tyden", "tydnu", "okres", "okrese", "okresu", "okresy", "obec", "obci", "mesto", "meste", "kolik", "vsechny", "seznam", "dej", "mi",
            "ktery", "ktera", "ktere", "kde", "nejvic", "nejhorsi", "nejlepsi", "jak", "dycha", "dychaji", "maji", "ma", "je", "se", "v", "ve", "na", "s",
            "proc", "jaky", "jaka", "jake", "co", "tim", "udelat", "delat", "tu", "tady", "mam", "muzu", "prosim", "pro", "o", "a", "i",
        ];

        private readonly IGuideDataService _data;
        private readonly IGuideKnowledgeBase _knowledge;

        public GuideFallback(IGuideDataService data, IGuideKnowledgeBase knowledge)
        {
            _data = data;
            _knowledge = knowledge;
        }

        // Dotaz zjevně o budovách kraje — když ho model přesto odmítne (falešný poplach), odpoví záložní režim z dat.
        // Záložní režim nic negeneruje, takže tím nevzniká cesta k obsahu mimo účel.
        public static bool LooksLikeDomainQuestion(string question) => DomainWords().IsMatch(GuideText.Normalize(question));

        public static IReadOnlyList<string> DefaultSuggestions(bool canSeeMoney) => canSeeMoney
            ? ["Kde kraj ztrácí nejvíc peněz?", "Který okres má nejvíc zjištění?", "Které budovy se přetápí?"]
            : ["Kde se teď dýchá nejhůř?", "Co znamená 1 500 ppm CO₂?", "Školy v okrese Náchod"];

        public async Task<GuideDraft> AskAsync(string question, GuideAskContext context, CancellationToken ct)
        {
            var text = GuideText.Normalize(question);
            var facilities = await _data.FacilitiesAsync(ct);
            var knowledge = await _knowledge.SearchAsync(question, _data.CanSeeMoney, 2, ct);

            // 1. „tady“, „tahle budova“ — objekt vybraný v mapě nebo karta, na které uživatel je
            var contextFacility = context.FacilityId is { } id ? facilities.FirstOrDefault(f => f.Id == id) : null;
            if (contextFacility != null && Here().IsMatch(text) && !KnowledgeLead().IsMatch(text))
                return await FacilityAnswerAsync(contextFacility, ct);

            var district = DistrictFromText(text, facilities);
            var municipality = MunicipalityFromText(text, facilities);

            // 2. Srovnání okresů
            if (DistrictWord().IsMatch(text) && district == null && (DataWords().IsMatch(text) || Issues().IsMatch(text)))
                return await DistrictsAnswerAsync(ct);

            if (WholeRegion().IsMatch(text) && district == null && municipality == null)
                return await RegionAnswerAsync(ct);

            var search = new GuideSearch(
                Text: municipality,
                District: municipality == null ? district : null,
                Category: CategoryFromText(text),
                Status: Overheating().IsMatch(text) ? AuditStatus.Critical : null,
                OnlyIssues: Issues().IsMatch(text),
                OnlyLive: Live().IsMatch(text),
                Sort: SortFromText(text),
                Limit: 5);

            // 3. Konkrétní objekt podle názvu („Lepařovo gymnázium“, „proč má Domov Dědina problém“)
            var nameWords = NameWords(text, facilities);
            if (nameWords.Length > 0)
            {
                var byName = await _data.SearchAsync(search with { Text = string.Join(' ', nameWords.Append(municipality ?? string.Empty)).Trim() }, ct);
                if (byName.Total > 0)
                    return await SearchAnswerAsync(byName, municipality, district, ct);
            }

            // 4. Vysvětlení pojmu nebo návod („co je…“, „jak…“) — pokud se zároveň nehledají objekty
            if (knowledge.Count > 0 && KnowledgeLead().IsMatch(text) && !DataWords().IsMatch(text) && district == null && municipality == null)
                return KnowledgeAnswer(knowledge);

            // 5. Hledání objektů podle filtrů, které v dotazu jsou
            var hasFilter = search.District != null || search.Text != null || search.Category != null || search.Status != null || search.OnlyIssues || search.OnlyLive || search.Sort != GuideSort.Severity;
            if (hasFilter)
                return await SearchAnswerAsync(await _data.SearchAsync(search, ct), municipality, search.District, ct);

            // 6. Aspoň znalosti, jinak nabídka, na co se ptát
            if (knowledge.Count > 0)
                return KnowledgeAnswer(knowledge);

            return new GuideDraft(
                "Jsem průvodce budovami Královéhradeckého kraje. Umím najít školu nebo domov a ukázat ho v mapě, porovnat okresy, říct, kde se dýchá nejhůř, a vysvětlit, co znamenají zjištění auditu.",
                GuideFocus.None, [], [], null, null, [], false, DefaultSuggestions(_data.CanSeeMoney));
        }

        private async Task<GuideDraft> FacilityAnswerAsync(GuideFacility facility, CancellationToken ct)
        {
            var detail = await _data.DetailAsync(facility.Id, ct);
            var finding = detail?.Assessment?.Findings.FirstOrDefault();

            var answer = $"**{facility.Name}** ({facility.MunicipalityName}): {facility.Headline}. "
                + (finding != null ? $"{finding.Detail} " : "Útlum mimo provoz funguje a CO₂ zůstává pod limitem. ")
                + AirSentence(facility)
                + LossSentence(facility);

            return new GuideDraft(answer.Trim(), GuideFocus.Facility, [facility], [], facility.DistrictCode, "audit", [], true, FacilitySuggestions());
        }

        private async Task<GuideDraft> DistrictsAnswerAsync(CancellationToken ct)
        {
            var districts = await _data.DistrictsAsync(ct);
            var top = districts.FirstOrDefault();
            if (top == null)
                return new GuideDraft("Data o okresech zatím nejsou načtená.", GuideFocus.None, [], [], null, null, [], false, DefaultSuggestions(_data.CanSeeMoney));

            var answer = $"Nejvyšší podíl budov se zjištěním má okres **{top.Name}**: {top.WithIssues} z {top.Facilities}. "
                + (top.AnnualLossCzk is { } loss && loss > 0 ? $"Odhad ztráty v okrese je **{loss.ToString("N0", Cs)} Kč** za rok. " : "")
                + "Okres jsem vybral v mapě, srovnání všech okresů je v kartách.";

            return new GuideDraft(answer, GuideFocus.District, [], districts, top.Code, null, [], true,
                [$"Nejhorší budovy v okrese {top.Name}", "Kde se dýchá nejhůř?", "Co znamená přetápění?"]);
        }

        private async Task<GuideDraft> RegionAnswerAsync(CancellationToken ct)
        {
            var districts = await _data.DistrictsAsync(ct);
            var total = districts.Sum(d => d.Facilities);
            var issues = districts.Sum(d => d.WithIssues);
            var loss = districts.Sum(d => d.AnnualLossCzk ?? 0);

            var answer = $"V kraji sleduji **{total} {Plural(total, "objekt", "objekty", "objektů")}**, z toho {issues} se zjištěním. "
                + (_data.CanSeeMoney && loss > 0 ? $"Odhad roční ztráty za celý kraj je **{loss.ToString("N0", Cs)} Kč**." : "Mapa ukazuje celý kraj po okresech.");

            return new GuideDraft(answer, GuideFocus.Region, [], districts, null, null, [], true, DefaultSuggestions(_data.CanSeeMoney));
        }

        private async Task<GuideDraft> SearchAnswerAsync(GuideSearchResult result, string? municipality, string? districtCode, CancellationToken ct)
        {
            var applied = result.AppliedSearch;
            var scope = municipality != null ? $" v obci {municipality}"
                : districtCode != null && result.Facilities.Count > 0 ? $" v okrese {result.Facilities[0].DistrictName}" : "";
            var category = applied.Category switch
            {
                FacilityCategory.School => " mezi školami",
                FacilityCategory.Social => " mezi sociálními službami",
                FacilityCategory.Health => " ve zdravotnictví",
                FacilityCategory.Culture => " mezi kulturními organizacemi",
                _ => "",
            };

            if (result.Total == 0)
                return new GuideDraft($"Takový objekt jsem{scope}{category} nenašel. Zkuste jiný název, obec nebo okres.", GuideFocus.None, [], [], null, null, [], true, DefaultSuggestions(_data.CanSeeMoney));

            // Jediný objekt — rovnou s vysvětlením zjištění, jako u „tady“
            if (result.Total == 1)
                return await FacilityAnswerAsync(result.Facilities[0], ct);

            var first = result.Facilities[0];
            var filtered = scope.Length > 0 || category.Length > 0 || applied.Status != null || applied.OnlyIssues || applied.OnlyLive || applied.Text != null;
            var found = filtered
                ? $"Našel jsem {result.Total} {Plural(result.Total, "objekt", "objekty", "objektů")}{scope}{category}{SortPhrase(applied.Sort)}. "
                : $"Prošel jsem všech {result.Total} objektů kraje{SortPhrase(applied.Sort)}. ";
            var answer = found
                + $"Na prvním místě je **{first.Name}** — {Metric(first, applied.Sort)}. "
                + (result.Total > result.Facilities.Count ? $"V mapě jsem očísloval prvních {result.Facilities.Count}." : "Všechny jsem očísloval v mapě.");

            return new GuideDraft(answer, GuideFocus.Facilities, result.Facilities, [], districtCode, null, [], true, Chart: GuideChartRequest.Ranking, ChartMetric: GuideAgent.ChartMetricOf(applied.Sort), Suggestions:
                [first.Status == AuditStatus.Nominal ? $"Jak se dýchá: {Shorten(first.Name)}" : $"Proč má {Shorten(first.Name)} problém?","Který okres má nejvíc zjištění?", "Co znamená 1 500 ppm CO₂?"]);
        }

        private GuideDraft KnowledgeAnswer(IReadOnlyList<KnowledgeHit> hits)
        {
            var top = hits[0];
            var text = top.Chunk.Text;

            // Celé věty do rozumné délky — karta průvodce není dokument
            if (text.Length > MaxKnowledgeLength)
            {
                var cut = text.LastIndexOf(". ", MaxKnowledgeLength, StringComparison.Ordinal);
                text = cut > 80 ? text[..(cut + 1)] : text[..MaxKnowledgeLength] + "…";
            }

            return new GuideDraft(text, GuideFocus.None, [], [], null, null, hits.Take(2).ToList(), false, DefaultSuggestions(_data.CanSeeMoney));
        }

        private string AirSentence(GuideFacility f)
        {
            if (f.Co2 is not { } co2)
                return "";

            var hours = f.Co2HoursOverLimit is { } h && h > 0 ? $", za týden {Math.Round(h)} h nad limitem" : "";
            var source = f.HasLiveSensors ? "naměřeno čidly" : "modelová data";
            return $"CO₂ teď {Math.Round(co2).ToString("N0", Cs)} ppm{hours} ({source}). ";
        }

        private string LossSentence(GuideFacility f)
        {
            return _data.CanSeeMoney && f.AnnualLossCzk is { } loss && loss > 0 ? $"Odhad ztráty **{loss.ToString("N0", Cs)} Kč** za rok." : "";
        }

        private IReadOnlyList<string> FacilitySuggestions() => _data.CanSeeMoney
            ? ["Co s tím udělat?", "Jak se počítá ztráta?", "Podobné budovy v okrese"]
            : ["Kdy tu větrat?", "Co znamená 1 500 ppm CO₂?", "Školy se živými čidly"];

        private string Metric(GuideFacility f, GuideSort sort) => sort switch
        {
            GuideSort.Loss when f.AnnualLossCzk is { } loss => $"odhad ztráty {loss.ToString("N0", Cs)} Kč za rok",
            GuideSort.Co2Hours when f.Co2HoursOverLimit is { } hours && hours >= 1 => $"{Math.Round(hours)} h nad limitem CO₂ za týden",
            GuideSort.Co2Hours => "CO₂ za týden ani jednou nad limitem",
            GuideSort.Co2 when f.Co2 is { } co2 => $"CO₂ teď {Math.Round(co2).ToString("N0", Cs)} ppm",
            GuideSort.Temperature when f.Temperature is { } temperature => $"průměrně {temperature.ToString("0.#", Cs)} °C",
            _ => f.Headline.ToLower(Cs),
        };

        private static string SortPhrase(GuideSort sort) => sort switch
        {
            GuideSort.Loss => ", seřazené podle odhadované ztráty",
            GuideSort.Co2Hours => ", seřazené podle hodin nad limitem CO₂",
            GuideSort.BestAir => ", od nejčerstvějšího vzduchu",
            GuideSort.Co2 => ", seřazené podle CO₂",
            GuideSort.Temperature => ", seřazené podle teploty",
            _ => "",
        };

        private GuideSort SortFromText(string text)
        {
            if (Loss().IsMatch(text))
                return _data.CanSeeMoney ? GuideSort.Loss : GuideSort.Severity;
            if (Air().IsMatch(text))
                return Best().IsMatch(text) ? GuideSort.BestAir : GuideSort.Co2Hours;
            if (Temperature().IsMatch(text))
                return GuideSort.Temperature;
            return GuideSort.Severity;
        }

        private static FacilityCategory? CategoryFromText(string text)
        {
            if (Schools().IsMatch(text)) return FacilityCategory.School;
            if (Social().IsMatch(text)) return FacilityCategory.Social;
            if (Health().IsMatch(text)) return FacilityCategory.Health;
            if (Culture().IsMatch(text)) return FacilityCategory.Culture;
            return null;
        }

        // Okres podle začátku názvu („náchodě“ → Náchod, „rychnovsku“ → Rychnov nad Kněžnou)
        private static string? DistrictFromText(string text, IReadOnlyList<GuideFacility> facilities)
        {
            return facilities
                .Select(f => (f.DistrictCode, Stem: Prefix(GuideText.Normalize(f.DistrictName).Split(' ')[0], 4)))
                .Distinct()
                .FirstOrDefault(d => Regex.IsMatch(text, $@"\b{Regex.Escape(d.Stem)}"))
                .DistrictCode;
        }

        // Nejdelší obec, jejíž název (bez koncovky) v dotazu je — „v Jičíně“ najde Jičín
        private static string? MunicipalityFromText(string text, IReadOnlyList<GuideFacility> facilities)
        {
            return facilities
                .Select(f => f.MunicipalityName)
                .Distinct()
                .Select(name => (Name: name, Normalized: GuideText.Normalize(name)))
                .Where(m => m.Normalized.Length >= 3)
                .Select(m => (m.Name, Stem: m.Normalized.Length > 5 ? m.Normalized[..^2] : m.Normalized))
                .Where(m => Regex.IsMatch(text, $@"\b{Regex.Escape(m.Stem)}"))
                .OrderByDescending(m => m.Stem.Length)
                .Select(m => m.Name)
                .FirstOrDefault();
        }

        // Slova, která můžou být z názvu objektu („gymnázium“, „Komenského“) — bez výplňových, filtrů a obcí
        private static string[] NameWords(string text, IReadOnlyList<GuideFacility> facilities)
        {
            var places = facilities.SelectMany(f => GuideText.Normalize($"{f.MunicipalityName} {f.DistrictName}").Split(' ')).ToHashSet();

            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim('.', ',', '?', '!', '„', '“', '"', '…'))
                .Where(w => w.Length >= 3 && !Filler.Contains(w) && !places.Any(p => p.Length >= 4 && w.StartsWith(p[..^1], StringComparison.Ordinal)))
                .Where(w => !Issues().IsMatch(w) && !Air().IsMatch(w) && !Loss().IsMatch(w) && !Temperature().IsMatch(w) && !Live().IsMatch(w) && !Schools().IsMatch(w)
                    && !Social().IsMatch(w) && !Health().IsMatch(w) && !Culture().IsMatch(w) && !DataWords().IsMatch(w) && !Here().IsMatch(w))
                .ToArray();
        }

        private static string Prefix(string text, int length) => text.Length > length ? text[..length] : text;

        private static string Shorten(string name) => name.Length > 28 ? name[..26].TrimEnd() + "…" : name;

        private static string Plural(int count, string one, string few, string many) => count == 1 ? one : count is >= 2 and <= 4 ? few : many;

        [GeneratedRegex(@"\b(budov\w*|skol\w*|gymnazi\w*|akademi\w*|ucilist\w*|domov\w*|nemocnic\w*|muze\w*|galeri\w*|knihovn\w*|objekt\w*|okres\w*|co2|vzduch\w*|tepl\w*|vytap\w*|utlum\w*|preta\w*|ztrat\w*|cidl\w*|iqrf|gateway\w*|audit\w*|energi\w*|spotreb\w*|vetr\w*|zjisten\w*|opatren\w*|smlouv\w*|report\w*)\b")]
        private static partial Regex DomainWords();

        [GeneratedRegex(@"\b(tady|zde|tato|tahle|tenhle|tento|teto|tomto|tomhle|tuhle|vybran\w*|tu\s+budov\w*)\b")]
        private static partial Regex Here();

        [GeneratedRegex(@"^(co\s+(je|jsou|znamena|znaci|delat|udelat|s\s+tim)|jak\w*\s+(se|funguje|pocita|spravne|vetrat|pripojit|sparovat|najit|vytisknout|cist|poznam)|proc|kdo|odkud|vysvetli|k\s+cemu|muzu|lze|da\s+se|kolik\s+stoji)\b")]
        private static partial Regex KnowledgeLead();

        [GeneratedRegex(@"\b(kde|ktere|ktery|ktera|nejvic\w*|nejhor\w*|nejhur\w*|nejlep\w*|nejlip\w*|nejvys\w*|ukaz\w*|najdi|najit|seznam|vsechn\w*|srovn\w*|porovn\w*|zebric\w*|nejdraz\w*)\b")]
        private static partial Regex DataWords();

        [GeneratedRegex(@"\b(nejlep\w*|nejlip|nejcerstv\w*|nejmen\w*)\b")]
        private static partial Regex Best();

        [GeneratedRegex(@"\bokres\w*\b")]
        private static partial Regex DistrictWord();

        [GeneratedRegex(@"\b(cel\w*\s+kraj\w*|kraj\s+celkem|za\s+kraj)\b")]
        private static partial Regex WholeRegion();

        [GeneratedRegex(@"\b(problem\w*|zjisten\w*|anomali\w*|spatn\w*|preta\w*|utlum\w*|nefung\w*|kritick\w*|varovan\w*)\b")]
        private static partial Regex Issues();

        [GeneratedRegex(@"\b(preta\w*|kritick\w*)\b")]
        private static partial Regex Overheating();

        [GeneratedRegex(@"\b(co2|vzduch\w*|dych\w*|vetr\w*|vydychan\w*|oxid\w*)\b")]
        private static partial Regex Air();

        [GeneratedRegex(@"\b(ztrat\w*|penez|penize|kc|korun\w*|usetr\w*|uspor\w*|nejdraz\w*)\b")]
        private static partial Regex Loss();

        [GeneratedRegex(@"\b(teplot\w*|tepl[ao]\w*|zima|horko)\b")]
        private static partial Regex Temperature();

        [GeneratedRegex(@"\b(ziv\w*|cidl\w*|merene|mereni|iqrf|skutecn\w*)\b")]
        private static partial Regex Live();

        [GeneratedRegex(@"\b(skol\w*|stredn\w*|ucilist\w*|domov\w*\s+mladez\w*|internat\w*)\b")]
        private static partial Regex Schools();

        [GeneratedRegex(@"\b(senior\w*|socialn\w*|domov\w*(?!\s+mladez))\b")]
        private static partial Regex Social();

        [GeneratedRegex(@"\b(nemocnic\w*|lecebn\w*|zdravot\w*|pece)\b")]
        private static partial Regex Health();

        [GeneratedRegex(@"\b(muze\w*|galeri\w*|knihovn\w*|divadl\w*|kultur\w*|hvezdarn\w*)\b")]
        private static partial Regex Culture();
    }
}
