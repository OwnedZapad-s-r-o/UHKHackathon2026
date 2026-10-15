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

        // Rozdělení profilů ~70 / 10 / 10 / 5 / 5 % — stabilní podle klíče objektu
        public static SimulationProfile SimulationProfileFor(string sourceKey, FacilityCategory category)
        {
            var percent = StableHash.Percent(sourceKey);

            var profile = percent switch
            {
                < 70 => SimulationProfile.Nominal,
                < 80 => SimulationProfile.WeekendOverheating,
                < 90 => SimulationProfile.PoorVentilation,
                < 95 => SimulationProfile.EarlyHeatingStart,
                _ => SimulationProfile.NoNightSetback,
            };

            // Nemocnice jedou 24/7 — víkendový útlum ani ranní náběh u nich nedávají smysl
            if (category == FacilityCategory.Health && profile is SimulationProfile.WeekendOverheating or SimulationProfile.EarlyHeatingStart)
                return SimulationProfile.NoNightSetback;

            return profile;
        }
    }
}
