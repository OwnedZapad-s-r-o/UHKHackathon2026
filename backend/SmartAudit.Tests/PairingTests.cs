using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Párování heslem z nálepky: nabídka jde otevřít jen správným heslem a jen pro svou gatewayi
    public class PairingTests
    {
        private static readonly MqttCredentials Credentials = new("iqrf-81a4f2c0", "heslo-k-brokeru-0123456789");

        [Fact]
        public void Offer_opens_with_the_password_from_the_label()
        {
            var offer = PairingProtocol.SealOffer("7Q4M-2PXA", "81A4F2C0", Credentials);

            Assert.Equal(Credentials, PairingProtocol.OpenOffer("7Q4M-2PXA", "81A4F2C0", offer));
        }

        [Fact]
        public void Password_can_be_typed_in_lower_case_without_the_dash()
        {
            var offer = PairingProtocol.SealOffer("7Q4M-2PXA", "81A4F2C0", Credentials);

            Assert.Equal(Credentials, PairingProtocol.OpenOffer("7q4m 2pxa", "81A4F2C0", offer));
        }

        [Fact]
        public void Wrong_password_does_not_open_the_offer()
        {
            var offer = PairingProtocol.SealOffer("7Q4M-2PXB", "81A4F2C0", Credentials);

            Assert.Null(PairingProtocol.OpenOffer("7Q4M-2PXA", "81A4F2C0", offer));
        }

        [Fact]
        public void Offer_for_another_gateway_does_not_open()
        {
            var offer = PairingProtocol.SealOffer("7Q4M-2PXA", "81A4F3D9", Credentials);

            Assert.Null(PairingProtocol.OpenOffer("7Q4M-2PXA", "81A4F2C0", offer));
        }

        // Nabídka zašifrovaná Python emulátorem (tools/iqrf-emulator/pairing.py, seal_offer s pevným nonce) — oba musí mluvit stejně
        [Fact]
        public void Offer_sealed_by_the_python_emulator_opens()
        {
            const string offer = PythonSealedOffer;

            Assert.Equal(Credentials, PairingProtocol.OpenOffer("7Q4M-2PXA", "81A4F2C0", offer));
        }

        [Fact]
        public void Announcement_round_trips_without_secrets()
        {
            var gateway = PairableDevices.FindGateway("81a4f2c0")!;
            var json = PairingProtocol.Announcement(gateway, -58, new DateTimeOffset(2026, 10, 15, 9, 0, 0, TimeSpan.Zero));

            var announcement = PairingProtocol.ParseAnnouncement(json)!;

            Assert.Equal("81A4F2C0", announcement.Serial);
            Assert.Equal([1, 2], announcement.Nodes.Select(n => n.Address));
            Assert.True(announcement.Emulated);
            Assert.DoesNotContain(PairingCode.Normalize(gateway.Password), json);
        }

        [Fact]
        public void Topics_carry_the_serial()
        {
            Assert.Equal("81A4F2C0", PairingProtocol.AnnouncedSerial("provisioning/announce/81a4f2c0"));
            Assert.Equal("81A4F2C0", PairingProtocol.ResultSerial("provisioning/81A4F2C0/result"));
            Assert.Null(PairingProtocol.ResultSerial("provisioning/../result"));
            Assert.Null(PairingProtocol.AnnouncedSerial("gateway/81A4F2C0/iqrf/responses"));
        }

        [Fact]
        public void Nearby_sensor_joins_the_network_with_the_code_from_its_label()
        {
            var response = Respond("gw-rk-sps", IqrfResponseParser.SmartConnectRequest("rk2b x8qd", "m1"), bonded: [1, 2]);

            Assert.True(response.IsOk);
            Assert.Equal(3, response.AssignedAddress);
        }

        [Fact]
        public void Wrong_code_fails_like_a_failed_bonding()
        {
            var response = Respond("gw-rk-sps", IqrfResponseParser.SmartConnectRequest("RK2B-X8QE", "m2"), bonded: [1, 2]);

            Assert.False(response.IsOk);
            Assert.Null(response.AssignedAddress);
        }

        [Fact]
        public void Bonded_sensor_is_in_the_network_and_no_longer_nearby()
        {
            var before = Respond("gw-rk-sps", IqrfResponseParser.NearbyNodesRequest("m3"), bonded: [1, 2]);
            var after = Respond("gw-rk-sps", IqrfResponseParser.NearbyNodesRequest("m4"), bonded: [1, 2, 3]);
            var bondedList = Respond("gw-rk-sps", IqrfResponseParser.BondedDevicesRequest("m5"), bonded: [1, 2, 3]);
            var read = Respond("gw-rk-sps", IqrfResponseParser.ReadSensorsRequest(3, "m6"), bonded: [1, 2, 3]);

            Assert.Equal(["5C0E91A7"], before.NearbyNodes!.Select(n => n.Mid));
            Assert.Empty(after.NearbyNodes!);
            Assert.Equal([1, 2, 3], bondedList.BondedNodes);
            Assert.Equal(["n3/t2-0", "n3/t1-0"], read.Readings.Select(r => r.ChannelKey));
        }

        [Fact]
        public void Paired_factory_gateway_answers_with_its_own_nodes()
        {
            var response = Respond("iqrf-81a4f3d9", IqrfResponseParser.ReadSensorsRequest(1, "m7"));

            Assert.Equal(["n1/t1-0", "n1/t128-0", "n1/t11-0"], response.Readings.Select(r => r.ChannelKey));
        }

        [Fact]
        public void Demo_classroom_has_a_nearby_sensor_with_a_stable_code()
        {
            var first = PairableDevices.NearbyOf("virtualni-sko-5205-12-abcd");
            var again = PairableDevices.NearbyOf("virtualni-sko-5205-12-abcd");

            Assert.Single(first);
            Assert.Equal(first[0].Code, again[0].Code);
            Assert.Matches("^[A-Z2-9]{4}-[A-Z2-9]{4}$", first[0].Code);
        }

        private static IqrfResponse Respond(string gatewayId, string request, int[]? bonded = null)
        {
            var json = VirtualGatewayResponder.Respond(gatewayId, request, new DateTimeOffset(2026, 11, 10, 9, 20, 0, TimeSpan.Zero), bonded)!;
            return IqrfResponseParser.Parse(json)!;
        }

        private const string PythonSealedOffer = """{"v": 1, "nonce": "AAECAwQFBgcICQoL", "data": "AQ62km4qhVZjL/UF+c0rtz3mAOc2id2NvEzdiT3hFW0oi1cCBGghx9Jmcfphf8I1kp7/D1WWJ7IjFl90iXVbMNQjswI7gv1R7LbR38FxvajpiR8ufvQt7w+q+xk9CBCJ5fqGH6uEYnZmaCU8myJikw=="}""";
    }
}
