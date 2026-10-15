namespace SmartAudit.Core.Domain.Settings
{
    // Připojení k MQTT brokeru, přes který mluví IQRF Gateway Daemon
    public class IqrfSettings
    {
        public const string SectionName = "IqrfSettings";

        public bool Enabled { get; set; } = true;

        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1883;
        public bool UseTls { get; set; }

        // Účet backendu = administrátor Dynamic Security (zakládá účty gatewayí)
        public string? Username { get; set; }
        public string? Password { get; set; }

        // Tajemství pro odvození hesla každé gatewaye (HMAC) — heslo se nikde neukládá a jde kdykoli znovu ukázat správci
        public string DeviceSecret { get; set; } = string.Empty;

        // Heslo sdíleného účtu „provisioning“ na brokeru — smí jen ohlásit nespárovanou gatewayi a převzít zašifrovanou nabídku.
        // Prázdné = párování heslem z nálepky je vypnuté.
        public string ProvisioningPassword { get; set; } = string.Empty;

        // Výchozí topicy IQRF Gateway Daemonu; {0} = id gatewaye
        public string ResponseTopicFilter { get; set; } = "gateway/+/iqrf/responses";
        public string RequestTopicTemplate { get; set; } = "gateway/{0}/iqrf/requests";
        public string ResponseTopicTemplate { get; set; } = "gateway/{0}/iqrf/responses";

        // Adresa brokeru, kterou aplikace ukáže v návodu (IP notebooku v síti hackathonu); gatewaye se připojují přes TLS
        public string? PublicHost { get; set; }
        public int PublicPort { get; set; } = 8883;
        public bool PublicTls { get; set; } = true;

        // Veřejný certifikát CA brokeru (vytvoří ho kontejner mqtt) — správce ho nahraje do gatewaye
        public string? CaCertificatePath { get; set; }
    }
}
