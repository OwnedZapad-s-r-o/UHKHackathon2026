namespace SmartAudit.Core.Domain.Enums
{
    public enum SimulationProfile
    {
        Nominal,

        // Kritická anomálie — o víkendu se netlumí, budova drží ~24,5 °C
        WeekendOverheating,

        // Varování — při výuce vysoké CO₂, rekuperace/větrání nestačí
        PoorVentilation,

        // Drobná neefektivita — ranní náběh vytápění příliš brzy
        EarlyHeatingStart,

        // Neefektivita — nefunguje noční útlum
        NoNightSetback,
    }
}
