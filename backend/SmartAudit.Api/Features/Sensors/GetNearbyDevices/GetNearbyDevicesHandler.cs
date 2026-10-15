using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Sensors.GetNearbyDevices
{
    public record GetNearbyDevicesCommand(ViewerContext Viewer) : IRequest<Response<List<NearbyDeviceDto>>>;

    public record NearbyDeviceNodeDto(int Address, List<SensorMetric> Metrics);

    // Nálepka emulovaného zařízení — v demu si z ní uživatel „přečte“ heslo jako z krabičky
    public record DeviceLabelDto(string Serial, string Model, string Password);

    public record NearbyDeviceDto(string Serial, string Model, List<NearbyDeviceNodeDto> Nodes, int? Rssi, bool Emulated, DateTimeOffset LastSeenAt, DeviceLabelDto? Label);

    // Nové gatewaye online, které čekají na spárování. Zatím nikomu nepatří, proto je vidí kraj i každý správce —
    // připojit si je může jen ten, kdo zná heslo z nálepky.
    public class GetNearbyDevicesHandler : IRequestHandler<GetNearbyDevicesCommand, Response<List<NearbyDeviceDto>>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IPairingRegistry _registry;

        public GetNearbyDevicesHandler(SmartAuditDbContext dbContext, IPairingRegistry registry)
        {
            _dbContext = dbContext;
            _registry = registry;
        }

        public async Task<Response<List<NearbyDeviceDto>>> Handle(GetNearbyDevicesCommand request, CancellationToken ct)
        {
            var online = _registry.Online(DateTimeOffset.UtcNow);
            if (online.Count == 0)
                return Response.Success(new List<NearbyDeviceDto>());

            // Už připojená gateway se může chvíli dál ohlašovat (emulátor po restartu) — v seznamu ji neukazovat
            var gatewayIds = online.Select(g => PairableDevices.GatewayIdOf(g.Announcement.Serial)).ToList();
            var claimed = await _dbContext.SensorDevices
                .AsNoTracking()
                .Where(d => gatewayIds.Contains(d.GatewayId))
                .Select(d => d.GatewayId)
                .ToListAsync(ct);

            var devices = online
                .Where(g => !claimed.Contains(PairableDevices.GatewayIdOf(g.Announcement.Serial)))
                .Select(g => new NearbyDeviceDto(
                    g.Announcement.Serial,
                    g.Announcement.Model,
                    g.Announcement.Nodes.Select(n => new NearbyDeviceNodeDto(n.Address, n.SensorTypes.Select(IqrfSensorTypes.MetricOf).ToList())).ToList(),
                    g.Announcement.Rssi,
                    g.Announcement.Emulated,
                    g.LastSeenAt,
                    Label(g.Announcement)))
                .ToList();

            return Response.Success(devices);
        }

        private static DeviceLabelDto? Label(PairingAnnouncement announcement)
        {
            var catalog = announcement.Emulated ? PairableDevices.FindGateway(announcement.Serial) : null;
            return catalog == null ? null : new DeviceLabelDto(catalog.Serial, catalog.Model, catalog.Password);
        }
    }
}
