using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.Sensors.GetGateways
{
    public record GetGatewaysCommand(ViewerContext Viewer) : IRequest<Response<GetGatewaysDto>>;

    public record GetGatewaysDto(BrokerDto Broker, IReadOnlyCollection<GatewayListItemDto> Gateways, IReadOnlyCollection<UnknownGatewayDto> Unknown);

    // PublicTls = gatewaye se připojují šifrovaně; CaAvailable = certifikát CA brokeru jde stáhnout
    public record BrokerDto(bool Connected, string? Error, string? PublicHost, int PublicPort, bool PublicTls, bool CaAvailable, string ResponseTopicFilter, string RequestTopicTemplate);

    public record GatewayListItemDto(Guid Id, string Name, string GatewayId, Guid FacilityId, string FacilityCode, string FacilityName, bool Online, DateTimeOffset? LastSeenAt, int NodeCount, int ChannelCount, int AssignedCount, bool IsVirtual);

    public record UnknownGatewayDto(string GatewayId, DateTimeOffset LastSeenAt, int MessageCount);

    // Obrazovka Senzory — připojené IQRF gatewaye a gatewaye, které se ozývají, ale v aplikaci ještě nejsou
    public class GetGatewaysHandler : IRequestHandler<GetGatewaysCommand, Response<GetGatewaysDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IIqrfGatewayState _state;
        private readonly IqrfSettings _settings;

        public GetGatewaysHandler(SmartAuditDbContext dbContext, IIqrfGatewayState state, IqrfSettings settings)
        {
            _dbContext = dbContext;
            _state = state;
            _settings = settings;
        }

        public async Task<Response<GetGatewaysDto>> Handle(GetGatewaysCommand request, CancellationToken ct)
        {
            var query = _dbContext.SensorDevices.AsNoTracking();

            // Správce budovy vidí jen gatewaye objektů své organizace
            if (request.Viewer.Role == AppRoles.Manager)
                query = query.Where(d => d.Facility.OrganizationId == request.Viewer.OrganizationId);

            var devices = await query
                .OrderBy(d => d.Name)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.GatewayId,
                    d.FacilityId,
                    FacilityCode = d.Facility.Code,
                    FacilityName = d.Facility.Name,
                    d.LastSeenAt,
                    d.PollSeconds,
                    NodeCount = d.NodeAddresses.Length,
                    ChannelCount = d.Channels.Count,
                    AssignedCount = d.Channels.Count(c => c.SensorId != null),
                    d.IsVirtual,
                })
                .ToListAsync(ct);

            var now = DateTimeOffset.UtcNow;
            var gateways = devices
                .Select(d => new GatewayListItemDto(
                    d.Id,
                    d.Name,
                    d.GatewayId,
                    d.FacilityId,
                    d.FacilityCode,
                    d.FacilityName,
                    GatewayStatus.IsOnline(d.LastSeenAt, d.PollSeconds, now),
                    d.LastSeenAt,
                    d.NodeCount,
                    d.ChannelCount,
                    d.AssignedCount,
                    d.IsVirtual))
                .ToList();

            // Neznámé gatewaye ukážeme jen kraji — správce by viděl i cizí zařízení v síti
            var unknown = request.Viewer.Role == AppRoles.Region
                ? _state.GetUnknown().Select(u => new UnknownGatewayDto(u.GatewayId, u.LastSeenAt, u.MessageCount)).ToList()
                : [];

            return Response.Success(new GetGatewaysDto(GatewayStatus.Broker(_state, _settings), gateways, unknown));
        }
    }
}
