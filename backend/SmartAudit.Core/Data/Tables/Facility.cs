using NetTopologySuite.Geometries;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    // Objekt portfolia kraje — budova, kterou audit sleduje
    public class Facility
    {
        // Zdrojový klíč budovy, kterou do mapy přidal správce — v otevřených datech není, import ji nesmí smazat
        public const string ManagerSourcePrefix = "spravce:";

        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Identifikace
        public string SourceKey { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public FacilityCategory Category { get; private set; }
        public string[] Kinds { get; private set; } = [];
        public string[] Izos { get; private set; } = [];
        public string? Redizo { get; private set; }
        public string? Ico { get; private set; }
        public string? Web { get; private set; }


        // Adresa a územní členění
        public string? Street { get; private set; }
        public string? HouseNumber { get; private set; }
        public string? Psc { get; private set; }
        public string MunicipalityCode { get; private set; } = string.Empty;
        public string MunicipalityName { get; private set; } = string.Empty;
        public string OrpCode { get; private set; } = string.Empty;
        public string OrpName { get; private set; } = string.Empty;
        public string DistrictCode { get; private set; } = string.Empty;
        public string DistrictName { get; private set; } = string.Empty;
        public Point Location { get; private set; } = null!;


        // Kontext z dalších sad
        public HeritageStatus HeritageStatus { get; private set; }
        public string? HeritageName { get; private set; }


        // Profil fyzikálního modelu telemetrie — odvozuje ho pre-audit z otevřených dat (OpenDataPreAudit), ne náhoda
        public SimulationProfile SimulationProfile { get; private set; }


        // Cizí klíče
        public Guid? OrganizationId { get; private set; }


        // Časové údaje
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EditedAt { get; private set; }


        // Navigační vlastnosti
        public Organization? Organization { get; set; }
        public FacilityBuilding? Building { get; set; }
        public FacilityAssessment? Assessment { get; set; }
        public FacilityPreAudit? PreAudit { get; set; }
        public ICollection<FacilityMetric> Metrics { get; private set; } = new List<FacilityMetric>();
        public ICollection<Zone> Zones { get; private set; } = new List<Zone>();
        public ICollection<Sensor> Sensors { get; private set; } = new List<Sensor>();

        private Facility() { }

        public Facility(string sourceKey, string name, FacilityCategory category, Point location)
        {
            if (string.IsNullOrWhiteSpace(sourceKey))
                throw new ArgumentException("Zdrojový klíč objektu nesmí být prázdný.", nameof(sourceKey));

            SourceKey = sourceKey;
            Category = category;
            Rename(name);
            Relocate(location);
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Název objektu nesmí být prázdný.", nameof(name));

            Name = name.Trim();
            EditedAt = DateTimeOffset.UtcNow;
        }

        public void Relocate(Point location)
        {
            ArgumentNullException.ThrowIfNull(location);

            if (location.SRID != 4326)
                throw new ArgumentException("Poloha objektu musí být v WGS84 (SRID 4326).", nameof(location));

            Location = location;
            EditedAt = DateTimeOffset.UtcNow;
        }

        public void AssignCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Kód objektu nesmí být prázdný.", nameof(code));

            Code = code;
        }

        public void SetIdentifiers(string[] kinds, string[] izos, string? redizo, string? ico, string? web)
        {
            Kinds = kinds;
            Izos = izos;
            Redizo = redizo;
            Ico = ico;
            Web = web;
            EditedAt = DateTimeOffset.UtcNow;
        }

        public void SetAddress(string? street, string? houseNumber, string? psc, string municipalityCode, string municipalityName, string orpCode, string orpName, string districtCode, string districtName)
        {
            if (string.IsNullOrWhiteSpace(municipalityCode))
                throw new ArgumentException("Kód obce nesmí být prázdný.", nameof(municipalityCode));

            Street = street;
            HouseNumber = houseNumber;
            Psc = psc;
            MunicipalityCode = municipalityCode;
            MunicipalityName = municipalityName;
            OrpCode = orpCode;
            OrpName = orpName;
            DistrictCode = districtCode;
            DistrictName = districtName;
            EditedAt = DateTimeOffset.UtcNow;
        }

        public void AssignOrganization(Guid? organizationId)
        {
            OrganizationId = organizationId;
        }

        public void SetHeritage(HeritageStatus status, string? name)
        {
            HeritageStatus = status;
            HeritageName = status == HeritageStatus.None ? null : name;
        }

        public void SetSimulationProfile(SimulationProfile profile)
        {
            SimulationProfile = profile;
        }
    }
}
