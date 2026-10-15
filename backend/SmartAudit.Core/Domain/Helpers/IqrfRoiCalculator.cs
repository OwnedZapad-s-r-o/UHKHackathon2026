namespace SmartAudit.Core.Domain.Helpers
{
    public record IqrfKit(int Gateways, int Sensors, decimal HardwareCostCzk);

    public record IqrfRoi(int Gateways, int Sensors, decimal HardwareCostCzk, decimal AnnualSavingsCzk, double? PaybackMonths, string SavingsBasis);

    // Návratnost osazení budovy bezdrátovou sítí MICRORISC IQRF: gateway IQD-GW04 + čidla teploty/CO₂ podle velikosti
    // budovy z ČÚZK. Bez sekání kabelů — čidla jsou bateriová a síť IQRF si cestu k bráně najde sama (mesh).
    public static class IqrfRoiCalculator
    {
        public const decimal GatewayCzk = 8_500m;
        public const decimal SensorCzk = 2_400m;

        // Jedno čidlo na ~250 m² podlahy (reprezentativní místnosti každého podlaží a křídla), aspoň 2 na podlaží
        private const double FloorAreaPerSensor = 250;
        private const int MinSensors = 6;
        private const int MaxSensors = 24;

        // Nad ~6 000 m² podlahy druhá brána, ať mesh nemá příliš mnoho skoků
        private const double FloorAreaPerGateway = 6_000;

        public static IqrfKit Kit(double floorArea, int floors)
        {
            var sensors = Math.Clamp(Math.Max((int)Math.Ceiling(floorArea / FloorAreaPerSensor), 2 * Math.Max(1, floors)), MinSensors, MaxSensors);
            var gateways = Math.Max(1, (int)Math.Ceiling(floorArea / FloorAreaPerGateway));

            return new IqrfKit(gateways, sensors, gateways * GatewayCzk + sensors * SensorCzk);
        }

        // Roční úspora = větší z (ztráta, kterou audit vyčíslil z teplotní řady) a (potenciál chytré regulace z pre-auditu)
        public static IqrfRoi Calculate(IqrfKit kit, decimal measuredLossCzk, decimal regulationPotentialCzk)
        {
            var fromLoss = measuredLossCzk >= regulationPotentialCzk && measuredLossCzk > 0;
            var savings = Math.Max(measuredLossCzk, regulationPotentialCzk);

            double? payback = savings > 0 ? Math.Round((double)(kit.HardwareCostCzk / (savings / 12m)), 1) : null;

            return new IqrfRoi(
                kit.Gateways,
                kit.Sensors,
                kit.HardwareCostCzk,
                Math.Round(savings / 100) * 100,
                payback,
                fromLoss ? "ztráta z přetápění vyčíslená auditem" : "potenciál chytré regulace z pre-auditu");
        }
    }
}
