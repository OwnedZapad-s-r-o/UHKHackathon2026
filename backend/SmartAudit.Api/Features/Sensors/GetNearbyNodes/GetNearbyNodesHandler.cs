using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.Sensors.GetNearbyNodes
{
    public record GetNearbyNodesCommand(ViewerContext Viewer, Guid GatewayId) : IRequest<Response<NearbyNodesDto>>;

    // Nálepka emulovaného čidla (MID a kód) — v demu z ní uživatel opíše kód jako z krabičky
    public record NearbyNodeLabelDto(string Mid, string Code);

    public record NearbyNodeDto(string Mid, List<SensorMetric> Metrics, int? Rssi, NearbyNodeLabelDto? Label);

    // Supported = false: gateway seznam čidel v dosahu neumí (skutečný IQRF daemon) nebo neodpověděla — zbývá zadat kód ručně
    public record NearbyNodesDto(bool Supported, List<NearbyNodeDto> Nodes);

    // Čidla v dosahu gatewaye, která ještě nejsou v její síti (rozšíření smartaudit_NearbyNodes emulátoru a virtuálních gatewayí)
    public class GetNearbyNodesHandler : IRequestHandler<GetNearbyNodesCommand, Response<NearbyNodesDto>>
    {
        private static readonly TimeSpan ReplyTimeout = TimeSpan.FromSeconds(4);

        private readonly SmartAuditDbContext _dbContext;
        private readonly IIqrfCommandBus _bus;
        private readonly IqrfSettings _settings;

        public GetNearbyNodesHandler(SmartAuditDbContext dbContext, IIqrfCommandBus bus, IqrfSettings settings)
        {
            _dbContext = dbContext;
            _bus = bus;
            _settings = settings;
        }

        public async Task<Response<NearbyNodesDto>> Handle(GetNearbyNodesCommand request, CancellationToken ct)
        {
            var device = await _dbContext.SensorDevices
                .AsNoTracking()
                .Select(d => new { d.Id, d.GatewayId, d.IsVirtual, d.Facility.OrganizationId })
                .FirstOrDefaultAsync(d => d.Id == request.GatewayId, ct);

            if (device == null)
                return Response.NotFound<NearbyNodesDto>("Gateway s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && device.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<NearbyNodesDto>("K tomuto nemáte oprávnění");

            var messageId = $"sa-{Guid.NewGuid():N}"[..16];
            var message = new OutgoingMessage(
                string.Format(_settings.RequestTopicTemplate, device.GatewayId),
                IqrfResponseParser.NearbyNodesRequest(messageId),
                device.GatewayId,
                IqrfResponseParser.NearbyNodes);

            var reply = await _bus.RequestAsync(message, IqrfCommandBus.MessageKey(messageId), ReplyTimeout, ct);
            var response = reply == null ? null : IqrfResponseParser.Parse(reply);
            if (response is not { IsOk: true, NearbyNodes: { } nearby })
                return Response.Success(new NearbyNodesDto(false, []));

            var catalog = PairableDevices.NearbyOf(device.GatewayId);
            var nodes = nearby
                .Select(n =>
                {
                    var known = catalog.FirstOrDefault(c => string.Equals(c.Mid, n.Mid, StringComparison.OrdinalIgnoreCase));
                    return new NearbyNodeDto(n.Mid, n.SensorTypes.Select(IqrfSensorTypes.MetricOf).ToList(), n.Rssi, known == null ? null : new NearbyNodeLabelDto(known.Mid, known.Code));
                })
                .ToList();

            return Response.Success(new NearbyNodesDto(true, nodes));
        }
    }
}
