using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Sensors
{
    // Předá zařízení z demo správy do katalogu PairableDevices, odkud je hrají virtuální gatewaye.
    // Volá se po každé změně v demo správě a pravidelně z VirtualGatewayService (po restartu aplikace).
    public static class DemoDeviceCatalog
    {
        public static async Task ReloadAsync(SmartAuditDbContext dbContext, CancellationToken ct)
        {
            var devices = await dbContext.DemoDevices.AsNoTracking().ToListAsync(ct);
            Apply(devices);
        }

        public static void Apply(IReadOnlyCollection<DemoDevice> devices)
        {
            var gateways = devices
                .Where(d => d.Kind == DemoDeviceKind.Gateway)
                .Select(d => new PairableGateway(d.Serial, d.Model, d.Code, d.Templates.Select((t, i) => DemoNodeTemplates.Node(t, i + 1, d.Behavior)).ToList()))
                .ToList();

            var nearby = devices
                .Where(d => d.Kind == DemoDeviceKind.Node && d.ParentGatewayId != null && d.NodeAddress != null)
                .GroupBy(d => d.ParentGatewayId!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(d => d.NodeAddress).Select(d => new NearbyNode(d.Serial, d.Code, DemoNodeTemplates.Node(d.Templates[0], d.NodeAddress!.Value, d.Behavior))).ToArray(),
                    StringComparer.OrdinalIgnoreCase);

            PairableDevices.UseDemoDevices(gateways, nearby);
        }
    }
}
