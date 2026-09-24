using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Sensors.GetGateways;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.Sensors.GetGateway
{
    public record GetGatewayCommand(Guid Id, ViewerContext Viewer) : IRequest<Response<GetGatewayDto>>;

    public record GetGatewayDto(
        Guid Id,
        string Name,
        string GatewayId,
        int PollSeconds,
        // Virtuální gateway (testovací síť, demo) — odpovídá za ni aplikace, ne hardware
        bool IsVirtual,
        IReadOnlyCollection<int> Nodes,
        bool Online,
        DateTimeOffset? LastSeenAt,
        GatewayFacilityDto Facility,
        BrokerDto Broker,
        // Jméno a heslo gatewaye k brokeru — vidí je jen kraj a správce této budovy (kontrola níže)
        MqttCredentials? Credentials,
        IReadOnlyCollection<ChannelDto> Channels,
        IReadOnlyCollection<ZoneOptionDto> Zones,
        IReadOnlyCollection<GatewayMessageDto> Messages);

    public record GatewayFacilityDto(Guid Id, string Code, string Name);

    public record ChannelDto(
        Guid Id,
        string ChannelKey,
        int NodeAddress,
        int IqrfType,
        string Name,
        string Unit,
        SensorMetric Metric,
        double? LastValue,
        DateTimeOffset? LastSeenAt,
        Guid? SensorId,
        Guid? ZoneId,
        string? ZoneName,
        IReadOnlyCollection<ChannelSampleDto> Samples);

    public record ChannelSampleDto(DateTimeOffset Timestamp, double Value);

    public record ZoneOptionDto(Guid Id, string Name, int Floor);

    public record GatewayMessageDto(DateTimeOffset Timestamp, string Direction, string MessageType, string Payload);

    // Detail gatewaye pro průvodce připojením — kanály s živými hodnotami, místnosti objektu, log zpráv.
    // Obrazovka ho načítá každé 2 s, proto žádné těžké dotazy.
    public class GetGatewayHandler : IRequestHandler<GetGatewayCommand, Response<GetGatewayDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IIqrfGatewayState _state;
        private readonly IqrfSettings _settings;

        public GetGatewayHandler(SmartAuditDbContext dbContext, IIqrfGatewayState state, IqrfSettings settings)
        {
            _dbContext = dbContext;
            _state = state;
            _settings = settings;
        }

        public async Task<Response<GetGatewayDto>> Handle(GetGatewayCommand request, CancellationToken ct)
        {
            var device = await _dbContext.SensorDevices
                .AsNoTracking()
                .Where(d => d.Id == request.Id)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.GatewayId,
                    d.PollSeconds,
                    d.IsVirtual,
                    d.NodeAddresses,
                    d.LastSeenAt,
                    d.FacilityId,
                    FacilityCode = d.Facility.Code,
                    FacilityName = d.Facility.Name,
                    d.Facility.OrganizationId,
                    Channels = d.Channels
                        .OrderBy(c => c.NodeAddress)
                        .ThenBy(c => c.ChannelKey)
                        .Select(c => new
                        {
                            c.Id,
                            c.ChannelKey,
                            c.NodeAddress,
                            c.IqrfType,
                            c.Name,
                            c.Unit,
                            c.Metric,
                            c.LastValue,
                            c.LastSeenAt,
                            c.SensorId,
                            ZoneId = c.Sensor != null ? c.Sensor.ZoneId : null,
                            ZoneName = c.Sensor != null && c.Sensor.Zone != null ? c.Sensor.Zone.Name : null,
                        })
                        .ToList(),
                })
                .FirstOrDefaultAsync(ct);

            if (device == null)
                return Response.NotFound<GetGatewayDto>("Gateway s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && device.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<GetGatewayDto>("K tomuto nemáte oprávnění");

            var zones = await _dbContext.Zones
                .AsNoTracking()
                .Where(z => z.FacilityId == device.FacilityId)
                .OrderBy(z => z.Floor)
                .ThenBy(z => z.Order)
                .Select(z => new ZoneOptionDto(z.Id, z.Name, z.Floor))
                .ToListAsync(ct);

            var channels = device.Channels
                .Select(c => new ChannelDto(
                    c.Id,
                    c.ChannelKey,
                    c.NodeAddress,
                    c.IqrfType,
                    c.Name,
                    c.Unit,
                    c.Metric,
                    c.LastValue,
                    c.LastSeenAt,
                    c.SensorId,
                    c.ZoneId,
                    c.SensorId != null ? c.ZoneName ?? "Celý objekt" : null,
                    _state.GetSamples(device.GatewayId, c.ChannelKey).Select(s => new ChannelSampleDto(s.Timestamp, s.Value)).ToList()))
                .ToList();

            var messages = _state.GetMessages(device.GatewayId)
                .Select(m => new GatewayMessageDto(m.Timestamp, m.Direction.ToString(), m.MessageType, m.Payload))
                .ToList();

            var dto = new GetGatewayDto(
                device.Id,
                device.Name,
                device.GatewayId,
                device.PollSeconds,
                device.IsVirtual,
                device.NodeAddresses,
                GatewayStatus.IsOnline(device.LastSeenAt, device.PollSeconds, DateTimeOffset.UtcNow),
                device.LastSeenAt,
                new GatewayFacilityDto(device.FacilityId, device.FacilityCode, device.FacilityName),
                GatewayStatus.Broker(_state, _settings),
                string.IsNullOrEmpty(_settings.DeviceSecret) ? null : GatewayCredentials.For(_settings.DeviceSecret, device.GatewayId),
                channels,
                zones,
                messages);

            return Response.Success(dto);
        }
    }
}
