using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace SmartAudit.Core.Domain.Helpers
{
    public record RegistrySearchRow(string VersionId, string Title, DateOnly? PublishedOn);

    public record RegistryAttachment(string Id, string Name, string Url);

    public record RegistryContract(
        string ContractId,
        string VersionId,
        string? ContractNumber,
        string Title,
        string PublisherIco,
        string PartnerName,
        string? PartnerIco,
        decimal? AmountWithoutVat,
        decimal? AmountWithVat,
        DateOnly? DateSigned,
        DateOnly PublishedOn,
        IReadOnlyList<RegistryAttachment> Attachments);

    // Registr smluv (smlouvy.gov.cz) nemá veřejné REST API pro vyhledávání — čte se výsledek podrobného vyhledávání
    // (AJAX snippet tabulky) a stránka detailu smlouvy. Obojí je stabilní HTML z Nette, bez JavaScriptu.
    public static class ContractRegistryParser
    {
        public const string BaseUrl = "https://smlouvy.gov.cz";

        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");

        private static readonly Regex Row = new(@"<tr>\s*<td.*?</tr>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex Cell = new(@"<td[^>]*>(.*?)</td>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex DetailLink = new(@"/smlouva/(\d+)", RegexOptions.Compiled);
        // Položka bez hodnoty (např. „Ev. číslo zak. z VVZ“) nesmí spolknout další štítek — skupiny nepřekročí </span>
        private static readonly Regex Item = new(@"<span class=""name[^""]*"">((?:(?!</span>).)*)</span>\s*<span class=""value[^""]*"">((?:(?!</span>).)*)</span>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex Contractor = new(@"<div class=""contractor"">(.*?)</div>\s*</div>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex Attachment = new(@"<a href=""(/smlouva/soubor/(\d+)/[^""]+)"">(.*?)</a>", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex Tag = new(@"<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

        // Řádky tabulky výsledků: publikující subjekt | předmět | poslední verze | publikováno | hodnota | smluvní strany
        public static IReadOnlyList<RegistrySearchRow> ParseSearchResults(string html)
        {
            var rows = new List<RegistrySearchRow>();

            foreach (Match row in Row.Matches(html))
            {
                var link = DetailLink.Match(row.Value);
                var cells = Cell.Matches(row.Value).Select(c => Text(c.Groups[1].Value)).ToList();

                if (!link.Success || cells.Count < 4)
                    continue;

                rows.Add(new RegistrySearchRow(link.Groups[1].Value, cells[1], ParseDate(cells[3])));
            }

            return rows;
        }

        // Detail: levý sloupec (publikující strana + smlouva), prostřední (smluvní strany), pravý (verze + soubory)
        public static RegistryContract? ParseDetail(string html)
        {
            var partiesStart = html.IndexOf("<h2>Smluvní strany</h2>", StringComparison.Ordinal);
            var versionsStart = html.IndexOf("<h2>Verze záznamu</h2>", StringComparison.Ordinal);
            if (partiesStart < 0 || versionsStart < partiesStart)
                return null;

            var main = Items(html[..partiesStart]);
            var contractId = main.GetValueOrDefault("ID smlouvy");
            var versionId = main.GetValueOrDefault("ID verze");
            var title = main.GetValueOrDefault("Předmět smlouvy");
            var publishedOn = ParseDate(main.GetValueOrDefault("Zveřejnění"));

            if (string.IsNullOrEmpty(contractId) || string.IsNullOrEmpty(versionId) || string.IsNullOrEmpty(title) || publishedOn == null)
                return null;

            // Dodavatel = strana označená „Příjemce“, jinak první s jiným IČO — registr mezi stranami občas uvádí i publikující organizaci
            var publisherIco = FirstToken(main.GetValueOrDefault("IČO")) ?? string.Empty;
            var parties = Contractor.Matches(html[partiesStart..versionsStart]).Select(c => Items(c.Groups[1].Value)).ToList();
            var partner = parties.FirstOrDefault(p => p.GetValueOrDefault("Plátce / příjemce") == "Příjemce")
                ?? parties.FirstOrDefault(p => !SameIco(FirstToken(p.GetValueOrDefault("IČO")), publisherIco))
                ?? parties.FirstOrDefault()
                ?? new Dictionary<string, string>();

            var attachments = Attachment.Matches(html[versionsStart..])
                .Select(a => new RegistryAttachment(a.Groups[2].Value, Text(a.Groups[3].Value), BaseUrl + WebUtility.HtmlDecode(a.Groups[1].Value)))
                .ToList();

            return new RegistryContract(
                contractId,
                versionId,
                NullIfEmpty(main.GetValueOrDefault("Číslo smlouvy / č.j.")),
                title,
                publisherIco,
                NullIfEmpty(partner.GetValueOrDefault("Název")) ?? "Neuvedeno",
                FirstToken(partner.GetValueOrDefault("IČO")),
                ParseCzk(main.GetValueOrDefault("Hodnota bez DPH")),
                ParseCzk(main.GetValueOrDefault("Hodnota vč. DPH")),
                ParseDate(main.GetValueOrDefault("Datum uzavření")),
                publishedOn.Value,
                attachments);
        }

        public static string DetailUrl(string versionId) => $"{BaseUrl}/smlouva/{versionId}";

        // Registr píše IČO s úvodními nulami i bez nich
        public static bool SameIco(string? left, string? right)
        {
            return left != null && right != null && left.TrimStart('0') == right.TrimStart('0');
        }

        // „Název subjektu:“ → hodnota; první výskyt vyhrává (publikující strana má IČO před zveřejňujícím)
        private static Dictionary<string, string> Items(string html)
        {
            var items = new Dictionary<string, string>();

            foreach (Match item in Item.Matches(html))
                items.TryAdd(Text(item.Groups[1].Value).TrimEnd(':').Trim(), Text(item.Groups[2].Value));

            return items;
        }

        // Dvakrát dekódovat — publikující občas vloží entitu do předmětu a registr ji zakóduje znovu („&amp;quot;“)
        private static string Text(string html)
        {
            return Whitespace.Replace(WebUtility.HtmlDecode(WebUtility.HtmlDecode(Tag.Replace(html, " "))), " ").Trim();
        }

        private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

        private static string? FirstToken(string? value) => NullIfEmpty(value?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());

        // „29.06.2026 14:47:04“ i „29.06.2026“
        private static DateOnly? ParseDate(string? value)
        {
            var date = value?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            return DateOnly.TryParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;
        }

        // „32 387 340,10 CZK“ — jiná měna než CZK se nepřepočítává
        private static decimal? ParseCzk(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.EndsWith("CZK", StringComparison.Ordinal))
                return null;

            var number = value[..^3].Replace(" ", "").Replace(" ", "");

            return decimal.TryParse(number, NumberStyles.Number, Cs, out var parsed) ? parsed : null;
        }
    }
}
