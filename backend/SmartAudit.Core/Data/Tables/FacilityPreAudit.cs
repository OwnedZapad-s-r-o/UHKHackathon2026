using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    // Pre-audit objektu z otevřených dat (ČÚZK, Data KHK, Registr smluv, Open-Meteo) — přepočítává se po importu dat,
    // po stažení smluv a po živé synchronizaci. Z něj se odvozuje profil fyzikálního modelu telemetrie a kalkulace IQRF.
    public class FacilityPreAudit
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Výsledek modelu
        public int Score { get; private set; }
        public AuditStatus Status { get; private set; }
        public SimulationProfile Profile { get; private set; }
        public string DominantDriver { get; private set; } = string.Empty;
        public string ParametersJson { get; private set; } = "[]";
        public string FindingsJson { get; private set; } = "[]";


        // Fyzika a ekonomika budovy
        public double SpecificHeatLoss { get; private set; }
        public double HeatLossWattsPerKelvin { get; private set; }
        public double DegreeDays { get; private set; }
        public double AnnualHeatDemandKwh { get; private set; }
        public decimal AnnualHeatCostCzk { get; private set; }
        public decimal HeatPriceCzkPerKwh { get; private set; }
        public bool HeatPriceFromContract { get; private set; }
        public double RegulationSavingPercent { get; private set; }
        public decimal RegulationPotentialCzk { get; private set; }


        // Sada IQRF pro tuto budovu
        public int IqrfGateways { get; private set; }
        public int IqrfSensors { get; private set; }
        public decimal IqrfHardwareCostCzk { get; private set; }


        // Cizí klíče
        public Guid FacilityId { get; private set; }


        // Časové údaje
        public DateTimeOffset ComputedAt { get; private set; } = DateTimeOffset.UtcNow;


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;

        private FacilityPreAudit() { }

        public FacilityPreAudit(Guid facilityId)
        {
            FacilityId = facilityId;
        }

        public void Update(int score, AuditStatus status, SimulationProfile profile, string dominantDriver, string parametersJson, string findingsJson,
            double specificHeatLoss, double heatLossWattsPerKelvin, double degreeDays, double annualHeatDemandKwh, decimal annualHeatCostCzk,
            decimal heatPriceCzkPerKwh, bool heatPriceFromContract, double regulationSavingPercent, decimal regulationPotentialCzk,
            int iqrfGateways, int iqrfSensors, decimal iqrfHardwareCostCzk)
        {
            if (score is < 0 or > 100)
                throw new ArgumentException("Skóre pre-auditu musí být 0–100.", nameof(score));

            if (annualHeatCostCzk < 0 || regulationPotentialCzk < 0 || iqrfHardwareCostCzk < 0)
                throw new ArgumentException("Částky nesmí být záporné.", nameof(annualHeatCostCzk));

            Score = score;
            Status = status;
            Profile = profile;
            DominantDriver = dominantDriver;
            ParametersJson = parametersJson;
            FindingsJson = findingsJson;
            SpecificHeatLoss = specificHeatLoss;
            HeatLossWattsPerKelvin = heatLossWattsPerKelvin;
            DegreeDays = degreeDays;
            AnnualHeatDemandKwh = annualHeatDemandKwh;
            AnnualHeatCostCzk = annualHeatCostCzk;
            HeatPriceCzkPerKwh = heatPriceCzkPerKwh;
            HeatPriceFromContract = heatPriceFromContract;
            RegulationSavingPercent = regulationSavingPercent;
            RegulationPotentialCzk = regulationPotentialCzk;
            IqrfGateways = iqrfGateways;
            IqrfSensors = iqrfSensors;
            IqrfHardwareCostCzk = iqrfHardwareCostCzk;
            ComputedAt = DateTimeOffset.UtcNow;
        }
    }
}
