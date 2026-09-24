using System.Text.Json;
using System.Text.Json.Nodes;

namespace SmartAudit.Core.Domain.Helpers
{
    // Odpovědi virtuální gatewaye ve stejném tvaru jako IQRF Gateway Daemon (IQRF JSON API) —
    // backend je zpracuje úplně stejně jako odpověď skutečného hardwaru.
    public static class VirtualGatewayResponder
    {
        // Podle id instance se pozná, že odpověď poslala aplikace sama (a ne externí emulátor nebo hardware)
        public const string InstanceId = "smartaudit-virtual";

        public static string? Respond(string gatewayId, string requestJson, DateTimeOffset now)
        {
            JsonNode? request;
            try
            {
                request = JsonNode.Parse(requestJson);
            }
            catch (JsonException)
            {
                return null;
            }

            var messageType = request?["mType"]?.GetValue<string>();
            var data = request?["data"];
            var messageId = data?["msgId"]?.GetValue<string>() ?? string.Empty;
            var req = data?["req"];
            var address = req?["nAdr"]?.GetValue<int>() ?? 0;

            var nodes = VirtualGateways.NodesOf(gatewayId);
            var profile = VirtualGateways.ProfileOf(gatewayId);

            switch (messageType)
            {
                case IqrfResponseParser.BondedDevices:
                    return Envelope(messageType, messageId, 0, new JsonObject { ["bondedDevices"] = new JsonArray(nodes.Select(n => (JsonNode)n.Address).ToArray()) });

                case IqrfResponseParser.ReadSensorsWithTypes:
                    var node = nodes.FirstOrDefault(n => n.Address == address);
                    if (node == null)
                        return Envelope(messageType, messageId, address, null, 1002, "Timeout — uzel neodpověděl");

                    var sensors = new JsonArray(node.Sensors.Select(s => (JsonNode)Sensor(gatewayId, profile, node.Address, s, now)).ToArray());
                    return Envelope(messageType, messageId, address, new JsonObject { ["sensors"] = sensors });

                case IqrfResponseParser.SensorFrc:
                    // Jedna veličina ze všech uzlů: pole indexované adresou uzlu (0 = koordinátor), uzel bez senzoru = null
                    var type = req?["param"]?["sensorType"]?.GetValue<int>() ?? 0;
                    var index = req?["param"]?["sensorIndex"]?.GetValue<int>() ?? 0;
                    var frc = new JsonArray();
                    for (var a = 0; a <= nodes.Max(n => n.Address); a++)
                    {
                        var sensor = nodes.FirstOrDefault(n => n.Address == a)?.Sensors.Where(s => s.Type == type).ElementAtOrDefault(index);
                        frc.Add(sensor == null ? null : Sensor(gatewayId, profile, a, sensor, now));
                    }

                    return Envelope(messageType, messageId, 0, new JsonObject { ["sensors"] = frc });

                default:
                    return messageType == null ? null : Envelope(messageType, messageId, address, null, -1, "Virtuální gateway tento typ zprávy nepodporuje");
            }
        }

        private static JsonObject Sensor(string gatewayId, VirtualProfile profile, int address, VirtualSensor sensor, DateTimeOffset now)
        {
            return new JsonObject
            {
                ["id"] = sensor.Id,
                ["type"] = sensor.Type,
                ["name"] = sensor.Name,
                ["shortName"] = sensor.Id[..3],
                ["value"] = VirtualGateways.Value(gatewayId, profile, address, sensor.Type, now),
                ["unit"] = sensor.Unit,
                ["decimalPlaces"] = 2,
            };
        }

        private static string Envelope(string messageType, string messageId, int address, JsonObject? result, int status = 0, string statusText = "ok")
        {
            var rsp = new JsonObject { ["nAdr"] = address, ["hwpId"] = 0, ["rCode"] = status == 0 ? 0 : 1, ["dpaVal"] = 64 };
            if (result != null)
                rsp["result"] = result;

            return new JsonObject
            {
                ["mType"] = messageType,
                ["data"] = new JsonObject
                {
                    ["msgId"] = messageId,
                    ["rsp"] = rsp,
                    ["insId"] = InstanceId,
                    ["status"] = status,
                    ["statusStr"] = statusText,
                },
            }.ToJsonString();
        }
    }
}
