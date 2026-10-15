using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartAudit.Core.Domain.Helpers
{
    public record IqrfReading(string ChannelKey, int NodeAddress, int Type, string Name, string Unit, double Value);

    // Čidlo v dosahu gatewaye, které ještě není v její síti (rozšíření emulátoru smartaudit_NearbyNodes)
    public record IqrfNearbyNode(string Mid, IReadOnlyList<int> SensorTypes, int? Rssi);

    public record IqrfResponse(string MessageType, string? MessageId, int? NodeAddress, int Status, string? StatusText, IReadOnlyList<IqrfReading> Readings, IReadOnlyList<int>? BondedNodes)
    {
        public bool IsOk => Status == 0;

        // Adresa, kterou koordinátor přidělil čidlu přidanému přes SmartConnect
        public int? AssignedAddress { get; init; }

        public IReadOnlyList<IqrfNearbyNode>? NearbyNodes { get; init; }
    }

    // Odpovědi IQRF Gateway Daemonu (IQRF JSON API) → hodnoty senzorů.
    // Podporuje iqrfSensor_ReadSensorsWithTypes (jeden uzel, všechny senzory), iqrfSensor_Frc (jeden typ, všechny uzly)
    // a iqrfEmbedCoordinator_BondedDevices (seznam spárovaných uzlů pro dotazování).
    public static partial class IqrfResponseParser
    {
        public const string ReadSensorsWithTypes = "iqrfSensor_ReadSensorsWithTypes";
        public const string SensorFrc = "iqrfSensor_Frc";
        public const string BondedDevices = "iqrfEmbedCoordinator_BondedDevices";
        public const string SmartConnect = "iqmeshNetwork_SmartConnect";

        // Vlastní rozšíření emulátoru a virtuálních gatewayí: čidla v dosahu mimo síť. Skutečný daemon ho nezná a odpoví chybou.
        public const string NearbyNodes = "smartaudit_NearbyNodes";

        public static IqrfResponse? Parse(string json)
        {
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                return null;
            }

            using (document)
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("mType", out var mType) || mType.ValueKind != JsonValueKind.String)
                    return null;

                var messageType = mType.GetString()!;
                var data = root.TryGetProperty("data", out var d) ? d : default;
                var messageId = String(data, "msgId");
                var status = Int(data, "status") ?? 0;
                var statusText = String(data, "statusStr");
                var rsp = Property(data, "rsp");
                var nodeAddress = Int(rsp, "nAdr");
                var result = Property(rsp, "result");

                var readings = messageType switch
                {
                    ReadSensorsWithTypes when nodeAddress != null => NodeSensors(nodeAddress.Value, Property(result, "sensors")),
                    SensorFrc => FrcSensors(Property(result, "sensors"), FrcSensorIndex(messageId)),
                    _ => [],
                };

                var bonded = messageType == BondedDevices && Property(result, "bondedDevices") is { ValueKind: JsonValueKind.Array } list
                    ? list.EnumerateArray().Where(n => n.ValueKind == JsonValueKind.Number).Select(n => n.GetInt32()).ToList()
                    : null;

                var assigned = messageType == SmartConnect ? Int(rsp, "assignedAddr") : null;

                var nearby = messageType == NearbyNodes && Property(result, "nodes") is { ValueKind: JsonValueKind.Array } nodes
                    ? nodes.EnumerateArray()
                        .Where(n => String(n, "mid") != null)
                        .Select(n => new IqrfNearbyNode(
                            String(n, "mid")!,
                            Property(n, "sensors") is { ValueKind: JsonValueKind.Array } types ? types.EnumerateArray().Where(t => t.ValueKind == JsonValueKind.Number).Select(t => t.GetInt32()).ToList() : [],
                            Int(n, "rssi")))
                        .ToList()
                    : null;

                return new IqrfResponse(messageType, messageId, nodeAddress, status, statusText, readings, bonded) { AssignedAddress = assigned, NearbyNodes = nearby };
            }
        }

        // Požadavek na hodnoty všech senzorů jednoho uzlu
        public static string ReadSensorsRequest(int nodeAddress, string messageId)
        {
            return JsonSerializer.Serialize(new
            {
                mType = ReadSensorsWithTypes,
                data = new
                {
                    msgId = messageId,
                    req = new { nAdr = nodeAddress, param = new { sensorIndexes = -1 } },
                    returnVerbose = true,
                },
            });
        }

        // Hromadný dotaz FRC: jedna veličina (typ a pořadí senzoru toho typu) ze všech uzlů sítě v jednom rádiovém cyklu.
        // Pořadí senzoru se odpověď nevrací — nese ho id zprávy (…-i{pořadí}-…), aby odpověď šla přiřadit ke kanálu.
        public static string FrcRequest(int sensorType, int sensorIndex, string messageId)
        {
            return JsonSerializer.Serialize(new
            {
                mType = SensorFrc,
                data = new
                {
                    msgId = FrcMessageId(sensorType, sensorIndex, messageId),
                    req = new
                    {
                        nAdr = 0,
                        param = new { sensorType, sensorIndex, frcCommand = FrcCommand(sensorType), getExtraResult = true },
                    },
                    returnVerbose = true,
                },
            });
        }

        // Typy s daty v jednom příkazu FRC (IQRF Standard Sensor): 1–127 = 2 bajty, 128–159 = 1 bajt, 160–191 = 4 bajty.
        // Bloky dat (192+) FRC neumí — ty se čtou po uzlech.
        public static bool SupportsFrc(int sensorType) => sensorType is > 0 and < 192;

        public static int FrcCommand(int sensorType) => sensorType switch
        {
            < 128 => 0xE0,
            < 160 => 0x90,
            _ => 0xF9,
        };

        public static string FrcMessageId(int sensorType, int sensorIndex, string messageId) => $"frc-t{sensorType}-i{sensorIndex}-{messageId}";

        // Pořadí senzoru z id zprávy; neznámá zpráva (dotaz z jiné aplikace) = první senzor typu
        public static int FrcSensorIndex(string? messageId)
        {
            var match = messageId == null ? null : FrcIndexPattern().Match(messageId);
            return match is { Success: true } ? int.Parse(match.Groups[1].Value) : 0;
        }

        // Pořadí senzoru daného typu z klíče kanálu n{uzel}/t{typ}-{pořadí}
        public static int ChannelOrdinal(string channelKey)
        {
            var dash = channelKey.LastIndexOf('-');
            return dash >= 0 && int.TryParse(channelKey[(dash + 1)..], out var ordinal) ? ordinal : 0;
        }

        [GeneratedRegex(@"^frc-t\d+-i(\d+)-")]
        private static partial Regex FrcIndexPattern();

        // Požadavek na seznam spárovaných uzlů (koordinátor má adresu 0)
        public static string BondedDevicesRequest(string messageId)
        {
            return JsonSerializer.Serialize(new
            {
                mType = BondedDevices,
                data = new
                {
                    msgId = messageId,
                    req = new { nAdr = 0, param = new { } },
                    returnVerbose = true,
                },
            });
        }

        // Přidání čidla do sítě podle kódu z nálepky (IQRF SmartConnect); adresa 0 = koordinátor přidělí první volnou
        public static string SmartConnectRequest(string smartConnectCode, string messageId)
        {
            return JsonSerializer.Serialize(new
            {
                mType = SmartConnect,
                data = new
                {
                    msgId = messageId,
                    req = new { deviceAddr = 0, smartConnectCode, bondingTestRetries = 1, userData = new[] { 0, 0, 0, 0 } },
                    returnVerbose = true,
                },
            });
        }

        public static string NearbyNodesRequest(string messageId)
        {
            return JsonSerializer.Serialize(new
            {
                mType = NearbyNodes,
                data = new
                {
                    msgId = messageId,
                    req = new { nAdr = 0, param = new { } },
                    returnVerbose = true,
                },
            });
        }

        public static string ChannelKey(int nodeAddress, int type, int ordinal) => $"n{nodeAddress}/t{type}-{ordinal}";

        private static List<IqrfReading> NodeSensors(int nodeAddress, JsonElement sensors)
        {
            var readings = new List<IqrfReading>();
            if (sensors.ValueKind != JsonValueKind.Array)
                return readings;

            // Uzel může mít víc senzorů stejného typu — pořadí v rámci typu drží klíč stabilní
            var ordinals = new Dictionary<int, int>();
            foreach (var sensor in sensors.EnumerateArray())
            {
                if (Reading(nodeAddress, sensor, ordinals) is { } reading)
                    readings.Add(reading);
            }

            return readings;
        }

        // FRC vrací jeden senzor ze všech uzlů; index v poli je adresa uzlu (0 = koordinátor), uzel bez senzoru = null
        private static List<IqrfReading> FrcSensors(JsonElement sensors, int sensorIndex)
        {
            var readings = new List<IqrfReading>();
            if (sensors.ValueKind != JsonValueKind.Array)
                return readings;

            var index = 0;
            foreach (var sensor in sensors.EnumerateArray())
            {
                var nodeAddress = Int(sensor, "nAdr") ?? index;
                if (nodeAddress > 0 && Reading(nodeAddress, sensor, new Dictionary<int, int>(), sensorIndex) is { } reading)
                    readings.Add(reading);
                index++;
            }

            return readings;
        }

        // fixedOrdinal = pořadí známé z dotazu (FRC); jinak se počítá postupně v rámci uzlu
        private static IqrfReading? Reading(int nodeAddress, JsonElement sensor, Dictionary<int, int> ordinals, int? fixedOrdinal = null)
        {
            if (sensor.ValueKind != JsonValueKind.Object)
                return null;

            var type = Int(sensor, "type");
            if (type == null)
                return null;

            var ordinal = fixedOrdinal ?? ordinals.GetValueOrDefault(type.Value);
            ordinals[type.Value] = ordinal + 1;

            // Senzor bez hodnoty (chyba čtení) — kanál přeskočit, ale pořadí typu už je započítané
            if (!sensor.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Number)
                return null;

            return new IqrfReading(
                ChannelKey(nodeAddress, type.Value, ordinal),
                nodeAddress,
                type.Value,
                String(sensor, "name") ?? String(sensor, "id") ?? $"Typ {type}",
                String(sensor, "unit") ?? string.Empty,
                value.GetDouble());
        }

        private static JsonElement Property(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) ? value : default;
        }

        private static string? String(JsonElement element, string name)
        {
            var value = Property(element, name);
            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }

        private static int? Int(JsonElement element, string name)
        {
            var value = Property(element, name);
            return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : null;
        }
    }
}
