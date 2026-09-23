namespace SmartAudit.Core.Data.Tables
{
    // Jedno měření — bez vlastního Id, klíč je (SensorId, Timestamp), řádků jsou miliony
    public class SensorReading
    {
        public Guid SensorId { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public double Value { get; private set; }

        private SensorReading() { }

        public SensorReading(Guid sensorId, DateTimeOffset timestamp, double value)
        {
            SensorId = sensorId;
            Timestamp = timestamp;
            Value = value;
        }
    }
}
