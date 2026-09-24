namespace SmartAudit.Core.Domain.Settings
{
    // Připojení k MQTT brokeru, přes který mluví IQRF Gateway Daemon
    public class IqrfSettings
    {
        public const string SectionName = "IqrfSettings";

        public bool Enabled { get; set; } = true;

        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1883;
        public string? Username { get; set; }
        public string? Password { get; set; }

        // Výchozí topicy IQRF Gateway Daemonu; {0} = id gatewaye
        public string ResponseTopicFilter { get; set; } = "gateway/+/iqrf/responses";
        public string RequestTopicTemplate { get; set; } = "gateway/{0}/iqrf/requests";

        // Adresa brokeru, kterou aplikace ukáže v návodu (IP notebooku v síti hackathonu)
        public string? PublicHost { get; set; }
        public int PublicPort { get; set; } = 1883;
    }
}
