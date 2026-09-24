using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    public class GatewayCredentialsTests
    {
        // Stejný výpočet má emulátor v Pythonu (tools/iqrf-emulator/emulator.py, gateway_password) — hodnota spočítaná jím
        [Fact]
        public void Password_matches_the_emulator_derivation()
        {
            var credentials = GatewayCredentials.For("test-secret-for-unit-tests-0123456789", "skola-gw01");

            Assert.Equal("skola-gw01", credentials.Username);
            Assert.Equal("oMDpUAjQltcn7nrhIGs-2mAz", credentials.Password);
        }

        [Fact]
        public void Each_gateway_gets_a_different_password()
        {
            var first = GatewayCredentials.For("test-secret-for-unit-tests-0123456789", "skola-gw01");
            var second = GatewayCredentials.For("test-secret-for-unit-tests-0123456789", "skola-gw02");

            Assert.NotEqual(first.Password, second.Password);
        }
    }
}
