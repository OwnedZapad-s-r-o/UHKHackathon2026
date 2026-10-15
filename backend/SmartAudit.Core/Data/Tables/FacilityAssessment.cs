using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Data.Tables
{
    // Poslední vyhodnocení telemetrie objektu — cache pro mapu a portfolio, přepočítává se po každém kroku simulace
    public class FacilityAssessment
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Výsledek
        public AuditStatus Status { get; private set; }
        public string Headline { get; private set; } = string.Empty;
        public string Summary { get; private set; } = string.Empty;
        public string FindingsJson { get; private set; } = "[]";


        // Aktuální hodnoty (poslední hodina)
        public double? LatestTemperature { get; private set; }
        public double? LatestCo2 { get; private set; }
        public double? LatestPower { get; private set; }


        // Ukazatele za posledních 7 dní
        public double Co2HoursOverLimit { get; private set; }
        public double? FreeDayMeanTemperature { get; private set; }
        public double? NightMeanTemperature { get; private set; }
        public double WeeklyEnergyKwh { get; private set; }


        // Peníze
        public decimal AnnualLossCzk { get; private set; }
        public double SavingPercent { get; private set; }


        // Cizí klíče
        public Guid FacilityId { get; private set; }


        // Časové údaje
        public DateTimeOffset EvaluatedAt { get; private set; } = DateTimeOffset.UtcNow;


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;

        private FacilityAssessment() { }

        public FacilityAssessment(Guid facilityId)
        {
            FacilityId = facilityId;
        }

        public void Update(AuditStatus status, string headline, string summary, string findingsJson, double? latestTemperature, double? latestCo2, double? latestPower,
            double co2HoursOverLimit, double? freeDayMeanTemperature, double? nightMeanTemperature, double weeklyEnergyKwh, decimal annualLossCzk, double savingPercent)
        {
            if (string.IsNullOrWhiteSpace(headline))
                throw new ArgumentException("Shrnutí vyhodnocení nesmí být prázdné.", nameof(headline));

            if (annualLossCzk < 0)
                throw new ArgumentException("Ztráta nesmí být záporná.", nameof(annualLossCzk));

            Status = status;
            Headline = headline;
            Summary = summary;
            FindingsJson = findingsJson;
            LatestTemperature = latestTemperature;
            LatestCo2 = latestCo2;
            LatestPower = latestPower;
            Co2HoursOverLimit = co2HoursOverLimit;
            FreeDayMeanTemperature = freeDayMeanTemperature;
            NightMeanTemperature = nightMeanTemperature;
            WeeklyEnergyKwh = weeklyEnergyKwh;
            AnnualLossCzk = annualLossCzk;
            SavingPercent = savingPercent;
            EvaluatedAt = DateTimeOffset.UtcNow;
        }
    }
}
