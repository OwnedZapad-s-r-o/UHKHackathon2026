using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Sensors
{
    public record ProvisionedGateway(Guid Id, string GatewayId, string FacilityCode, bool Created, bool Preassigned);

    // Založení virtuální gateway v databázi. U předem přiřazené gateway vzniknou rovnou kanály a skutečná čidla v místnostech
    // (stejně jako po ručním přiřazení v průvodci), takže budova má živá data hned po první odpovědi.
    public interface IVirtualGatewayProvisioner
    {
        Task<ProvisionedGateway?> EnsurePresetAsync(VirtualGatewayPreset preset, CancellationToken ct);
        Task<SensorDevice> CreateClassroomAsync(Guid facilityId, string facilityCode, CancellationToken ct);
    }

    public class VirtualGatewayProvisioner : IVirtualGatewayProvisioner
    {
        // Virtuální gateway odpovídá hned — krátký interval, aby demo bylo živé
        private const int PollSeconds = 10;

        private readonly SmartAuditDbContext _dbContext;
        private readonly IAuditTrigger _auditTrigger;

        public VirtualGatewayProvisioner(SmartAuditDbContext dbContext, IAuditTrigger auditTrigger)
        {
            _dbContext = dbContext;
            _auditTrigger = auditTrigger;
        }

        public async Task<ProvisionedGateway?> EnsurePresetAsync(VirtualGatewayPreset preset, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities.AsNoTracking().FirstOrDefaultAsync(f => f.Code == preset.FacilityCode, ct);
            if (facility == null)
                return null;

            var existing = await _dbContext.SensorDevices.AsNoTracking().FirstOrDefaultAsync(d => d.GatewayId == preset.GatewayId, ct);
            if (existing != null)
                return new ProvisionedGateway(existing.Id, existing.GatewayId, preset.FacilityCode, false, preset.Preassigned);

            var device = new SensorDevice(facility.Id, preset.Name, preset.GatewayId, PollSeconds, preset.Nodes.Select(n => n.Address).ToArray(), isVirtual: true);
            _dbContext.SensorDevices.Add(device);

            if (preset.Preassigned)
                await AssignNodesAsync(device, facility.Id, preset.Nodes, ct);

            await _dbContext.SaveChangesAsync(ct);

            if (preset.Preassigned)
                _auditTrigger.RequestFacility(facility.Id);

            return new ProvisionedGateway(device.Id, device.GatewayId, preset.FacilityCode, true, preset.Preassigned);
        }

        public async Task<SensorDevice> CreateClassroomAsync(Guid facilityId, string facilityCode, CancellationToken ct)
        {
            // Id jako skutečná gateway, jen s poznávacím „virtualni-“ — jednoznačné i při opakovaném kliknutí
            var gatewayId = $"virtualni-{facilityCode.ToLowerInvariant()}-{Guid.NewGuid():N}"[..(19 + facilityCode.Length)];
            var device = new SensorDevice(facilityId, "Virtuální učebna (demo bez hardwaru)", gatewayId, PollSeconds, VirtualGateways.Classroom.Select(n => n.Address).ToArray(), isVirtual: true);

            _dbContext.SensorDevices.Add(device);
            await _dbContext.SaveChangesAsync(ct);

            return device;
        }

        // Každý uzel do místnosti jeho druhu (učebna, jídelna, dílna…), jinak do první volné místnosti budovy
        private async Task AssignNodesAsync(SensorDevice device, Guid facilityId, IReadOnlyList<VirtualNode> nodes, CancellationToken ct)
        {
            var zones = await _dbContext.Zones.Where(z => z.FacilityId == facilityId).OrderBy(z => z.Floor).ThenBy(z => z.Order).ToListAsync(ct);
            var used = new HashSet<Guid>();
            var now = DateTimeOffset.UtcNow;

            foreach (var node in nodes)
            {
                var zone = zones.FirstOrDefault(z => z.Type == node.PreferredZone && !used.Contains(z.Id)) ?? zones.FirstOrDefault(z => !used.Contains(z.Id));
                if (zone != null)
                    used.Add(zone.Id);

                var ordinals = new Dictionary<int, int>();
                foreach (var sensor in node.Sensors)
                {
                    var ordinal = ordinals.GetValueOrDefault(sensor.Type);
                    ordinals[sensor.Type] = ordinal + 1;

                    var metric = IqrfSensorTypes.MetricOf(sensor.Type);
                    var channel = new SensorChannel(device.Id, IqrfResponseParser.ChannelKey(node.Address, sensor.Type, ordinal), node.Address, sensor.Type, sensor.Name, sensor.Unit.Length > 0 ? sensor.Unit : SensorUnits.Of(metric), metric);
                    _dbContext.SensorChannels.Add(channel);

                    if (metric == SensorMetric.Other)
                        continue;

                    var physical = Sensor.Physical(facilityId, zone?.Id, metric, device.Id, now);
                    _dbContext.Sensors.Add(physical);
                    channel.LinkSensor(physical.Id, metric);
                    await SimulationHandover.TakeOverAsync(_dbContext, facilityId, zone?.Id, metric, ct);
                }
            }
        }
    }
}
