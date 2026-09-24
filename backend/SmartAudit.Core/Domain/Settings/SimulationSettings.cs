namespace SmartAudit.Core.Domain.Settings
{
    public class SimulationSettings
    {
        public const string SectionName = "SimulationSettings";

        public bool Enabled { get; set; } = true;

        // Kolik dní historie se dopočítá při prvním startu
        public int BackfillDays { get; set; } = 7;

        // Krok měření v minutách (historie i živá data)
        public int StepMinutes { get; set; } = 15;

        // Jak často služba kontroluje nový krok simulace a budovy s novými měřeními z čidel
        // (kontrola bez práce je levná — nový krok se pozná z hodin, přepočet jen dotčených budov)
        public int LiveCheckSeconds { get; set; } = 10;
    }
}
