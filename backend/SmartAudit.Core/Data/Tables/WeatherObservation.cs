namespace SmartAudit.Core.Data.Tables
{
    // Hodinové počasí z Open-Meteo pro střed ORP — klíč (OrpCode, Timestamp)
    public class WeatherObservation
    {
        public string OrpCode { get; private set; } = string.Empty;
        public DateTimeOffset Timestamp { get; private set; }

        public double Temperature { get; private set; }
        public double? Pm10 { get; private set; }
        public double? Pm25 { get; private set; }

        private WeatherObservation() { }

        public WeatherObservation(string orpCode, DateTimeOffset timestamp, double temperature)
        {
            if (string.IsNullOrWhiteSpace(orpCode))
                throw new ArgumentException("Kód ORP nesmí být prázdný.", nameof(orpCode));

            OrpCode = orpCode;
            Timestamp = timestamp;
            Temperature = temperature;
        }

        public void SetAirQuality(double? pm10, double? pm25)
        {
            Pm10 = pm10;
            Pm25 = pm25;
        }
    }
}
