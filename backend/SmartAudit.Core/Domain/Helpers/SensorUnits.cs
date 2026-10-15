using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Jednotky veličin na jednom místě — telemetrie, heatmapa, senzory i PDF ukazují totéž
    public static class SensorUnits
    {
        public static string Of(SensorMetric metric, bool perDay = false)
        {
            return metric switch
            {
                SensorMetric.Temperature => "°C",
                SensorMetric.Co2 => "ppm",
                SensorMetric.Power => perDay ? "kWh/den" : "kWh/h",
                SensorMetric.Humidity => "%",
                SensorMetric.Voc => "VOC",
                SensorMetric.Illuminance => "lx",
                SensorMetric.Pressure => "hPa",
                SensorMetric.Pm25 => "µg/m³",
                SensorMetric.Noise => "dB",
                SensorMetric.Motion => "",
                _ => "",
            };
        }

        public static string Label(SensorMetric metric)
        {
            return metric switch
            {
                SensorMetric.Temperature => "Teplota",
                SensorMetric.Co2 => "CO₂",
                SensorMetric.Power => "Příkon",
                SensorMetric.Humidity => "Vlhkost",
                SensorMetric.Voc => "Těkavé látky (VOC)",
                SensorMetric.Illuminance => "Osvětlení",
                SensorMetric.Pressure => "Tlak",
                SensorMetric.Pm25 => "Prach PM2,5",
                SensorMetric.Noise => "Hluk",
                SensorMetric.Motion => "Pohyb",
                _ => "Jiná veličina",
            };
        }
    }
}
