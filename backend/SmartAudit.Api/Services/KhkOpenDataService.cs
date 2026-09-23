using System.Text.Json;
using System.Text.Json.Nodes;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace SmartAudit.Api.Services
{
    public record OpenDataRecord(string ExternalId, string Properties, Geometry? Geometry);

    public record OpenDataDownload(string FeatureCollectionJson, IReadOnlyList<OpenDataRecord> Records);

    public interface IKhkOpenDataService
    {
        Task<OpenDataDownload> DownloadAsync(string serviceUrl, CancellationToken ct);
        IReadOnlyList<OpenDataRecord> Parse(string featureCollectionJson);
    }
    public class KhkOpenDataService : IKhkOpenDataService
    {
        private const int PageSize = 1000;

        // ~5 m zjednodušení polygonů — na bod v polygonu i mapu stačí a snapshoty zůstanou malé
        private const string GeometryOffset = "0.00005";

        private static readonly JsonSerializerOptions GeoJsonOptions = CreateGeoJsonOptions();

        private readonly HttpClient _httpClient;

        public KhkOpenDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OpenDataDownload> DownloadAsync(string serviceUrl, CancellationToken ct)
        {
            var features = new JsonArray();
            var offset = 0;

            while (true)
            {
                var url = $"{serviceUrl}/query?where=1%3D1&outFields=*&outSR=4326&f=geojson&geometryPrecision=6&maxAllowableOffset={GeometryOffset}&resultOffset={offset}&resultRecordCount={PageSize}";

                using var response = await _httpClient.GetAsync(url, ct);
                response.EnsureSuccessStatusCode();

                var page = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct))
                    ?? throw new InvalidOperationException("Prázdná odpověď ArcGIS");

                if (page["error"] != null)
                    throw new InvalidOperationException($"ArcGIS vrátil chybu: {page["error"]?.ToJsonString()}");

                var pageFeatures = page["features"]?.AsArray() ?? new JsonArray();
                var count = pageFeatures.Count;

                foreach (var feature in pageFeatures.ToList())
                {
                    pageFeatures.Remove(feature);
                    features.Add(feature);
                }

                var exceeded = page["properties"]?["exceededTransferLimit"]?.GetValue<bool>() ?? false;

                if (count == 0 || (!exceeded && count < PageSize))
                    break;

                offset += count;
            }

            var collection = new JsonObject
            {
                ["type"] = "FeatureCollection",
                ["features"] = features,
            };

            var json = collection.ToJsonString();

            return new OpenDataDownload(json, Parse(json));
        }

        public IReadOnlyList<OpenDataRecord> Parse(string featureCollectionJson)
        {
            using var document = JsonDocument.Parse(featureCollectionJson);

            var records = new List<OpenDataRecord>();

            foreach (var feature in document.RootElement.GetProperty("features").EnumerateArray())
            {
                var properties = feature.TryGetProperty("properties", out var props) && props.ValueKind == JsonValueKind.Object
                    ? props
                    : default;

                var externalId = ExternalId(feature, properties);
                if (externalId == null)
                    continue;

                Geometry? geometry = null;

                if (feature.TryGetProperty("geometry", out var geometryElement) && geometryElement.ValueKind == JsonValueKind.Object)
                {
                    geometry = geometryElement.Deserialize<Geometry>(GeoJsonOptions);

                    if (geometry != null)
                        geometry.SRID = 4326;
                }

                var propertiesJson = properties.ValueKind == JsonValueKind.Object ? properties.GetRawText() : "{}";

                records.Add(new OpenDataRecord(externalId, propertiesJson, geometry));
            }

            return records;
        }

        private static string? ExternalId(JsonElement feature, JsonElement properties)
        {
            if (feature.TryGetProperty("id", out var id) && id.ValueKind is JsonValueKind.Number or JsonValueKind.String)
                return id.ToString();

            if (properties.ValueKind == JsonValueKind.Object && properties.TryGetProperty("OBJECTID", out var objectId))
                return objectId.ToString();

            return null;
        }

        private static JsonSerializerOptions CreateGeoJsonOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new GeoJsonConverterFactory(new GeometryFactory(new PrecisionModel(), 4326)));

            return options;
        }
    }
}
