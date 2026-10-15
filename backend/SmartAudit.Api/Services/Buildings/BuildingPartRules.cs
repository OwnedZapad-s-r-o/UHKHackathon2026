using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Buildings
{
    public record CheckedBuildingPart(Polygon Footprint, double AreaSquareMeters);

    // Společná pravidla pro doplnění, výměnu i ruční úpravu přilehlé budovy: platný obrys, rozumná plocha,
    // blízko objektu a nejde o budovu, která už k objektu patří (z ČÚZK nebo doplněnou dřív)
    public static class BuildingPartRules
    {
        // Přilehlá budova musí stát u objektu — ne o ulici dál
        public const double MaxDistanceMeters = 100;

        public const int MaxParts = 10;

        // Budova z větší části překrytá jiným obrysem objektu je tatáž budova
        private const double DuplicateOverlap = 0.5;

        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

        // excludePartId = měněná budova (při výměně se nesrovnává sama se sebou). Vrací obrys nebo českou chybu.
        public static async Task<(CheckedBuildingPart? Part, string? Error)> CheckAsync(
            SmartAuditDbContext dbContext, Guid facilityId, Geometry anchor, Geometry? cadastre, Geometry? geometry, Guid? excludePartId, CancellationToken ct)
        {
            if (geometry is not Polygon polygon || polygon.IsEmpty)
                return (null, "Vyberte v mapě budovu (obrys musí být polygon)");

            // Obrys z dlaždic mapy nebo z ruční úpravy může mít drobné chyby (překřížené hrany) — Buffer(0) je opraví
            var repaired = polygon.IsValid ? polygon : polygon.Buffer(0) as Polygon;
            if (repaired == null || repaired.IsEmpty)
                return (null, "Obrys budovy se nepodařilo zpracovat — hrany se nesmí křížit");

            var footprint = Factory.CreatePolygon(repaired.ExteriorRing.Coordinates);

            var area = Math.Round(GeoMeasure.AreaSquareMeters(footprint));
            if (area is < FacilityBuildingPart.MinAreaSquareMeters or > FacilityBuildingPart.MaxAreaSquareMeters)
                return (null, $"Budova má {area:N0} m² — doplnit jde budovu o ploše {FacilityBuildingPart.MinAreaSquareMeters:N0}–{FacilityBuildingPart.MaxAreaSquareMeters:N0} m²");

            if (GeoMeasure.DistanceMeters(anchor, footprint) > MaxDistanceMeters)
                return (null, $"Budova je od objektu dál než {MaxDistanceMeters:N0} m — doplnit jde jen budovu, která k objektu patří");

            if (cadastre != null && Overlap(cadastre, footprint) > DuplicateOverlap)
                return (null, "Tahle budova už k objektu patří — je v katastru ČÚZK");

            var others = await dbContext.FacilityBuildingParts
                .AsNoTracking()
                .Where(p => p.FacilityId == facilityId && p.Id != excludePartId)
                .Select(p => p.Footprint)
                .ToListAsync(ct);

            if (excludePartId == null && others.Count >= MaxParts)
                return (null, $"K objektu jde doplnit nejvýš {MaxParts} budov");

            if (others.Any(p => Overlap(p, footprint) > DuplicateOverlap))
                return (null, "Tuhle budovu už jste k objektu doplnili");

            return (new CheckedBuildingPart(footprint, area), null);
        }

        // Jaká část doplňované budovy leží v jiném obrysu
        private static double Overlap(Geometry existing, Polygon added)
        {
            var intersection = existing.Intersection(added);
            return added.Area > 0 ? intersection.Area / added.Area : 0;
        }
    }
}
