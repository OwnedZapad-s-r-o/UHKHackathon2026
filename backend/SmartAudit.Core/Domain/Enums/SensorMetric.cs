namespace SmartAudit.Core.Domain.Enums
{
    // Veličiny měřené senzory — jednotky v Helpers/SensorUnits
    public enum SensorMetric
    {
        // °C
        Temperature,

        // ppm
        Co2,

        // kWh za hodinu (průměrný příkon objektu)
        Power,

        // % relativní vlhkosti
        Humidity,

        // index VOC (IQRF: hodnota 0–500)
        Voc,

        // lx
        Illuminance,

        // hPa
        Pressure,

        // µg/m³
        Pm25,

        // dB
        Noise,

        // 0/1 — pohyb nebo obsazenost (PIR)
        Motion,

        // Veličina, kterou aplikace nezná — ukládá se s jednotkou ze senzoru
        Other,
    }
}
