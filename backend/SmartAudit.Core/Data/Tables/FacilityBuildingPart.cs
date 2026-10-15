using NetTopologySuite.Geometries;

namespace SmartAudit.Core.Data.Tables
{
    // Přilehlá budova, kterou k objektu doplnil správce: katastr ČÚZK ji k objektu nevede (křídlo zapsané zvlášť
    // nebo v INSPIRE Budovách chybí), ale v mapě je vidět. Obrys je z podkladové mapy (OpenStreetMap).
    // Plocha se přičítá k zastavěné ploše z ČÚZK ve výpočtu ztráty.
    public class FacilityBuildingPart
    {
        public const double MinAreaSquareMeters = 15;
        public const double MaxAreaSquareMeters = 20_000;

        public Guid Id { get; private set; } = Guid.CreateVersion7();

        public Polygon Footprint { get; private set; } = null!;
        public double AreaSquareMeters { get; private set; }

        // Odkud obrys je — ukazuje se v aplikaci i v otevřených datech
        public string Source { get; private set; } = string.Empty;


        // Cizí klíče
        public Guid FacilityId { get; private set; }


        // Časové údaje
        public DateTimeOffset AddedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;

        private FacilityBuildingPart() { }

        public FacilityBuildingPart(Guid facilityId, Polygon footprint, double areaSquareMeters, string source)
        {
            FacilityId = facilityId;
            Replace(footprint, areaSquareMeters, source, DateTimeOffset.UtcNow);
        }

        // Výměna za jinou budovu z mapy nebo ručně upravený obrys
        public void Replace(Polygon footprint, double areaSquareMeters, string source, DateTimeOffset now)
        {
            if (footprint.IsEmpty || !footprint.IsValid)
                throw new ArgumentException("Obrys budovy musí být platný polygon.", nameof(footprint));

            if (areaSquareMeters is < MinAreaSquareMeters or > MaxAreaSquareMeters)
                throw new ArgumentException($"Plocha budovy musí být {MinAreaSquareMeters}–{MaxAreaSquareMeters} m².", nameof(areaSquareMeters));

            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Zdroj obrysu nesmí být prázdný.", nameof(source));

            Footprint = footprint;
            AreaSquareMeters = areaSquareMeters;
            Source = source;
            UpdatedAt = now;
        }
    }
}
