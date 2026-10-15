using System.Text.Encodings.Web;
using System.Text.Json;

namespace SmartAudit.Api.Services.OpenData
{
    // Metadata otevřených dat podle DCAT-AP-CZ (OFN Rozhraní katalogů otevřených dat, verze 2024-05-28) —
    // lokální katalog, který Národní katalog otevřených dat (NKOD) umí sám sklízet.
    // Klíče jsou česká jména z kontextu OFN, proto slovníky místo tříd.
    public static class DcatCatalog
    {
        public const string ContentType = "application/ld+json";

        private const string Context = "https://ofn.gov.cz/dcat-ap-cz-rozhraní-katalogů-otevřených-dat/2024-05-28/kontexty/rozhraní-katalogů-otevřených-dat.jsonld";

        // Území kraje v RÚIAN (VÚSC 86 = Královéhradecký kraj)
        private const string RegionRuian = "https://linked.cuzk.cz/resource/ruian/vusc/86";

        private const string FileType = "http://publications.europa.eu/resource/authority/file-type/";
        private const string MediaType = "http://www.iana.org/assignments/media-types/";
        private const string Theme = "http://publications.europa.eu/resource/authority/data-theme/";

        public const string BuildingsDataset = "budovy-a-energeticky-audit";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            // Čeština a „/“ v IRI čitelně, ne jako \u escapy
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public static string CatalogIri(string baseUrl) => $"{baseUrl}/api/open/v1/catalog";

        public static string DatasetIri(string baseUrl, string dataset) => $"{CatalogIri(baseUrl)}/datasets/{dataset}";

        public static string Catalog(string baseUrl, string providerIri)
        {
            return Serialize(new Dictionary<string, object>
            {
                ["@context"] = Context,
                ["iri"] = CatalogIri(baseUrl),
                ["typ"] = "Katalog",
                ["název"] = new { cs = "Otevřená data SmartAudit KHK", en = "SmartAudit KHK open data" },
                ["popis"] = new
                {
                    cs = "Budovy, které zřizuje Královéhradecký kraj, jejich údaje z katastru a stav energetického auditu z měření vnitřního prostředí.",
                    en = "Buildings operated by the Hradec Králové Region, cadastral data and the energy audit status from indoor measurements.",
                },
                ["poskytovatel"] = providerIri,
                ["domovská_stránka"] = $"{baseUrl}/otevrena-data",
                ["datová_sada"] = new[] { DatasetIri(baseUrl, BuildingsDataset) },
            });
        }

        public static string BuildingsDatasetRecord(string baseUrl, string providerIri)
        {
            var iri = DatasetIri(baseUrl, BuildingsDataset);
            var api = $"{baseUrl}/api/open/v1";

            return Serialize(new Dictionary<string, object>
            {
                ["@context"] = Context,
                ["iri"] = iri,
                ["typ"] = "Datová sada",
                ["název"] = new { cs = "Budovy Královéhradeckého kraje a jejich energetický audit" },
                ["popis"] = new
                {
                    cs = "Školy, domovy sociálních služeb, nemocnice a kulturní zařízení zřizované krajem: poloha, adresa, druh, stavební objekt z ČÚZK, "
                        + "stav energetického auditu (přetápění mimo provoz, noční útlum, CO₂) a poslední naměřené hodnoty. Bez finančních údajů. "
                        + "Hodnoty z budov bez skutečných čidel jsou modelové a tak označené (sloupec zive_cidlo).",
                },
                ["poskytovatel"] = providerIri,
                ["téma"] = new[] { $"{Theme}ENER", $"{Theme}EDUC", $"{Theme}REGI" },
                ["periodicita_aktualizace"] = "http://publications.europa.eu/resource/authority/frequency/DAILY",
                ["klíčové_slovo"] = new { cs = new[] { "budovy", "energetický audit", "vytápění", "kvalita vnitřního vzduchu", "CO2", "školy", "Královéhradecký kraj" } },
                ["prvek_rúian"] = new[] { RegionRuian },
                ["dokumentace"] = $"{api}/docs/",
                ["distribuce"] = new[]
                {
                    Distribution($"{iri}/distribuce/csv", $"{api}/facilities.csv", "CSV", "text/csv"),
                    Distribution($"{iri}/distribuce/geojson", $"{api}/facilities.geojson", "GEOJSON", "application/geo+json"),
                    Distribution($"{iri}/distribuce/json", $"{api}/facilities", "JSON", "application/json"),
                },
            });
        }

        // Podmínky užití CC BY 4.0 — kraj jako autor, žádné osobní údaje, databáze bez zvláštního práva pořizovatele
        private static Dictionary<string, object> Distribution(string iri, string url, string format, string mediaType)
        {
            return new Dictionary<string, object>
            {
                ["iri"] = iri,
                ["typ"] = "Distribuce",
                ["podmínky_užití"] = new Dictionary<string, object>
                {
                    ["typ"] = "Specifikace podmínek užití",
                    ["autorské_dílo"] = "https://creativecommons.org/licenses/by/4.0/",
                    ["autor"] = new { cs = "Královéhradecký kraj" },
                    ["databáze_jako_autorské_dílo"] = "https://creativecommons.org/licenses/by/4.0/",
                    ["autor_databáze"] = new { cs = "Královéhradecký kraj" },
                    ["databáze_chráněná_zvláštními_právy"] = "https://data.gov.cz/podmínky-užití/není-chráněna-zvláštním-právem-pořizovatele-databáze/",
                    ["osobní_údaje"] = "https://data.gov.cz/podmínky-užití/neobsahuje-osobní-údaje/",
                },
                ["přístupové_url"] = url,
                ["soubor_ke_stažení"] = url,
                ["formát"] = $"{FileType}{format}",
                ["typ_média"] = $"{MediaType}{mediaType}",
            };
        }

        private static string Serialize(object value) => JsonSerializer.Serialize(value, JsonOptions);
    }
}
