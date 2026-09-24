using System.Text.Json;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Sensors
{
    // Příkazy pro plugin Dynamic Security v Mosquittu (topic $CONTROL/dynamic-security/v1).
    // Každá gateway dostane vlastní účet a roli, která smí jen odebírat své dotazy a posílat své odpovědi —
    // cizí gateway ani nikdo bez hesla tak nemůže číst telemetrii školy ani podvrhnout měření.
    public static class DynamicSecurity
    {
        public const string CommandTopic = "$CONTROL/dynamic-security/v1";
        public const string ResponseTopic = "$CONTROL/dynamic-security/v1/response";

        // Výchozí role administrátora smí všechno číst, ale zapisovat jen na $CONTROL — backend potřebuje posílat dotazy gatewayím.
        // Při prvním přidání broker backend jednou odpojí („administrative action“), aby platila nová práva; pak už jen hlásí „already exists“.
        public static string GrantBackendRequests(string requestTopicFilter)
        {
            return Serialize(new { command = "addRoleACL", rolename = "admin", acltype = "publishClientSend", topic = requestTopicFilter, priority = 0, allow = true });
        }

        // Založení (nebo obnovení) účtu gatewaye. Příkazy jsou idempotentní v praxi: „už existuje“ se ignoruje a heslo se vždy nastaví znovu.
        public static string ProvisionGateway(MqttCredentials credentials, string requestTopic, string responseTopic)
        {
            var role = RoleName(credentials.Username);

            return Serialize(
                new { command = "createRole", rolename = role, textdescription = $"IQRF gateway {credentials.Username}" },
                new { command = "addRoleACL", rolename = role, acltype = "subscribePattern", topic = requestTopic, priority = 0, allow = true },
                new { command = "addRoleACL", rolename = role, acltype = "publishClientSend", topic = responseTopic, priority = 0, allow = true },
                new { command = "createClient", username = credentials.Username, password = credentials.Password, roles = new[] { new { rolename = role } } },
                new { command = "setClientPassword", username = credentials.Username, password = credentials.Password });
        }

        // Odpojená gateway z aplikace ztratí i účet na brokeru
        public static string RemoveGateway(string gatewayId)
        {
            return Serialize(
                new { command = "deleteClient", username = gatewayId },
                new { command = "deleteRole", rolename = RoleName(gatewayId) });
        }

        // Chyby z odpovědi brokeru kromě „už existuje“ (opakované zakládání při každém připojení je záměr)
        public static IReadOnlyList<string> Errors(string responsePayload)
        {
            try
            {
                using var document = JsonDocument.Parse(responsePayload);
                if (!document.RootElement.TryGetProperty("responses", out var responses) || responses.ValueKind != JsonValueKind.Array)
                    return [];

                return responses.EnumerateArray()
                    .Where(r => r.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
                    .Select(r => $"{r.GetProperty("command").GetString()}: {r.GetProperty("error").GetString()}")
                    .Where(e => !e.Contains("already", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static string RoleName(string gatewayId) => $"gateway-{gatewayId}";

        private static string Serialize(params object[] commands) => JsonSerializer.Serialize(new { commands });
    }
}
