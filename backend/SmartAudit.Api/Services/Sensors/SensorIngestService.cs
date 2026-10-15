using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Sensors
{
    public record IngestResult(bool KnownGateway, string MessageType, int Readings, int Stored);

    // Zpracuje odpověď IQRF gatewaye (z MQTT i z HTTP): objeví nové kanály, uloží hodnoty přiřazených čidel
    // a vyžádá přepočet auditu. Hodnoty nepřiřazených kanálů zůstanou jen v paměti pro průvodce připojením.
    public interface ISensorIngestService
    {
        Task<IngestResult?> IngestAsync(string gatewayId, string payload, CancellationToken ct);
    }

    public class SensorIngestService : ISensorIngestService
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IIqrfGatewayState _state;
        private readonly IAuditTrigger _auditTrigger;

        public SensorIngestService(SmartAuditDbContext dbContext, IIqrfGatewayState state, IAuditTrigger auditTrigger)
        {
            _dbContext = dbContext;
            _state = state;
            _auditTrigger = auditTrigger;
        }

        public async Task<IngestResult?> IngestAsync(string gatewayId, string payload, CancellationToken ct)
        {
            var response = IqrfResponseParser.Parse(payload);
            if (response == null)
                return null;

            // Vlastní dotazy se na topicu odpovědí neobjevují, ale zrcadlený request od daemonu ano — ten nic nenese
            _state.RecordMessage(gatewayId, GatewayMessageDirection.Response, response.MessageType, payload);

            var now = DateTimeOffset.UtcNow;
            if (!payload.Contains(VirtualGatewayResponder.InstanceId, StringComparison.Ordinal))
                _state.RecordExternalResponse(gatewayId, now);
            var device = await _dbContext.SensorDevices
                .Include(d => d.Channels)
                .FirstOrDefaultAsync(d => d.GatewayId == gatewayId, ct);

            if (device == null)
            {
                _state.RecordUnknown(gatewayId, now);
                return new IngestResult(false, response.MessageType, response.Readings.Count, 0);
            }

            device.MarkSeen(now);

            if (response.IsOk && response.BondedNodes != null)
                device.SetNodes(response.BondedNodes);

            var toStore = new List<(Guid SensorId, double Value)>();

            foreach (var reading in response.IsOk ? response.Readings : [])
            {
                var channel = device.Channels.FirstOrDefault(c => c.ChannelKey == reading.ChannelKey);
                if (channel == null)
                {
                    var metric = IqrfSensorTypes.MetricOf(reading.Type);
                    channel = new SensorChannel(device.Id, reading.ChannelKey, reading.NodeAddress, reading.Type, reading.Name, reading.Unit.Length > 0 ? reading.Unit : SensorUnits.Of(metric), metric);
                    _dbContext.SensorChannels.Add(channel);
                    device.Channels.Add(channel);
                }

                channel.RecordValue(reading.Value, now);
                _state.RecordSample(gatewayId, reading.ChannelKey, new ChannelSample(now, reading.Value));

                if (channel.SensorId is { } sensorId)
                    toStore.Add((sensorId, reading.Value));
            }

            if (toStore.Count > 0)
            {
                var sensorIds = toStore.Select(r => r.SensorId).ToList();
                var sensors = await _dbContext.Sensors.Where(s => sensorIds.Contains(s.Id)).ToListAsync(ct);
                foreach (var sensor in sensors)
                    sensor.RecordReading(toStore.Last(r => r.SensorId == sensor.Id).Value, now);
            }

            await _dbContext.SaveChangesAsync(ct);

            var stored = await StoreReadingsAsync(toStore, now, ct);

            // Přepočet auditu jen pro budovu této gatewaye, ne pro celý kraj
            if (stored > 0)
                _auditTrigger.RequestFacility(device.FacilityId);

            return new IngestResult(true, response.MessageType, response.Readings.Count, stored);
        }

        // Všechna měření jedné odpovědi jedním příkazem (pole senzorů a hodnot přes unnest) — jeden round-trip do databáze
        // místo jednoho na každé čidlo. Klíč (SensorId, Timestamp): opakovaná zpráva ve stejné sekundě se tiše zahodí.
        private async Task<int> StoreReadingsAsync(List<(Guid SensorId, double Value)> readings, DateTimeOffset now, CancellationToken ct)
        {
            if (readings.Count == 0)
                return 0;

            var timestamp = new DateTimeOffset(now.UtcTicks - now.UtcTicks % TimeSpan.TicksPerSecond, TimeSpan.Zero);
            var sensorIds = readings.Select(r => r.SensorId).ToArray();
            var values = readings.Select(r => r.Value).ToArray();

            return await _dbContext.Database.ExecuteSqlAsync($"""
                INSERT INTO "SensorReadings" ("SensorId", "Timestamp", "Value")
                SELECT reading."SensorId", {timestamp}, reading."Value"
                FROM unnest({sensorIds}, {values}) AS reading("SensorId", "Value")
                ON CONFLICT DO NOTHING
                """, ct);
        }
    }
}
