using System.Text.Json;

namespace SmartAudit.Core.Domain.Helpers
{
    public record IqrfReading(string ChannelKey, int NodeAddress, int Type, string Name, string Unit, double Value);

    public record IqrfResponse(string MessageType, string? MessageId, int? NodeAddress, int Status, string? StatusText, IReadOnlyList<IqrfReading> Readings, IReadOnlyList<int>? BondedNodes)
    {
        public bool IsOk => Status == 0;
    }

    // Odpovědi IQRF Gateway Daemonu (IQRF JSON API) → hodnoty senzorů.
    // Podporuje iqrfSensor_ReadSensorsWithTypes (jeden uzel, všechny senzory), iqrfSensor_Frc (jeden typ, všechny uzly)
    // a iqrfEmbedCoordinator_BondedDevices (seznam spárovaných uzlů pro dotazování).
    public static class IqrfResponseParser
    {
        public const string ReadSensorsWithTypes = "iqrfSensor_ReadSensorsWithTypes";
        public const string SensorFrc = "iqrfSensor_Frc";
        public const string BondedDevices = "iqrfEmbedCoordinator_BondedDevices";

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
                    SensorFrc => FrcSensors(Property(result, "sensors")),
                    _ => [],
                };

                var bonded = messageType == BondedDevices && Property(result, "bondedDevices") is { ValueKind: JsonValueKind.Array } list
                    ? list.EnumerateArray().Where(n => n.ValueKind == JsonValueKind.Number).Select(n => n.GetInt32()).ToList()
                    : null;

                return new IqrfResponse(messageType, messageId, nodeAddress, status, statusText, readings, bonded);
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

        // FRC vrací jeden senzor ze všech uzlů; index v poli je adresa uzlu (0 = koordinátor)
        private static List<IqrfReading> FrcSensors(JsonElement sensors)
        {
            var readings = new List<IqrfReading>();
            if (sensors.ValueKind != JsonValueKind.Array)
                return readings;

            var index = 0;
            foreach (var sensor in sensors.EnumerateArray())
            {
                var nodeAddress = Int(sensor, "nAdr") ?? index;
                if (nodeAddress > 0 && Reading(nodeAddress, sensor, new Dictionary<int, int>()) is { } reading)
                    readings.Add(reading);
                index++;
            }

            return readings;
        }

        private static IqrfReading? Reading(int nodeAddress, JsonElement sensor, Dictionary<int, int> ordinals)
        {
            if (sensor.ValueKind != JsonValueKind.Object)
                return null;

            var type = Int(sensor, "type");
            if (type == null)
                return null;

            var ordinal = ordinals.GetValueOrDefault(type.Value);
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
