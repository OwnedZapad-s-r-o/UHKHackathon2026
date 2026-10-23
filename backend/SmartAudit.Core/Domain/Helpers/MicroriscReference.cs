namespace SmartAudit.Core.Domain.Helpers
{
    public record ReferenceZone(string Name, int Floor, int NodeAddress);

    // Referenční objekt MICRORISC s.r.o. (výrobce IQRF) — Průmyslová 1275, Jičín. Údaje z výpisu z katastru (LV 8314),
    // RÚIAN (stavební objekt 54097673) a skutečná měření ze tří skladových prostor (seed/microrisc, 6× CSV).
    public static class MicroriscReference
    {
        public const string SourceKey = "referencni:microrisc-jicin";
        public const string Code = "PRI-5207-MR1";
        public const string Name = "MICRORISC s.r.o. – Vývojové a skladové centrum IQRF";
        public const string Kind = "Vývojové a skladové centrum";
        public const string Ico = "25261681";
        public const string OrganizationName = "MICRORISC s.r.o.";
        public const string LegalForm = "Společnost s ručením omezeným";
        public const string Web = "https://www.microrisc.cz/cs";

        // Adresa a územní členění
        public const string Street = "Průmyslová";
        public const string HouseNumber = "1275";
        public const string Psc = "50601";
        public const string MunicipalityCode = "572659";
        public const string MunicipalityName = "Jičín";
        public const string OrpCode = "5207";
        public const string OrpName = "Jičín";
        public const string DistrictCode = "CZ0522";
        public const string DistrictName = "Jičín";
        public const double Latitude = 50.4279446;
        public const double Longitude = 15.3696752;

        // Katastrální karta (LV 8314) a technické údaje stavebního objektu z RÚIAN
        public const string CadastralTerritory = "Jičín [659541]";
        public const string OwnershipSheet = "8314";
        public const string ParcelNumbers = "st. 4314, st. 4520";
        public const double ParcelArea = 846;
        public const string MunicipalityPart = "Valdické Předměstí [410225]";
        public const string StructureUse = "víceúčelová stavba";
        public const string RuianCode = "54097673";
        public const int ConstructionYear = 2013;
        public const double BuiltUpArea = 356;
        public const double FloorArea = 904;
        public const double EnclosedVolume = 4263;
        public const int Floors = 3;
        public const string HeatingType = "centrální domovní – tepelné čerpadlo";

        // Půdorys budovy z ČÚZK INSPIRE Budovy (stavební objekt 54097673, staženo 23. 10. 2026) — záloha, když WFS neodpoví
        public const string FootprintWkt =
            "POLYGON ((15.3695630 50.4278490, 15.3693540 50.4278320, 15.3693730 50.4277390, 15.3692950 50.4277320, 15.3693280 50.4275600, "
            + "15.3693290 50.4275550, 15.3695520 50.4275730, 15.3695510 50.4275780, 15.3695270 50.4277010, 15.3695910 50.4277060, "
            + "15.3695770 50.4277740, 15.3698720 50.4277980, 15.3698430 50.4279450, 15.3697410 50.4279370, 15.3696900 50.4279620, "
            + "15.3696510 50.4279300, 15.3695490 50.4279210, 15.3695630 50.4278490))";

        // IQRF gateway objektu a uzly čidel (jeden uzel = teplota + relativní vlhkost v jednom prostoru)
        public const string GatewayId = "gw-jc-microrisc";
        public const string GatewayName = "MICRORISC Jičín — referenční síť IQRF";
        public const string DatasetKey = "microrisc-telemetrie";
        public const string DataDirectory = "microrisc";

        // Prostory podle názvů sloupců v exportech CSV
        public static readonly ReferenceZone[] Zones =
        [
            new("Sklad 1", 0, 1),
            new("Sklad 2", 0, 2),
            new("Mezisklad", 0, 3),
        ];
    }
}
