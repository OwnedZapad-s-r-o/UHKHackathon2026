namespace SmartAudit.Core.Domain.Enums
{
    // Druh smlouvy z Registru smluv podle předmětu — jen to, co souvisí s energií a provozem budovy
    public enum ContractCategory
    {
        EnergySupply,       // Dodávka elektřiny, plynu, tepla
        FacilityManagement, // Správa budovy, MaR, servis a opravy kotelen a vytápění
        Reconstruction,     // Zateplení, výměna oken, FVE, snížení energetické náročnosti
        Other,
    }
}
