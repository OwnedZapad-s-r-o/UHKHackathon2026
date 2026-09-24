using SmartAudit.Api.Features.Sensors.GetGateways;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Sensors
{
    public static class GatewayStatus
    {
        // Online = gateway odpověděla během posledních dvou intervalů dotazování (alespoň minuta)
        public static bool IsOnline(DateTimeOffset? lastSeenAt, int pollSeconds, DateTimeOffset now)
        {
            return lastSeenAt != null && now - lastSeenAt.Value < TimeSpan.FromSeconds(Math.Max(60, pollSeconds * 2));
        }

        public static BrokerDto Broker(IIqrfGatewayState state, IqrfSettings settings)
        {
            var caAvailable = !string.IsNullOrEmpty(settings.CaCertificatePath) && File.Exists(settings.CaCertificatePath);

            return new BrokerDto(state.BrokerConnected, state.BrokerError, settings.PublicHost, settings.PublicPort, settings.PublicTls, caAvailable, settings.ResponseTopicFilter, settings.RequestTopicTemplate);
        }
    }
}
