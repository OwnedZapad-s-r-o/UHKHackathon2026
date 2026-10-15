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

        // bonded = adresy uzlů, které jsou v síti gatewaye (včetně čidel přidaných přes SmartConnect)
        public static string? Respond(string gatewayId, string requestJson, DateTimeOffset now, IReadOnlyCollection<int>? bonded = null)
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

            var nodes = VirtualGateways.NodesOf(gatewayId, bonded);
            var profile = VirtualGateways.ProfileOf(gatewayId);

            switch (messageType)
            {
                case IqrfResponseParser.BondedDevices:
                    return Envelope(messageType, messageId, 0, new JsonObject { ["bondedDevices"] = new JsonArray(nodes.Select(n => (JsonNode)n.Address).ToArray()) });

                case IqrfResponseParser.ReadSensorsWithTypes:
                    var node = nodes.FirstOrDefault(n => n.Address == address);
                    if (node == null)
                        return Envelope(messageType, messageId, address, null, 1002, "Timeout — uzel neodpověděl");

                    var sensors = new JsonArray(node.Sensors.Select(s => (JsonNode)Sensor(gatewayId, profile, node, s, now)).ToArray());
                    return Envelope(messageType, messageId, address, new JsonObject { ["sensors"] = sensors });

                case IqrfResponseParser.SensorFrc:
                    // Jedna veličina ze všech uzlů: pole indexované adresou uzlu (0 = koordinátor), uzel bez senzoru = null
                    var type = req?["param"]?["sensorType"]?.GetValue<int>() ?? 0;
                    var index = req?["param"]?["sensorIndex"]?.GetValue<int>() ?? 0;
                    var frc = new JsonArray();
                    for (var a = 0; a <= nodes.Max(n => n.Address); a++)
                    {
                        var frcNode = nodes.FirstOrDefault(n => n.Address == a);
                        var sensor = frcNode?.Sensors.Where(s => s.Type == type).ElementAtOrDefault(index);
                        frc.Add(sensor == null ? null : Sensor(gatewayId, profile, frcNode!, sensor, now));
                    }

                    return Envelope(messageType, messageId, 0, new JsonObject { ["sensors"] = frc });

                case IqrfResponseParser.SmartConnect:
                    // Kód z nálepky čidla v dosahu → čidlo dostane adresu v síti; už přidané čidlo odpoví stejnou adresou
                    var code = req?["smartConnectCode"]?.GetValue<string>();
                    var candidate = PairableDevices.NearbyOf(gatewayId).FirstOrDefault(n => PairingCode.Matches(n.Code, code));
                    if (candidate == null)
                        return Envelope(messageType, messageId, 0, null, 1003, "Bonding se nezdařil — čidlo s tímto kódem neodpovědělo");

                    var connected = Envelope(messageType, messageId, 0, null);
                    var json = JsonNode.Parse(connected)!;
                    var rsp = json["data"]!["rsp"]!.AsObject();
                    rsp["assignedAddr"] = candidate.Node.Address;
                    rsp["nodesNr"] = nodes.Count + (nodes.Any(n => n.Address == candidate.Node.Address) ? 0 : 1);
                    return json.ToJsonString();

                case IqrfResponseParser.NearbyNodes:
                    var nearby = PairableDevices.NearbyOf(gatewayId)
                        .Where(n => nodes.All(b => b.Address != n.Node.Address))
                        .Select(n => (JsonNode)new JsonObject
                        {
                            ["mid"] = n.Mid,
                            ["sensors"] = new JsonArray(n.Node.Sensors.Select(s => (JsonNode)s.Type).ToArray()),
                            ["rssi"] = -60 - Math.Abs(StableHash.Of(n.Mid) % 3) * 7,
                        })
                        .ToArray();
                    return Envelope(messageType, messageId, 0, new JsonObject { ["nodes"] = new JsonArray(nearby) });

                default:
                    return messageType == null ? null : Envelope(messageType, messageId, address, null, -1, "Virtuální gateway tento typ zprávy nepodporuje");
            }
        }

        private static JsonObject Sensor(string gatewayId, VirtualProfile profile, VirtualNode node, VirtualSensor sensor, DateTimeOffset now)
        {
            return new JsonObject
            {
                ["id"] = sensor.Id,
                ["type"] = sensor.Type,
                ["name"] = sensor.Name,
                ["shortName"] = sensor.Id[..3],
                ["value"] = VirtualGateways.Value(gatewayId, profile, node.Address, sensor.Type, now, node.Overheats),
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
