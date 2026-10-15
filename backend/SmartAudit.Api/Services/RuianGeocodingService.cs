using System.Globalization;
using System.Text.Json;
using NetTopologySuite.Geometries;

namespace SmartAudit.Api.Services
{
    public record RuianSuggestion(string Text, string Key);

    public record RuianAddressPoint(string Address, Point Location);

    public interface IRuianGeocodingService
    {
        Task<IReadOnlyList<RuianSuggestion>> SuggestAsync(string text, CancellationToken ct);
        Task<RuianAddressPoint?> ResolveAsync(string key, CancellationToken ct);
        Task<RuianAddressPoint?> ReverseAsync(Point location, CancellationToken ct);
    }

    // Vyhledávací služba nad daty RÚIAN (ČÚZK) — oficiální registr adres, stejný zdroj jako karty budov.
    // Tabulka 1 = adresní místa; klíč našeptávače (magicKey) je kód adresního místa.
    public class RuianGeocodingService : IRuianGeocodingService
    {
        private const string BaseUrl = "https://ags.cuzk.gov.cz/arcgis/rest/services/RUIAN/Vyhledavaci_sluzba_nad_daty_RUIAN/MapServer/exts/GeocodeSOE/tables/1";

        // Služba vrátí nejvýš 15 návrhů
        private const int MaxSuggestions = 15;

        // Zpětné hledání: nejbližší adresní místo do 150 m — dál už to nejspíš není adresa té budovy
        private const int ReverseDistanceMeters = 150;

        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

        private readonly HttpClient _httpClient;

        public RuianGeocodingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<RuianSuggestion>> SuggestAsync(string text, CancellationToken ct)
        {
            var url = $"{BaseUrl}/suggest?text={Uri.EscapeDataString(text)}&maxSuggestions={MaxSuggestions}&f=json";

            using var document = await GetJsonAsync(url, ct);

            if (!document.RootElement.TryGetProperty("suggestions", out var suggestions))
                return [];

            return suggestions.EnumerateArray()
                .Select(s => new RuianSuggestion(s.GetProperty("text").GetString() ?? string.Empty, s.GetProperty("magicKey").GetString() ?? string.Empty))
                .Where(s => s.Text.Length > 0 && s.Key.Length > 0)
                .ToList();
        }

        public async Task<RuianAddressPoint?> ResolveAsync(string key, CancellationToken ct)
        {
            var url = $"{BaseUrl}/findAddressCandidates?magicKey={Uri.EscapeDataString(key)}&outSR=4326&f=json";

            using var document = await GetJsonAsync(url, ct);

            if (!document.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                return null;

            var best = candidates.EnumerateArray().OrderByDescending(c => c.GetProperty("score").GetDouble()).First();

            return new RuianAddressPoint(best.GetProperty("address").GetString() ?? string.Empty, ReadPoint(best.GetProperty("location")));
        }

        public async Task<RuianAddressPoint?> ReverseAsync(Point location, CancellationToken ct)
        {
            var point = string.Create(CultureInfo.InvariantCulture, $$$"""{"x":{{{location.X}}},"y":{{{location.Y}}},"spatialReference":{"wkid":4326}}""");
            var url = $"{BaseUrl}/reverseGeocode?location={Uri.EscapeDataString(point)}&distance={ReverseDistanceMeters}&outSR=4326&f=json";

            using var document = await GetJsonAsync(url, ct);

            // Bez adresy v okolí vrací služba chybu „There is no closest feature“
            if (!document.RootElement.TryGetProperty("address", out var address))
                return null;

            return new RuianAddressPoint(address.GetProperty("Address").GetString() ?? string.Empty, ReadPoint(document.RootElement.GetProperty("location")));
        }

        private async Task<JsonDocument> GetJsonAsync(string url, CancellationToken ct)
        {
            using var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        }

        private static Point ReadPoint(JsonElement location)
        {
            return Factory.CreatePoint(new Coordinate(location.GetProperty("x").GetDouble(), location.GetProperty("y").GetDouble()));
        }
    }
}
