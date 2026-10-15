using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Výchozí hodnoty nového objektu — stejné pro import z otevřených dat i budovu, kterou přidá správce
    public static class FacilityDefaults
    {
        public static string CodePrefix(FacilityCategory category)
        {
            return category switch
            {
                FacilityCategory.School => "SKO",
                FacilityCategory.Social => "SOC",
                FacilityCategory.Health => "ZDR",
                _ => "KUL",
            };
        }
    }
}
