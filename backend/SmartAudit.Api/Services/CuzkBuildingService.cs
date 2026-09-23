using System.Globalization;
using System.Xml.Linq;
using NetTopologySuite.Geometries;

namespace SmartAudit.Api.Services
{
    public record CuzkBuilding(Polygon Footprint, double? BuiltUpArea, int? ConstructionYear, string? CurrentUse, string? Condition, string? ParcelRef, string? RuianCode);

    public interface ICuzkBuildingService
    {
        Task<CuzkBuilding?> FindBuildingAsync(Point location, CancellationToken ct);
    }
    public class CuzkBuildingService : ICuzkBuildingService
    {
        private const string BaseUrl = "https://services.cuzk.gov.cz/wfs/inspire-bu-wfs.asp";
        private const double MaxDistanceMeters = 30;

        private static readonly XNamespace Xlink = "http://www.w3.org/1999/xlink";
        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);
        private static readonly string[] PublicUses = ["publicServices", "education", "healthCare", "collectiveResidence", "commerceAndServices", "office"];

        private readonly HttpClient _httpClient;

        public CuzkBuildingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CuzkBuilding?> FindBuildingAsync(Point location, CancellationToken ct)
        {
            var lat = location.Y;
            var lon = location.X;
            var bbox = string.Create(CultureInfo.InvariantCulture, $"{lat - 0.0005},{lon - 0.0008},{lat + 0.0005},{lon + 0.0008}");
            var url = $"{BaseUrl}?service=WFS&version=2.0.0&request=GetFeature&typeNames=bu:Building&count=60&srsName=urn:ogc:def:crs:EPSG::4326&bbox={bbox},urn:ogc:def:crs:EPSG::4326";

            using var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var document = XDocument.Parse(await response.Content.ReadAsStringAsync(ct));

            var buildings = document.Descendants()
                .Where(e => e.Name.LocalName == "Building")
                .Select(ParseBuilding)
                .OfType<CuzkBuilding>()
                .ToList();

            var containing = buildings.FirstOrDefault(b => b.Footprint.Contains(location));
            if (containing != null)
                return containing;

            return buildings
                .Select(b => (Building: b, Distance: DistanceMeters(b.Footprint, location)))
                .Where(c => c.Distance <= MaxDistanceMeters)
                .OrderByDescending(c => PublicUses.Contains(c.Building.CurrentUse))
                .ThenByDescending(c => c.Building.BuiltUpArea ?? 0)
                .Select(c => c.Building)
                .FirstOrDefault();
        }

        private static CuzkBuilding? ParseBuilding(XElement element)
        {
            var posList = Descendant(element, "posList")?.Value;
            if (string.IsNullOrWhiteSpace(posList))
                return null;

            var values = posList.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => double.Parse(v, CultureInfo.InvariantCulture))
                .ToArray();

            if (values.Length < 8)
                return null;

            // GML v EPSG:4326 má pořadí zeměpisná šířka, délka
            var coordinates = new List<Coordinate>();
            for (var i = 0; i + 1 < values.Length; i += 2)
                coordinates.Add(new Coordinate(values[i + 1], values[i]));

            if (!coordinates[0].Equals2D(coordinates[^1]))
                coordinates.Add(coordinates[0]);

            var footprint = Factory.CreatePolygon(coordinates.ToArray());

            var area = element.Descendants().FirstOrDefault(e => e.Name.LocalName == "OfficialArea") is { } officialArea
                && double.TryParse(Descendant(officialArea, "value")?.Value, CultureInfo.InvariantCulture, out var parsedArea)
                ? parsedArea
                : (double?)null;

            var yearText = Descendant(element, "anyPoint")?.Value;
            int? year = yearText != null && DateTime.TryParse(yearText, CultureInfo.InvariantCulture, out var date) ? date.Year : null;

            var currentUse = element.Descendants()
                .Where(e => e.Name.LocalName == "currentUse" && e.Attribute(Xlink + "href") != null)
                .Select(e => UseFromHref(e.Attribute(Xlink + "href")!.Value))
                .FirstOrDefault();

            var condition = Descendant(element, "conditionOfConstruction")?.Attribute(Xlink + "title")?.Value;

            var ruianCode = element.Descendants()
                .Where(e => e.Name.LocalName == "ExternalReference" && (Descendant(e, "informationSystem")?.Value.Contains("vdp") ?? false))
                .Select(e => Descendant(e, "reference")?.Value)
                .FirstOrDefault();

            var parcelHref = Descendant(element, "cadastralParcel")?.Attribute(Xlink + "href")?.Value;
            var parcelRef = parcelHref != null && parcelHref.Contains("Id=") ? parcelHref[(parcelHref.LastIndexOf("Id=", StringComparison.Ordinal) + 3)..] : null;

            return new CuzkBuilding(footprint, area, year, currentUse, condition, parcelRef, ruianCode);
        }

        private static XElement? Descendant(XElement element, string localName)
        {
            return element.Descendants().FirstOrDefault(e => e.Name.LocalName == localName);
        }

        // ".../CurrentUseValue/publicServices/publicServices.en.xml" → "publicServices"
        private static string? UseFromHref(string href)
        {
            var parts = href.Split('/', StringSplitOptions.RemoveEmptyEntries);

            return parts.Length >= 2 ? parts[^2] : null;
        }

        private static double DistanceMeters(Geometry footprint, Point location)
        {
            var nearest = NetTopologySuite.Operation.Distance.DistanceOp.NearestPoints(footprint, location);
            var dLat = (nearest[0].Y - nearest[1].Y) * 111_320;
            var dLon = (nearest[0].X - nearest[1].X) * 111_320 * Math.Cos(location.Y * Math.PI / 180);

            return Math.Sqrt(dLat * dLat + dLon * dLon);
        }
    }
}
