using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Zařízení z demo správy se musí párovat a měřit stejně jako pevný katalog — hraje je stejná virtuální gateway
    public class DemoDevicesTests
    {
        // Středa 2 h ráno místního času — provozní den, noc, kdy má běžet útlum
        private static readonly DateTimeOffset Night = new(2026, 11, 11, 1, 0, 0, TimeSpan.Zero);

        [Fact]
        public void Demo_gateway_is_pairable_and_answers_with_its_own_nodes()
        {
            var gateway = new PairableGateway("A1B2C3D4", DemoNodeTemplates.GatewayModel, "ABCD-EFGH",
                [DemoNodeTemplates.Node("kancelar", 1, DemoBehavior.Setback), DemoNodeTemplates.Node("co2", 2, DemoBehavior.Setback)]);
            PairableDevices.UseDemoDevices([gateway], new Dictionary<string, NearbyNode[]>());

            Assert.Same(gateway, PairableDevices.FindGateway("a1b2c3d4"));
            Assert.Contains(PairableDevices.All, g => g.Serial == "A1B2C3D4");

            var response = IqrfResponseParser.Parse(VirtualGatewayResponder.Respond(gateway.GatewayId, IqrfResponseParser.BondedDevicesRequest("d1"), Night)!)!;
            Assert.Equal([1, 2], response.BondedNodes);
        }

        [Fact]
        public void Demo_node_is_in_range_and_bonds_with_the_code_from_its_label()
        {
            const string gatewayId = "virtualni-demo-test";
            var node = new NearbyNode("5C0EAAAA", "WXYZ-2345", DemoNodeTemplates.Node("ucebna", 4, DemoBehavior.Overheating));
            PairableDevices.UseDemoDevices([], new Dictionary<string, NearbyNode[]> { [gatewayId] = [node] });

            // Virtuální učebna má své čidlo na adrese 3 a k němu čidlo z demo správy
            Assert.Equal([3, 4], PairableDevices.NearbyOf(gatewayId).Select(n => n.Node.Address));

            var response = IqrfResponseParser.Parse(VirtualGatewayResponder.Respond(gatewayId, IqrfResponseParser.SmartConnectRequest("wxyz 2345", "d2"), Night)!)!;
            Assert.Equal(4, response.AssignedAddress);
        }

        [Fact]
        public void Overheating_node_keeps_comfort_temperature_at_night()
        {
            var setback = VirtualGateways.Value("gw-demo", VirtualProfile.School, 1, VirtualGateways.Temperature.Type, Night);
            var overheating = VirtualGateways.Value("gw-demo", VirtualProfile.School, 1, VirtualGateways.Temperature.Type, Night, overheats: true);

            Assert.InRange(setback, 17, 19.5);
            Assert.True(overheating > 21, $"Přetápějící místnost má v noci {overheating} °C");
        }

        [Fact]
        public void New_codes_look_like_label_codes()
        {
            var code = DemoNodeTemplates.NewCode();

            Assert.Matches("^[A-Z2-9]{4}-[A-Z2-9]{4}$", code);
            Assert.True(PairingProtocol.IsValidSerial(DemoNodeTemplates.NewSerial()));
        }
    }
}
