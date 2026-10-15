using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartAudit.Core.Domain.Helpers
{
    public record AnnouncedNode(int Address, IReadOnlyList<int> SensorTypes);

    // Ohlášení nespárované gatewaye: co je zač a co umí měřit. Nikdy v něm není tajemství.
    public record PairingAnnouncement(string Serial, string Model, IReadOnlyList<AnnouncedNode> Nodes, int? Rssi, bool Emulated, DateTimeOffset Timestamp);

    // Párování gatewaye přes broker, podobně jako připojení k Wi-Fi:
    // 1) nespárovaná gateway se přihlásí sdíleným účtem „provisioning“ a každých pár sekund se ohlásí (provisioning/announce/{serial}),
    // 2) uživatel v aplikaci zadá heslo z nálepky; aplikace jím zašifruje nabídku s přihlašovacími údaji k brokeru
    //    (AES-GCM, klíč PBKDF2 z hesla a serialu) a pošle ji na provisioning/{serial}/offer,
    // 3) gateway nabídku rozšifruje svým heslem a odpoví na provisioning/{serial}/result — ok, nebo „špatné heslo“.
    // Heslo tak nejde po síti ani do databáze a nabídku nepřečte nikdo, kdo ho nezná (ani jiné zařízení na sdíleném účtu).
    // Stejný protokol má Python emulátor (tools/iqrf-emulator/pairing.py).
    public static partial class PairingProtocol
    {
        public const string AccountName = "provisioning";
        public const string AnnounceFilter = "provisioning/announce/+";
        public const string OfferFilter = "provisioning/+/offer";
        public const string ResultFilter = "provisioning/+/result";

        public const string WrongPassword = "wrong-password";

        private const int Iterations = 100_000;
        private const int NonceSize = 12;
        private const int TagSize = 16;

        public static string AnnounceTopic(string serial) => $"provisioning/announce/{serial}";

        public static string OfferTopic(string serial) => $"provisioning/{serial}/offer";

        public static string ResultTopic(string serial) => $"provisioning/{serial}/result";

        // Serial = MID modulu IQRF, 8 hexadecimálních znaků
        public static bool IsValidSerial(string? serial) => serial != null && SerialPattern().IsMatch(serial);

        public static string? AnnouncedSerial(string topic)
        {
            var segments = topic.Split('/');
            return segments.Length == 3 && segments[0] == "provisioning" && segments[1] == "announce" && IsValidSerial(segments[2]) ? segments[2].ToUpperInvariant() : null;
        }

        public static string? ResultSerial(string topic)
        {
            var segments = topic.Split('/');
            return segments.Length == 3 && segments[0] == "provisioning" && segments[2] == "result" && IsValidSerial(segments[1]) ? segments[1].ToUpperInvariant() : null;
        }

        public static string? OfferSerial(string topic)
        {
            var segments = topic.Split('/');
            return segments.Length == 3 && segments[0] == "provisioning" && segments[2] == "offer" && IsValidSerial(segments[1]) ? segments[1].ToUpperInvariant() : null;
        }

        public static string Announcement(PairableGateway gateway, int rssi, DateTimeOffset now)
        {
            return JsonSerializer.Serialize(new
            {
                serial = gateway.Serial,
                kind = "gateway",
                model = gateway.Model,
                nodes = gateway.Nodes.Select(n => new { address = n.Address, sensors = n.Sensors.Select(s => s.Type) }),
                rssi,
                emulated = true,
                ts = now.ToUnixTimeSeconds(),
            });
        }

        public static PairingAnnouncement? ParseAnnouncement(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("serial", out var serial) || !IsValidSerial(serial.GetString()))
                    return null;

                var nodes = root.TryGetProperty("nodes", out var list) && list.ValueKind == JsonValueKind.Array
                    ? list.EnumerateArray()
                        .Where(n => n.ValueKind == JsonValueKind.Object && n.TryGetProperty("address", out var a) && a.ValueKind == JsonValueKind.Number)
                        .Select(n => new AnnouncedNode(
                            n.GetProperty("address").GetInt32(),
                            n.TryGetProperty("sensors", out var s) && s.ValueKind == JsonValueKind.Array
                                ? s.EnumerateArray().Where(t => t.ValueKind == JsonValueKind.Number).Select(t => t.GetInt32()).ToList()
                                : []))
                        .Where(n => n.Address is > 0 and < 240)
                        .ToList()
                    : [];

                return new PairingAnnouncement(
                    serial.GetString()!.ToUpperInvariant(),
                    root.TryGetProperty("model", out var model) && model.ValueKind == JsonValueKind.String ? model.GetString()![..Math.Min(model.GetString()!.Length, 40)] : "IQRF gateway",
                    nodes,
                    root.TryGetProperty("rssi", out var rssi) && rssi.ValueKind == JsonValueKind.Number ? rssi.GetInt32() : null,
                    root.TryGetProperty("emulated", out var emulated) && emulated.ValueKind == JsonValueKind.True,
                    root.TryGetProperty("ts", out var ts) && ts.ValueKind == JsonValueKind.Number ? DateTimeOffset.FromUnixTimeSeconds(ts.GetInt64()) : DateTimeOffset.UtcNow);
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
            {
                return null;
            }
        }

        // Nabídka připojení zašifrovaná heslem z nálepky; serial je navíc přidaná data AES-GCM — nabídka pro jinou gateway neprojde
        public static string SealOffer(string password, string serial, MqttCredentials credentials)
        {
            var plaintext = JsonSerializer.SerializeToUtf8Bytes(new { gatewayId = credentials.Username, username = credentials.Username, password = credentials.Password });
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var cipher = new byte[plaintext.Length];
            var tag = new byte[TagSize];

            using (var aes = new AesGcm(Key(password, serial), TagSize))
                aes.Encrypt(nonce, plaintext, cipher, tag, Encoding.UTF8.GetBytes(serial.ToUpperInvariant()));

            return JsonSerializer.Serialize(new { v = 1, nonce = Convert.ToBase64String(nonce), data = Convert.ToBase64String([.. cipher, .. tag]) });
        }

        // Rozšifrování na straně gatewaye (virtuální gatewaye v aplikaci); null = špatné heslo nebo poškozená nabídka
        public static MqttCredentials? OpenOffer(string password, string serial, string offerJson)
        {
            try
            {
                using var document = JsonDocument.Parse(offerJson);
                var nonce = Convert.FromBase64String(document.RootElement.GetProperty("nonce").GetString()!);
                var data = Convert.FromBase64String(document.RootElement.GetProperty("data").GetString()!);
                if (nonce.Length != NonceSize || data.Length < TagSize)
                    return null;

                var plaintext = new byte[data.Length - TagSize];
                using (var aes = new AesGcm(Key(password, serial), TagSize))
                    aes.Decrypt(nonce, data[..^TagSize], data[^TagSize..], plaintext, Encoding.UTF8.GetBytes(serial.ToUpperInvariant()));

                using var offer = JsonDocument.Parse(plaintext);
                return new MqttCredentials(offer.RootElement.GetProperty("username").GetString()!, offer.RootElement.GetProperty("password").GetString()!);
            }
            catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException or KeyNotFoundException or InvalidOperationException)
            {
                return null;
            }
        }

        public static string Result(bool ok) => JsonSerializer.Serialize(ok ? new { ok = true, reason = (string?)null } : new { ok = false, reason = (string?)WrongPassword });

        // true = spárováno, false = zařízení heslo odmítlo, null = nesrozumitelná odpověď
        public static bool? ParseResult(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                return document.RootElement.TryGetProperty("ok", out var ok) && ok.ValueKind is JsonValueKind.True or JsonValueKind.False ? ok.GetBoolean() : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static byte[] Key(string password, string serial)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(PairingCode.Normalize(password)),
                Encoding.UTF8.GetBytes($"smartaudit-pairing:{serial.ToUpperInvariant()}"),
                Iterations,
                HashAlgorithmName.SHA256,
                32);
        }

        [GeneratedRegex("^[0-9A-Fa-f]{8}$")]
        private static partial Regex SerialPattern();
    }
}
