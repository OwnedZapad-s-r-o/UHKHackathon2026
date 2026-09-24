using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    // Typy senzorů podle IQRF Standard Sensor (pole "type" v odpovědi iqrfSensor_*) → veličina aplikace.
    // Díky tomu se u nového čidla veličina nastaví sama a uživatel vybírá jen místnost.
    public static class IqrfSensorTypes
    {
        private static readonly Dictionary<int, SensorMetric> Known = new()
        {
            [1] = SensorMetric.Temperature,
            [2] = SensorMetric.Co2,
            [3] = SensorMetric.Voc,
            [11] = SensorMetric.Illuminance,
            [16] = SensorMetric.Pressure,
            [18] = SensorMetric.Pm25,
            [19] = SensorMetric.Noise,
            [128] = SensorMetric.Humidity,
            [129] = SensorMetric.Motion,
        };

        public static SensorMetric MetricOf(int iqrfType) => Known.GetValueOrDefault(iqrfType, SensorMetric.Other);
    }
}
