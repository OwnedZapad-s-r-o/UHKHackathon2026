using NetTopologySuite.Geometries;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services.Import
{
    // Památková ochrana místa ze čtyř sad KHK: národní kulturní památky (plošné i bodové), rezervace a zóny.
    // Sdílí ji import portfolia i budova, kterou přidá správce — obě musí dojít ke stejnému výsledku.
    public class HeritageIndex
    {
        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

        private readonly List<(string? Name, Geometry Geometry)> _zones;
        private readonly List<(string? Name, Geometry Geometry)> _reserves;
        private readonly List<(string? Name, Geometry Geometry)> _monumentAreas;
        private readonly List<(string? Name, Point Point)> _monumentPoints;

        private HeritageIndex(List<(string?, Geometry)> zones, List<(string?, Geometry)> reserves, List<(string?, Geometry)> monumentAreas, List<(string?, Point)> monumentPoints)
        {
            _zones = zones;
            _reserves = reserves;
            _monumentAreas = monumentAreas;
            _monumentPoints = monumentPoints;
        }

        public static async Task<HeritageIndex> LoadAsync(IOpenDataImportService importService, CancellationToken ct)
        {
            var monumentPoints = (await importService.LoadAsync("nkp-bodove", ct))
                .Select(f => (Name: f.Text("nazev") ?? f.Text("objekt"), Point: f.Point(Factory)))
                .Where(m => m.Point != null)
                .Select(m => (m.Name, m.Point!))
                .ToList();

            return new HeritageIndex(
                await PolygonsAsync(importService, "pamatkove-zony", ct),
                await PolygonsAsync(importService, "pamatkove-rezervace", ct),
                await PolygonsAsync(importService, "nkp-plosne", ct),
                monumentPoints);
        }

        public (HeritageStatus Status, string? Name) Match(Point location)
        {
            // ~20 m tolerance — adresní bod často leží u vchodu, ne uvnitř obrysu památky
            var monument = _monumentAreas.FirstOrDefault(m => m.Geometry.IsWithinDistance(location, 0.00025)).Name
                ?? _monumentPoints.FirstOrDefault(m => m.Point.IsWithinDistance(location, 0.0003)).Name;

            if (monument != null)
                return (HeritageStatus.NationalMonument, monument);

            if (_reserves.FirstOrDefault(r => r.Geometry.Contains(location)) is { Name: not null } reserve)
                return (HeritageStatus.Reserve, reserve.Name);

            if (_zones.FirstOrDefault(z => z.Geometry.Contains(location)) is { Name: not null } zone)
                return (HeritageStatus.Zone, zone.Name);

            return (HeritageStatus.None, null);
        }

        private static async Task<List<(string?, Geometry)>> PolygonsAsync(IOpenDataImportService importService, string key, CancellationToken ct)
        {
            return (await importService.LoadAsync(key, ct))
                .Where(f => f.Geometry is Polygon or MultiPolygon)
                .Select(f => (f.Text("nazev") ?? f.Text("objekt"), f.Geometry!))
                .ToList();
        }
    }
}
