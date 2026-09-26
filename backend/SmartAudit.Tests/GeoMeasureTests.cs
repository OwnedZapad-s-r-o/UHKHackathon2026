using NetTopologySuite.Geometries;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Plocha doplněné budovy vstupuje do výpočtu ztráty — přepočet stupňů na metry musí sedět
    public class GeoMeasureTests
    {
        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

        [Fact]
        public void Square_of_20_by_20_meters_in_the_region_has_400_square_meters()
        {
            // Hradec Králové: 20 m na sever ≈ 0,0001797°, 20 m na východ ≈ 0,0002812° (cos 50,21°)
            const double lat = 50.21, lon = 15.83, dLat = 20 / 111_320.0;
            var dLon = 20 / (111_320.0 * Math.Cos(lat * Math.PI / 180));
            var square = Factory.CreatePolygon([new(lon, lat), new(lon + dLon, lat), new(lon + dLon, lat + dLat), new(lon, lat + dLat), new(lon, lat)]);

            Assert.InRange(GeoMeasure.AreaSquareMeters(square), 398, 402);
        }

        [Fact]
        public void Distance_between_buildings_is_in_meters()
        {
            const double lat = 50.21, lon = 15.83;
            var a = Factory.CreatePoint(new Coordinate(lon, lat));
            var b = Factory.CreatePoint(new Coordinate(lon, lat + 50 / 111_320.0));

            Assert.InRange(GeoMeasure.DistanceMeters(a, b), 49.5, 50.5);
        }
    }
}
