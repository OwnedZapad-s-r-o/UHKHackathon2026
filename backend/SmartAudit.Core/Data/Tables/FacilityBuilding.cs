using NetTopologySuite.Geometries;
using SmartAudit.Core.Domain.Enums;

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

        public void SetEstimates(EnergyCategory energyCategory, int estimatedFloors)
        {
            if (estimatedFloors < 1)
                throw new ArgumentException("Počet podlaží musí být alespoň 1.", nameof(estimatedFloors));

            EnergyCategory = energyCategory;
            EstimatedFloors = estimatedFloors;
        }
    }
}
