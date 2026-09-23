using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Vše, co simulátor potřebuje vědět o jednom senzoru — skládá se z reálných dat objektu
    public record SensorSimulationSpec(
        SensorMetric Metric,
        FacilityCategory Category,
        SimulationProfile Profile,
        ZoneType? ZoneType,
        int Seed,

        // Budova (ČÚZK plocha × odhad podlaží, měrná ztráta podle roku výstavby)
        double FloorArea,
        double SpecificHeatLoss,

        // Obsazenost: 1 = typická, >1 plnější třídy (odvozeno z počtu žáků)
        double OccupancyFactor = 1.0,

        // Zóna, ve které se projevuje problém profilu (např. špatně větraná učebna)
        bool IsProblemZone = false);
}
