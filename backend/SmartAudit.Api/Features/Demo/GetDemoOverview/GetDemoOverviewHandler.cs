using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Demo.GetDemoOverview
{
    public record GetDemoOverviewCommand(ViewerContext Viewer) : IRequest<Response<DemoOverviewDto>>;

    public record DemoOverviewDto(
        IReadOnlyCollection<DemoTemplateDto> Templates,
        IReadOnlyCollection<DemoParentGatewayDto> Gateways,
        IReadOnlyCollection<DemoDeviceDto> Devices,
        IReadOnlyCollection<LossCorrectionDto> Corrections);

    public record DemoTemplateDto(string Id, string Label, List<SensorMetric> Metrics);

    // Virtuální gateway, do jejíhož dosahu jde „položit“ nové čidlo
    public record DemoParentGatewayDto(Guid Id, string GatewayId, string Name, string FacilityName);

    // Kde je zařízení v párování — od zapnutí po měření v místnosti
    public enum DemoStage
    {
        Starting,
        Announcing,
        InRange,
        Paired,
        Reporting,
        Assigned,
    }

    public record DemoDeviceDto(
        Guid Id,
        DemoDeviceKind Kind,
        string Serial,
        string Model,
        string Code,
        List<string> Nodes,
        List<SensorMetric> Metrics,
        DemoBehavior Behavior,
        DemoStage Stage,
        string? ParentName,
        Guid? GatewayId,
        Guid? FacilityId,
        string? FacilityName,
        int ChannelCount,
        int AssignedCount,
        DateTimeOffset CreatedAt);

    // Oprava odhadu ztráty skutečným měřením: ztráta těsně před prvním čidlem teploty → teď
    public record LossCorrectionDto(
        Guid FacilityId,
        string FacilityCode,
        string FacilityName,
        decimal? LossBeforeCzk,
        decimal LossNowCzk,
        int MeasuredZones,
        int ZoneCount,
        bool VirtualHistory,
        DateTimeOffset Since,
        DateTimeOffset EvaluatedAt);

    // Demo správa: vyrobená zařízení a kde jsou v párování, plus budovy, kterým čidla opravila odhad ztráty.
    // Kraj vidí všechno, správce budovy jen zařízení a budovy své organizace.
    public class GetDemoOverviewHandler : IRequestHandler<GetDemoOverviewCommand, Response<DemoOverviewDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IPairingRegistry _registry;

        public GetDemoOverviewHandler(SmartAuditDbContext dbContext, IPairingRegistry registry)
        {
            _dbContext = dbContext;
            _registry = registry;
        }

        public async Task<Response<DemoOverviewDto>> Handle(GetDemoOverviewCommand request, CancellationToken ct)
        {
            var manager = request.Viewer.Role == AppRoles.Manager;
            var organizationId = request.Viewer.OrganizationId;

            var demoDevices = await _dbContext.DemoDevices
                .AsNoTracking()
                .Where(d => !manager || d.OrganizationId == organizationId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(ct);

            // Gatewaye v aplikaci: spárované demo gatewaye a gatewaye, v jejichž dosahu jsou demo čidla
            var gatewayIds = demoDevices
                .Select(d => d.Kind == DemoDeviceKind.Gateway ? PairableDevices.GatewayIdOf(d.Serial) : d.ParentGatewayId!)
                .Distinct()
                .ToList();

            var sensorDevices = await _dbContext.SensorDevices
                .AsNoTracking()
                .Where(d => gatewayIds.Contains(d.GatewayId))
                .Select(d => new
                {
                    d.Id,
                    d.GatewayId,
                    d.Name,
                    d.NodeAddresses,
                    d.FacilityId,
                    FacilityName = d.Facility.Name,
                    Channels = d.Channels.Select(c => new { c.NodeAddress, Assigned = c.SensorId != null }).ToList(),
                })
                .ToListAsync(ct);

            var byGatewayId = sensorDevices.ToDictionary(d => d.GatewayId, StringComparer.OrdinalIgnoreCase);
            var now = DateTimeOffset.UtcNow;

            var devices = demoDevices
                .Select(d =>
                {
                    var nodes = d.Templates.Select(DemoNodeTemplates.Find).OfType<DemoNodeTemplate>().ToList();
                    var labels = nodes.Select(n => n.Label).ToList();
                    var metrics = nodes.SelectMany(n => n.Sensors).Select(s => IqrfSensorTypes.MetricOf(s.Type)).Distinct().ToList();

                    if (d.Kind == DemoDeviceKind.Gateway)
                    {
                        var device = byGatewayId.GetValueOrDefault(PairableDevices.GatewayIdOf(d.Serial));
                        var channels = device?.Channels.Count ?? 0;
                        var assigned = device?.Channels.Count(c => c.Assigned) ?? 0;
                        var stage = device == null
                            ? _registry.Find(d.Serial, now) != null ? DemoStage.Announcing : DemoStage.Starting
                            : Stage(channels, assigned);

                        return new DemoDeviceDto(d.Id, d.Kind, d.Serial, d.Model, d.Code, labels, metrics, d.Behavior, stage,
                            null, device?.Id, device?.FacilityId, device?.FacilityName, channels, assigned, d.CreatedAt);
                    }

                    var parent = byGatewayId.GetValueOrDefault(d.ParentGatewayId!);
                    var bonded = parent != null && parent.NodeAddresses.Contains(d.NodeAddress!.Value);
                    var nodeChannels = parent?.Channels.Where(c => c.NodeAddress == d.NodeAddress).ToList() ?? [];
                    var nodeAssigned = nodeChannels.Count(c => c.Assigned);

                    return new DemoDeviceDto(d.Id, d.Kind, d.Serial, d.Model, d.Code, labels, metrics, d.Behavior,
                        bonded ? Stage(nodeChannels.Count, nodeAssigned) : DemoStage.InRange,
                        parent?.Name, parent?.Id, parent?.FacilityId, parent?.FacilityName, nodeChannels.Count, nodeAssigned, d.CreatedAt);
                })
                .ToList();

            var parents = await _dbContext.SensorDevices
                .AsNoTracking()
                .Where(d => d.IsVirtual && (!manager || d.Facility.OrganizationId == organizationId))
                .OrderBy(d => d.Name)
                .Select(d => new DemoParentGatewayDto(d.Id, d.GatewayId, d.Name, d.Facility.Name))
                .ToListAsync(ct);

            var templates = DemoNodeTemplates.All
                .Select(t => new DemoTemplateDto(t.Id, t.Label, t.Sensors.Select(s => IqrfSensorTypes.MetricOf(s.Type)).Distinct().ToList()))
                .ToList();

            return Response.Success(new DemoOverviewDto(templates, parents, devices, await CorrectionsAsync(manager, organizationId, ct)));
        }

        private async Task<List<LossCorrectionDto>> CorrectionsAsync(bool manager, Guid? organizationId, CancellationToken ct)
        {
            var corrections = await _dbContext.FacilityAssessments
                .AsNoTracking()
                .Where(a => a.SensorsSince != null && (!manager || a.Facility.OrganizationId == organizationId))
                .OrderByDescending(a => a.SensorsSince)
                .Select(a => new
                {
                    a.FacilityId,
                    a.Facility.Code,
                    a.Facility.Name,
                    a.LossBeforeSensorsCzk,
                    a.AnnualLossCzk,
                    ZoneCount = a.Facility.Zones.Count,
                    Since = a.SensorsSince!.Value,
                    a.EvaluatedAt,
                })
                .ToListAsync(ct);

            var facilityIds = corrections.Select(c => c.FacilityId).ToList();
            var measuring = await MeasuredZones.QueryAsync(_dbContext, facilityIds, ct);

            return corrections
                .Select(c =>
                {
                    var zones = measuring.GetValueOrDefault(c.FacilityId);
                    return new LossCorrectionDto(c.FacilityId, c.Code, c.Name, c.LossBeforeSensorsCzk, c.AnnualLossCzk,
                        zones?.Count ?? 0, c.ZoneCount, zones?.Virtual ?? false, c.Since, c.EvaluatedAt);
                })
                .ToList();
        }

        private static DemoStage Stage(int channels, int assigned) => channels == 0 ? DemoStage.Paired : assigned == 0 ? DemoStage.Reporting : DemoStage.Assigned;
    }
}
