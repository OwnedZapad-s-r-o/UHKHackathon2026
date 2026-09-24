using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Odpovědi IQRF Gateway Daemonu (IQRF JSON API) — klíč kanálu musí být stejný pro čtení po uzlech i pro hromadné FRC,
    // jinak by FRC zakládalo duplicitní kanály a přiřazení do místností by se ztratilo
    public class IqrfResponseParserTests
    {
        [Fact]
        public void Node_read_numbers_sensors_of_the_same_type()
        {
            const string json = """
                {"mType":"iqrfSensor_ReadSensorsWithTypes","data":{"msgId":"sa-1","status":0,"rsp":{"nAdr":3,"result":{"sensors":[
                  {"id":"TEMPERATURE","type":1,"name":"Temperature","value":21.5,"unit":"°C"},
                  {"id":"TEMPERATURE","type":1,"name":"Temperature","value":19.0,"unit":"°C"},
                  {"id":"CARBON_DIOXIDE","type":2,"name":"Carbon dioxide","value":820,"unit":"ppm"}]}}}}
                """;

            var response = IqrfResponseParser.Parse(json)!;

            Assert.Equal(["n3/t1-0", "n3/t1-1", "n3/t2-0"], response.Readings.Select(r => r.ChannelKey));
        }

        [Fact]
        public void Frc_response_maps_array_index_to_node_and_message_id_to_sensor_ordinal()
        {
            var messageId = IqrfResponseParser.FrcMessageId(sensorType: 1, sensorIndex: 1, "sa-abc");
            var json = """
                {"mType":"iqrfSensor_Frc","data":{"msgId":"MESSAGE_ID","status":0,"rsp":{"nAdr":0,"result":{"sensors":[
                  null,
                  {"id":"TEMPERATURE","type":1,"name":"Temperature","value":22.1,"unit":"°C"},
                  null,
                  {"id":"TEMPERATURE","type":1,"name":"Temperature","value":20.4,"unit":"°C"}]}}}}
                """.Replace("MESSAGE_ID", messageId);

            var response = IqrfResponseParser.Parse(json)!;

            Assert.Equal(["n1/t1-1", "n3/t1-1"], response.Readings.Select(r => r.ChannelKey));
            Assert.Equal([22.1, 20.4], response.Readings.Select(r => r.Value));
        }

        [Fact]
        public void Frc_request_carries_the_sensor_ordinal_in_the_message_id()
        {
            var request = IqrfResponseParser.FrcRequest(sensorType: 2, sensorIndex: 0, "sa-xyz");

            Assert.Contains("\"mType\":\"iqrfSensor_Frc\"", request);
            Assert.Contains("\"frcCommand\":224", request);
            Assert.Equal(0, IqrfResponseParser.FrcSensorIndex(IqrfResponseParser.FrcMessageId(2, 0, "sa-xyz")));
            Assert.Equal(2, IqrfResponseParser.FrcSensorIndex(IqrfResponseParser.FrcMessageId(1, 2, "sa-xyz")));
        }

        [Theory]
        [InlineData(1, 0xE0)]    // teplota, 2 bajty
        [InlineData(128, 0x90)]  // vlhkost, 1 bajt
        [InlineData(160, 0xF9)]  // 4 bajty
        public void Frc_command_follows_the_value_width(int sensorType, int frcCommand)
        {
            Assert.Equal(frcCommand, IqrfResponseParser.FrcCommand(sensorType));
        }

        [Fact]
        public void Channel_ordinal_is_read_from_the_channel_key()
        {
            Assert.Equal(1, IqrfResponseParser.ChannelOrdinal(IqrfResponseParser.ChannelKey(5, 1, 1)));
        }
    }
}
