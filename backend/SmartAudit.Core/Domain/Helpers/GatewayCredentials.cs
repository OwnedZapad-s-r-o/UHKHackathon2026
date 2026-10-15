using System.Security.Cryptography;
using System.Text;

namespace SmartAudit.Core.Domain.Helpers
{
    public record MqttCredentials(string Username, string Password);

    // Přihlašovací údaje gatewaye k MQTT brokeru: jméno = id gatewaye, heslo = HMAC-SHA256(tajemství, id).
    // Heslo se tak nemusí ukládat do databáze, broker ho zná přes Dynamic Security a správce ho může kdykoli znovu zobrazit.
    // Stejný výpočet má emulátor (tools/iqrf-emulator), aby se v demu připojil bez ručního kopírování hesla.
    public static class GatewayCredentials
    {
        public static MqttCredentials For(string deviceSecret, string gatewayId)
        {
            var key = Encoding.UTF8.GetBytes(deviceSecret);
            var hash = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes($"mqtt:{gatewayId}"));

            // Base64url bez výplně — bez znaků, které by vadily v JSON konfiguraci gatewaye nebo v příkazové řádce
            var password = Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_')[..24];

            return new MqttCredentials(gatewayId, password);
        }
    }
}
