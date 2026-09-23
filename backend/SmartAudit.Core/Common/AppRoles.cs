namespace SmartAudit.Core.Common
{
    public static class AppRoles
    {
        // Krajský úřad — vidí celé portfolio včetně Kč
        public const string Region = "Kraj";

        // Správce budovy / ředitel — vidí jen objekty své organizace
        public const string Manager = "Spravce";

        // Veřejnost — jen vnitřní prostředí, bez Kč
        public const string Public = "Verejnost";

        public static readonly string[] All = [Region, Manager, Public];
    }
}
