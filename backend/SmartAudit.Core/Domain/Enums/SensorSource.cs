namespace SmartAudit.Core.Domain.Enums
{
    public enum SensorSource
    {
        // Simulace (SensorSimulator) — dokud objekt nemá skutečné čidlo
        Simulated,

        // Skutečné čidlo připojené přes IQRF gateway
        Physical,
    }
}
