namespace SmartAudit.Core.Domain.Settings
{
    // Stahování smluv z Registru smluv (smlouvy.gov.cz) — běží na pozadí po startu, poslední stav je v seed/opendata
    public class ContractRegistrySettings
    {
        public const string SectionName = "ContractRegistrySettings";

        // false = jen snímek ze seed/opendata, žádné dotazy do registru
        public bool Enabled { get; set; } = true;

        // Po kolika dnech se smlouvy organizace stáhnou znovu
        public int RefreshAfterDays { get; set; } = 7;

        // Odstup mezi dotazy — registr je veřejná služba státu, ne naše API
        public int RequestDelayMs { get; set; } = 1000;

        // Po tolika selháních za sebou se stahování přeruší (registr nedostupný nebo nás omezuje)
        public int MaxConsecutiveFailures { get; set; } = 3;

        // Z příloh (PDF, DOCX) se čte cena energie jen u smluv na dodávku — větší soubory (skeny) se přeskočí
        public int MaxPdfMegabytes { get; set; } = 15;
    }
}
