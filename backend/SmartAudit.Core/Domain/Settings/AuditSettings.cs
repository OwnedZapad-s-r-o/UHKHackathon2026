namespace SmartAudit.Core.Domain.Settings
{
    // Vstupy výpočtu ztrát — záměrně v konfiguraci, aby šlo ukázat, z čeho čísla vznikla (F30)
    public class AuditSettings
    {
        public const string SectionName = "AuditSettings";

        // Cena tepla v Kč za kWh (≈ 600 Kč/GJ)
        public decimal HeatPriceCzkPerKwh { get; set; } = 2.2m;

        // Délka topné sezóny pro přepočet týdenní ztráty na roční
        public int HeatingSeasonWeeks { get; set; } = 33;

        // Průměrná venkovní teplota topné sezóny (odhad)
        public double HeatingSeasonMeanOutdoor { get; set; } = 3.5;

        // Limit CO₂ pro vyhodnocení (ppm)
        public double Co2LimitPpm { get; set; } = 1500;

        // Sada čidel IQRF na jednu budovu bez DPH (gateway, 15 čidel do tříd, 3 do společných prostor, montáž) —
        // pro návratnost pilotu v souhrnu pro Radu kraje, rozpočet v docs/SENZORY_IQRF.md
        public decimal SensorKitCzkPerBuilding { get; set; } = 109_395m;
    }
}
