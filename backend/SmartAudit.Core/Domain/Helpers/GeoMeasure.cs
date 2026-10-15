using NetTopologySuite.Geometries;

namespace SmartAudit.Core.Domain.Helpers
{
    // Plocha a vzdálenost geometrií v EPSG:4326 v metrech — přepočet stupňů na metry podle zeměpisné šířky.
    // Na velikost budovy a vzdálenost pár set metrů je chyba zanedbatelná.
    public static class GeoMeasure
    {
        private const double MetersPerDegreeLatitude = 111_320;

        public static double AreaSquareMeters(Geometry geometry)
        {
            if (geometry.IsEmpty)
                return 0;

            return geometry.Area * MetersPerDegreeLatitude * MetersPerDegreeLatitude * LongitudeScale(geometry);
        }

        public static double DistanceMeters(Geometry a, Geometry b)
        {
            var nearest = NetTopologySuite.Operation.Distance.DistanceOp.NearestPoints(a, b);
            var dLat = (nearest[0].Y - nearest[1].Y) * MetersPerDegreeLatitude;
            var dLon = (nearest[0].X - nearest[1].X) * MetersPerDegreeLatitude * LongitudeScale(a);

            return Math.Sqrt(dLat * dLat + dLon * dLon);
        }

        private static double LongitudeScale(Geometry geometry) => Math.Cos(geometry.Centroid.Y * Math.PI / 180);
    }
}
