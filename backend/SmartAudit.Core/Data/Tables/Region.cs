using NetTopologySuite.Geometries;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    // Územní jednotka — kraj, okres, ORP nebo obec, včetně údajů z navázaných sad
    public class Region
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Identifikace
        public RegionType Type { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public Geometry? Geometry { get; private set; }


        // Obec — hospodářsky a sociálně ohrožené oblasti
        public string? OrpCode { get; private set; }
        public bool IsThreatened { get; private set; }
        public string? ThreatLevel { get; private set; }


        // ORP — schválené dotace 2021–2025
        public int? GrantCount { get; private set; }
        public decimal? GrantTotal { get; private set; }
        public int? IndividualGrantCount { get; private set; }
        public decimal? IndividualGrantTotal { get; private set; }
        public int? ProgramGrantCount { get; private set; }
        public decimal? ProgramGrantTotal { get; private set; }


        // ORP — nadmořská výška těžiště z Open-Meteo Elevation API (vstup klimatického parametru pre-auditu)
        public double? Elevation { get; private set; }

        private Region() { }

        public Region(RegionType type, string code, string name, Geometry? geometry = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Kód územní jednotky nesmí být prázdný.", nameof(code));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Název územní jednotky nesmí být prázdný.", nameof(name));

            Type = type;
            Code = code;
            Name = name;
            Geometry = geometry;
        }

        public void SetThreat(string orpCode, bool isThreatened, string? threatLevel)
        {
            OrpCode = orpCode;
            IsThreatened = isThreatened;
            ThreatLevel = isThreatened ? threatLevel : null;
        }

        public void SetGrants(int count, decimal total)
        {
            if (count < 0 || total < 0)
                throw new ArgumentException("Dotace nesmí být záporné.", nameof(count));

            GrantCount = count;
            GrantTotal = total;
        }

        public void SetIndividualGrants(int count, decimal total)
        {
            if (count < 0 || total < 0)
                throw new ArgumentException("Dotace nesmí být záporné.", nameof(count));

            IndividualGrantCount = count;
            IndividualGrantTotal = total;
        }

        public void SetElevation(double? elevation)
        {
            if (elevation is < -500 or > 9000)
                throw new ArgumentException("Nadmořská výška mimo rozsah.", nameof(elevation));

            Elevation = elevation;
        }

        public void SetProgramGrants(int count, decimal total)
        {
            if (count < 0 || total < 0)
                throw new ArgumentException("Dotace nesmí být záporné.", nameof(count));

            ProgramGrantCount = count;
            ProgramGrantTotal = total;
        }
    }
}
