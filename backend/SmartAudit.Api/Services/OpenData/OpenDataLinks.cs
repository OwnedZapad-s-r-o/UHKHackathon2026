using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.OpenData
{
    // Odkaz na zdrojovou sadu podle Čl. 7.3 pravidel: záznam v katalogu (Data KHK, ČÚZK, Registr smluv, Open-Meteo)
    // a živý endpoint, ze kterého aplikace data čte (ArcGIS GeoJSON, WFS, REST)
    // DcatRecordUrl = záznam sady v katalogu Data KHK podle DCAT-AP-CZ (open.datakhk.cz/{id}.jsonld), jen u sad Data KHK
    public record DatasetLinkDto(string Key, string Title, string Provider, string Category, string CatalogUrl, string LiveApiUrl, string? DcatRecordUrl);

    public class OpenDataLinks
    {
        public const string KhkCatalogUrl = "https://open.datakhk.cz/katalog.jsonld";
        public const string KhkPortalUrl = "https://www.datakhk.cz/search";

        // Ukázkový bod pro živé dotazy Open-Meteo v odkazech (Hradec Králové)
        private const string SamplePoint = "latitude=50.2092&longitude=15.8328";

        private readonly Dictionary<string, DatasetSettings> _datasets;

        public OpenDataLinks(OpenDataSettings settings)
        {
            _datasets = settings.Datasets.GroupBy(d => d.Key).ToDictionary(g => g.Key, g => g.First());
        }

        public IReadOnlyCollection<DatasetSettings> Datasets => _datasets.Values;

        // Vyhledávání smluv organizace v Registru smluv — formulář registru chce v adrese všechna pole, i prázdná
        public static string RegistrySearchUrl(string ico)
        {
            string[] fields =
            [
                "contract_id", "version_id", "publication_date[from]", "publication_date[to]", "subject_box", "subject_name", "subject_idnum",
                "subject_address", "party_box", "party_name", "party_idnum", "party_address", "contr_num", "contract_reference_number",
                "sign_date[from]", "sign_date[to]", "contract_descr", "sign_person_name", "value_no_vat[from]", "value_no_vat[to]",
                "value_vat[from]", "value_vat[to]", "value_foreign[from]", "value_foreign[to]", "foreign_currency", "file_text",
            ];

            var query = string.Join("&", fields.Select(f => $"{Uri.EscapeDataString(f)}={(f == "subject_idnum" ? Uri.EscapeDataString(ico) : string.Empty)}"));

            return $"https://smlouvy.gov.cz/vyhledavani?{query}&search_type=0";
        }

        public DatasetLinkDto? Get(string key) => _datasets.TryGetValue(key, out var dataset) ? ToLink(dataset) : null;

        public IReadOnlyList<DatasetLinkDto> For(IEnumerable<string> keys)
        {
            return keys.Distinct().Select(Get).OfType<DatasetLinkDto>().ToList();
        }

        public static DatasetLinkDto ToLink(DatasetSettings dataset)
        {
            return new DatasetLinkDto(dataset.Key, dataset.Title, dataset.Provider, CategoryOf(dataset), CatalogUrl(dataset), LiveApiUrl(dataset), DcatRecordUrl(dataset));
        }

        public static string? DcatRecordUrl(DatasetSettings dataset)
        {
            return dataset.Provider == "Data KHK" && dataset.DatasetId != null ? $"https://open.datakhk.cz/{dataset.DatasetId}.jsonld" : null;
        }

        public static string CatalogUrl(DatasetSettings dataset)
        {
            if (dataset.PortalUrl != null)
                return dataset.PortalUrl;

            return dataset.Provider == "Data KHK" ? KhkPortalUrl : dataset.ServiceUrl;
        }

        // Živý endpoint — přesně ten dotaz, kterým aplikace sadu stahuje nebo kontroluje
        public static string LiveApiUrl(DatasetSettings dataset)
        {
            return dataset.Key switch
            {
                "cuzk-budovy" => $"{dataset.ServiceUrl}?service=WFS&version=2.0.0&request=GetCapabilities",
                "open-meteo-pocasi" => $"{dataset.ServiceUrl}?{SamplePoint}&hourly=temperature_2m&past_days=1",
                "open-meteo-ovzdusi" => $"{dataset.ServiceUrl}?{SamplePoint}&hourly=pm10,pm2_5&past_days=1",
                "open-meteo-vyska" => $"{dataset.ServiceUrl}?{SamplePoint}",
                "registr-smluv" => "https://smlouvy.gov.cz/vyhledavani",
                _ when dataset.Provider == "Data KHK" => $"{dataset.ServiceUrl}/query?where=1%3D1&outFields=*&outSR=4326&f=geojson",
                _ => dataset.ServiceUrl,
            };
        }

        // Tematická kategorie sady (jako na portálu Data KHK) — pro filtry a štítky na stránce Zdroje dat
        public static string CategoryOf(DatasetSettings dataset)
        {
            if (!string.IsNullOrWhiteSpace(dataset.Category))
                return dataset.Category;

            return dataset.Key switch
            {
                "skoly" or "pravni-subjekty-skol" or "zaci-ss-kraj" or "zaci-ss-historie" or "studenti-vos-kraj" or "ubytovani-dm"
                    or "prijimacky-ss" or "prijimacky-vos" or "msmt-kalendar" => "Školství a výzkum",
                "socialni-sluzby" => "Lidé a sociální služby",
                "nemocnice" or "nasledna-pece" or "lecebne-ustavy" or "zzs-zakladny" => "Zdravotnictví",
                "organizace-khk" => "Organizace kraje",
                "dotace-orp" or "dotace-orp-individualni" or "dotace-orp-programove" => "Investice kraje a dotace",
                "hsoo" or "adresar-obci" or "hranice-okresu" or "hranice-orp" or "hranice-kraje" => "Obce a území",
                "pamatkove-zony" or "pamatkove-rezervace" or "nkp-plosne" or "nkp-bodove" => "Kultura a památky",
                "silnice-kraj" => "Doprava",
                "hzs-stanice" => "Bezpečnost a IZS",
                "cuzk-budovy" => "Katastr ČÚZK",
                "registr-smluv" => "Registr smluv",
                "open-meteo-ovzdusi" => "Životní prostředí",
                _ when dataset.Provider == "Open-Meteo" => "Open-Meteo",
                _ => dataset.Group,
            };
        }
    }
}
