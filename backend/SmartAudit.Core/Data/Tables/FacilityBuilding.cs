using NetTopologySuite.Geometries;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Core.Data.Tables
{
    // Karta budovy z ČÚZK INSPIRE Buildings (F20) — jen 2D, výšky ČÚZK neposkytuje
    public class FacilityBuilding
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Výsledek dotazu do ČÚZK
        public bool Found { get; private set; }
        public Geometry? Footprint { get; private set; }
        public double? BuiltUpArea { get; private set; }
        public int? ConstructionYear { get; private set; }
        public string? CurrentUse { get; private set; }
        public string? Condition { get; private set; }
        public string? ParcelRef { get; private set; }
        public string? RuianCode { get; private set; }


        // Odvozené odhady
        public EnergyCategory EnergyCategory { get; private set; }
        public int EstimatedFloors { get; private set; } = 2;


        // Opravy od správce — mají přednost před ČÚZK a odhady; původní hodnoty zůstávají pro srovnání.
        // AreaOverride je zastavěná plocha celého objektu (nahrazuje ČÚZK i doplněné přilehlé budovy).
        public double? AreaOverride { get; private set; }
        public int? FloorsOverride { get; private set; }
        public int? YearOverride { get; private set; }
        public DateTimeOffset? EditedAt { get; private set; }

        public double? EffectiveArea(double addedPartsArea) => AreaOverride ?? (addedPartsArea > 0 ? (BuiltUpArea ?? 0) + addedPartsArea : BuiltUpArea);

        public int EffectiveFloors => FloorsOverride ?? EstimatedFloors;

        public int? EffectiveYear => YearOverride ?? ConstructionYear;

        public EnergyCategory EffectiveEnergyCategory => YearOverride is { } year ? BuildingEstimator.CategoryFromYear(year) : EnergyCategory;


        // Cizí klíče
        public Guid FacilityId { get; private set; }


        // Časové údaje
        public DateTimeOffset FetchedAt { get; private set; } = DateTimeOffset.UtcNow;


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;

        private FacilityBuilding() { }

        public FacilityBuilding(Guid facilityId)
        {
            FacilityId = facilityId;
        }

        public void SetCadastre(Geometry? footprint, double? builtUpArea, int? constructionYear, string? currentUse, string? condition, string? parcelRef, string? ruianCode)
        {
            if (builtUpArea is < 0)
                throw new ArgumentException("Zastavěná plocha nesmí být záporná.", nameof(builtUpArea));

            Found = footprint != null;
            Footprint = footprint;
            BuiltUpArea = builtUpArea;
            ConstructionYear = constructionYear;
            CurrentUse = currentUse;
            Condition = condition;
            ParcelRef = parcelRef;
            RuianCode = ruianCode;
            FetchedAt = DateTimeOffset.UtcNow;
        }

        // null = vrátit údaj z ČÚZK / odhadu
        public void SetOverrides(double? area, int? floors, int? year, DateTimeOffset now)
        {
            if (area is <= 0 or > 200_000)
                throw new ArgumentException("Zastavěná plocha musí být 1–200 000 m².", nameof(area));

            if (floors is < 1 or > 40)
                throw new ArgumentException("Počet podlaží musí být 1–40.", nameof(floors));

            if (year is < 1000 || year > now.Year)
                throw new ArgumentException("Rok výstavby musí být mezi 1000 a letoškem.", nameof(year));

            AreaOverride = area;
            FloorsOverride = floors;
            YearOverride = year;
            EditedAt = area == null && floors == null && year == null ? null : now;
        }

        public void SetEstimates(EnergyCategory energyCategory, int estimatedFloors)
        {
            if (estimatedFloors < 1)
                throw new ArgumentException("Počet podlaží musí být alespoň 1.", nameof(estimatedFloors));

            EnergyCategory = energyCategory;
            EstimatedFloors = estimatedFloors;
        }
    }
}
