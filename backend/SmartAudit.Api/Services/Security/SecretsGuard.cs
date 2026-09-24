using Npgsql;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Security
{
    // Mimo Development aplikace odmítne nastartovat s ukázkovými tajemstvími z docker-compose a appsettings.
    // Veřejně známé JWT tajemství by komukoli dovolilo podepsat si token role Kraj, výchozí heslo MQTT zase připojit falešnou gateway.
    public static class SecretsGuard
    {
        private static readonly string[] DemoMarkers = ["change-me", "local-demo"];
        private const int MinimumSecretLength = 32;

        public static void EnsureProductionSecrets(IHostEnvironment environment, JwtSettings jwt, IqrfSettings iqrf, string connectionString)
        {
            if (environment.IsDevelopment())
                return;

            var problems = new List<string>();

            if (IsWeak(jwt.Secret))
                problems.Add($"JWT_SECRET musí být vlastní náhodný řetězec (min. {MinimumSecretLength} znaků)");

            if (iqrf.Enabled)
            {
                if (IsDemo(iqrf.Password))
                    problems.Add("MQTT_BACKEND_PASSWORD nesmí být ukázkové heslo");

                if (IsWeak(iqrf.DeviceSecret))
                    problems.Add($"MQTT_DEVICE_SECRET musí být vlastní náhodný řetězec (min. {MinimumSecretLength} znaků)");
            }

            var database = new NpgsqlConnectionStringBuilder(connectionString);
            if (database.Password is null or "smartaudit")
                problems.Add("POSTGRES_PASSWORD nesmí být výchozí heslo databáze");

            if (problems.Count > 0)
                throw new InvalidOperationException($"Aplikace v prostředí {environment.EnvironmentName} nenastartuje s ukázkovými tajemstvími: {string.Join("; ", problems)}. Nastavte je v .env (vzor v .env.example).");
        }

        private static bool IsWeak(string? secret) => secret is null || secret.Length < MinimumSecretLength || IsDemo(secret);

        private static bool IsDemo(string? secret) => string.IsNullOrEmpty(secret) || DemoMarkers.Any(marker => secret.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}
