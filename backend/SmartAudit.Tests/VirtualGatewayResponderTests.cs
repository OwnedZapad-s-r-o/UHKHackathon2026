using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Virtuální gateway musí mluvit přesně jako IQRF Gateway Daemon — odpovědi jdou stejným parserem jako od hardwaru
    public class VirtualGatewayResponderTests
    {
        private static readonly DateTimeOffset Now = new(2026, 11, 10, 9, 20, 0, TimeSpan.Zero);

        [Fact]
        public void Bonded_devices_lists_the_preset_nodes()
        {
            var response = Respond("gw-hk-tyl", IqrfResponseParser.BondedDevicesRequest("m1"));

            Assert.Equal([1, 2], response.BondedNodes);
        }

        [Fact]
        public void Node_read_returns_the_sensors_of_the_node()
        {
            var response = Respond("gw-hk-tyl", IqrfResponseParser.ReadSensorsRequest(1, "m2"));

            Assert.True(response.IsOk);
            Assert.Equal(["n1/t1-0", "n1/t2-0", "n1/t128-0"], response.Readings.Select(r => r.ChannelKey));
        }

        [Fact]
        public void Frc_returns_the_quantity_from_every_node_that_has_it()
        {
            var response = Respond("gw-hk-tyl", IqrfResponseParser.FrcRequest(sensorType: 1, sensorIndex: 0, "m3"));

            // Teplotu mají oba uzly, klíče sedí s kanály, které založí testovací síť
            Assert.Equal(["n1/t1-0", "n2/t1-0"], response.Readings.Select(r => r.ChannelKey));
            Assert.All(response.Readings, r => Assert.InRange(r.Value, 15, 26));
        }

        [Fact]
        public void Unknown_node_times_out_like_real_hardware()
        {
            var response = Respond("gw-hk-tyl", IqrfResponseParser.ReadSensorsRequest(7, "m4"));

            Assert.False(response.IsOk);
            Assert.Empty(response.Readings);
        }

        [Fact]
        public void Response_is_marked_as_virtual()
        {
            var json = VirtualGatewayResponder.Respond("gw-hk-tyl", IqrfResponseParser.BondedDevicesRequest("m5"), Now)!;

            Assert.Contains(VirtualGatewayResponder.InstanceId, json);
        }

        private static IqrfResponse Respond(string gatewayId, string request)
        {
            var json = VirtualGatewayResponder.Respond(gatewayId, request, Now);
            Assert.NotNull(json);
            return IqrfResponseParser.Parse(json)!;
        }
    }
}
