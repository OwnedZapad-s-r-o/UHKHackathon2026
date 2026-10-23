using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Api.Services.Import;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Reference
{
    public interface IMicroriscReferenceService
    {
        // Objekt, organizace, prostory, gateway a skutečná čidla — idempotentní, volá se při každém startu
        Task<Guid> EnsureFacilityAsync(CancellationToken ct);

        // Katastrální karta z LV 8314 a RÚIAN; když ČÚZK WFS neodpověděl, záložní půdorys budovy
        Task ApplyCadastreAsync(CancellationToken ct);

        // Dopíše skutečná měření z CSV až do now (posun o celé týdny) — vrací počet nových měření
        Task<int> SyncTelemetryAsync(DateTimeOffset now, CancellationToken ct);
    }

    // Referenční objekt MICRORISC s.r.o. v Jičíně se skutečnými měřeními teploty a vlhkosti ze tří skladů (6× CSV).
    // Čidla jsou skutečná (SensorSource.Physical) — simulace je nekreslí, audit z nich počítá jako z jakékoli brány IQRF.
    public class MicroriscReferenceService : IMicroriscReferenceService
    {
        private const int TemperatureType = 1;
        private const int HumidityType = 128;

        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

        // Načtené CSV — mění se jen s novým exportem, stačí jednou za běh
        private static IReadOnlyList<MeasuredSeries>? _series;
        private static readonly object SeriesLock = new();

        private readonly SmartAuditDbContext _dbContext;
        private readonly IOpenDataImportService _importService;
        private readonly IAuditTrigger _auditTrigger;
        private readonly OpenDataSettings _openDataSettings;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MicroriscReferenceService> _logger;

        public MicroriscReferenceService(SmartAuditDbContext dbContext, IOpenDataImportService importService, IAuditTrigger auditTrigger, OpenDataSettings openDataSettings, IWebHostEnvironment environment, ILogger<MicroriscReferenceService> logger)
        {
            _dbContext = dbContext;
            _importService = importService;
            _auditTrigger = auditTrigger;
            _openDataSettings = openDataSettings;
            _environment = environment;
            _logger = logger;
        }

        // seed/microrisc vedle seed/opendata (v Dockeru /seed/microrisc)
        private string DataDirectory => Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _openDataSettings.SnapshotPath, "..", MicroriscReference.DataDirectory));

        private IReadOnlyList<MeasuredSeries> Series()
        {
            lock (SeriesLock)
            {
                if (_series is { Count: > 0 })
                    return _series;

                _series = MicroriscCsvImporter.LoadDirectory(DataDirectory);
                _logger.LogInformation("MICRORISC: načteno {Count} řad měření ({Points} bodů) z {Directory}", _series.Count, _series.Sum(s => s.Points.Count), DataDirectory);

                return _series;
            }
        }

        public async Task<Guid> EnsureFacilityAsync(CancellationToken ct)
        {
            var series = Series();

            var organization = await _dbContext.Organizations.FirstOrDefaultAsync(o => o.Ico == MicroriscReference.Ico, ct);
            if (organization == null)
            {
                organization = new Organization(MicroriscReference.Ico, MicroriscReference.OrganizationName, MicroriscReference.LegalForm);
                _dbContext.Organizations.Add(organization);
            }

            var facility = await _dbContext.Facilities
                .Include(f => f.Zones)
                .FirstOrDefaultAsync(f => f.SourceKey == MicroriscReference.SourceKey, ct);

            var location = Factory.CreatePoint(new Coordinate(MicroriscReference.Longitude, MicroriscReference.Latitude));

            if (facility == null)
            {
                facility = new Facility(MicroriscReference.SourceKey, MicroriscReference.Name, FacilityCategory.Reference, location);
                _dbContext.Facilities.Add(facility);
            }
            else
            {
                facility.Rename(MicroriscReference.Name);
                facility.Relocate(location);
            }

            facility.SetIdentifiers([MicroriscReference.Kind], [], null, MicroriscReference.Ico, MicroriscReference.Web);
            facility.SetAddress(
                MicroriscReference.Street,
                MicroriscReference.HouseNumber,
                MicroriscReference.Psc,
                MicroriscReference.MunicipalityCode,
                MicroriscReference.MunicipalityName,
                MicroriscReference.OrpCode,
                MicroriscReference.OrpName,
                MicroriscReference.DistrictCode,
                MicroriscReference.DistrictName);
            facility.AssignOrganization(organization.Id);
            facility.AssignCode(MicroriscReference.Code);
            facility.SetHeritage(HeritageStatus.None, null);
            facility.SetSimulationProfile(SimulationProfile.Nominal);

            await _dbContext.SaveChangesAsync(ct);

            // Prostory podle sloupců CSV
            var order = 0;
            var zones = new Dictionary<string, Zone>();
            foreach (var definition in MicroriscReference.Zones)
            {
                var zone = facility.Zones.FirstOrDefault(z => z.Name == definition.Name);
                if (zone == null)
                {
                    zone = new Zone(facility.Id, definition.Name, ZoneType.Storage, definition.Floor, "skladová část", order);
                    _dbContext.Zones.Add(zone);
                }

                zones[definition.Name] = zone;
                order++;
            }

            var device = await _dbContext.SensorDevices.FirstOrDefaultAsync(d => d.GatewayId == MicroriscReference.GatewayId, ct);
            if (device == null)
            {
                // Export má krok 4 minuty — stejně často gateway „odpovídá“
                device = new SensorDevice(facility.Id, MicroriscReference.GatewayName, MicroriscReference.GatewayId, 240, MicroriscReference.Zones.Select(z => z.NodeAddress).ToArray());
                _dbContext.SensorDevices.Add(device);
            }

            await _dbContext.SaveChangesAsync(ct);

            var sensors = await _dbContext.Sensors.Where(s => s.DeviceId == device.Id).ToListAsync(ct);
            var channels = await _dbContext.SensorChannels.Where(c => c.DeviceId == device.Id).ToListAsync(ct);
            var installedAt = series.Count > 0 ? series.Min(s => s.Points[0].Timestamp) : DateTimeOffset.UtcNow;

            // Jedno čidlo na prostor a veličinu — jen pro řady, které v CSV opravdu jsou
            foreach (var definition in MicroriscReference.Zones)
            {
                var zone = zones[definition.Name];

                foreach (var metric in series.Where(s => s.Zone == definition.Name).Select(s => s.Metric).Distinct())
                {
                    var sensor = sensors.FirstOrDefault(s => s.ZoneId == zone.Id && s.Metric == metric);
                    if (sensor == null)
                    {
                        sensor = Sensor.Physical(facility.Id, zone.Id, metric, device.Id, installedAt);
                        _dbContext.Sensors.Add(sensor);
                        sensors.Add(sensor);
                    }

                    var type = metric == SensorMetric.Temperature ? TemperatureType : HumidityType;
                    var key = $"n{definition.NodeAddress}/t{type}-0";
                    if (channels.All(c => c.ChannelKey != key))
                    {
                        var channel = new SensorChannel(device.Id, key, definition.NodeAddress, type, $"{SensorUnits.Label(metric)} — {definition.Name}", SensorUnits.Of(metric), metric);
                        channel.LinkSensor(sensor.Id, metric);
                        _dbContext.SensorChannels.Add(channel);
                        channels.Add(channel);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(ct);
            _dbContext.ChangeTracker.Clear();

            await _importService.RecordExternalImportAsync(MicroriscReference.DatasetKey, series.Count > 0 ? ImportStatus.Live : ImportStatus.Failed, series.Sum(s => s.Points.Count),
                series.Count > 0 ? null : $"Ve složce {DataDirectory} nejsou exporty CSV", ct);

            return facility.Id;
        }

        public async Task ApplyCadastreAsync(CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .Include(f => f.Building)
                .FirstOrDefaultAsync(f => f.SourceKey == MicroriscReference.SourceKey, ct);

            if (facility == null)
                return;

            var building = facility.Building;
            if (building == null)
            {
                building = new FacilityBuilding(facility.Id);
                _dbContext.FacilityBuildings.Add(building);
            }

            // ČÚZK WFS neodpověděl (offline) — půdorys a údaje stavebního objektu 54097673 ze zálohy
            if (!building.Found)
            {
                var footprint = new WKTReader(new NtsGeometryServices(new PrecisionModel(), 4326)).Read(MicroriscReference.FootprintWkt);
                footprint.SRID = 4326;
                building.SetCadastre(footprint, MicroriscReference.BuiltUpArea, MicroriscReference.ConstructionYear, "industrial", "functional", null, MicroriscReference.RuianCode);
                _logger.LogInformation("MICRORISC: ČÚZK WFS bez odpovědi, použit záložní půdorys stavebního objektu {Ruian}", MicroriscReference.RuianCode);
            }

            building.SetCadastralCard(
                MicroriscReference.CadastralTerritory,
                MicroriscReference.OwnershipSheet,
                MicroriscReference.ParcelNumbers,
                MicroriscReference.ParcelArea,
                MicroriscReference.MunicipalityPart,
                MicroriscReference.OrganizationName,
                MicroriscReference.StructureUse,
                MicroriscReference.FloorArea,
                MicroriscReference.EnclosedVolume,
                MicroriscReference.HeatingType);

            // RÚIAN uvádí 3 podlaží — přesnější než odhad z plochy
            building.SetEstimates(BuildingEstimator.CategoryFromYear(building.ConstructionYear ?? MicroriscReference.ConstructionYear), MicroriscReference.Floors);

            await _dbContext.SaveChangesAsync(ct);
            _dbContext.ChangeTracker.Clear();
        }

        public async Task<int> SyncTelemetryAsync(DateTimeOffset now, CancellationToken ct)
        {
            var series = Series();
            if (series.Count == 0)
                return 0;

            var anchor = series.Min(s => s.Points[0].Timestamp);

            var sensors = await _dbContext.Sensors
                .Include(s => s.Zone)
                .Include(s => s.Device)
                .Where(s => s.Device != null && s.Device.GatewayId == MicroriscReference.GatewayId && s.IsActive)
                .ToListAsync(ct);

            if (sensors.Count == 0)
                return 0;

            var windowStart = now - MeasuredReplay.Period;
            var sensorIds = sensors.Select(s => s.Id).ToList();
            var lastReadings = await _dbContext.SensorReadings
                .Where(r => sensorIds.Contains(r.SensorId) && r.Timestamp >= windowStart)
                .GroupBy(r => r.SensorId)
                .Select(g => new { SensorId = g.Key, Last = g.Max(r => r.Timestamp) })
                .ToDictionaryAsync(x => x.SensorId, x => x.Last, ct);

            var channels = await _dbContext.SensorChannels.Where(c => c.SensorId != null && sensorIds.Contains(c.SensorId.Value)).ToListAsync(ct);
            var written = 0;

            foreach (var sensor in sensors)
            {
                var source = series.FirstOrDefault(s => s.Zone == sensor.Zone?.Name && s.Metric == sensor.Metric);
                if (source == null)
                    continue;

                // Bez měření v okně = první start: celý týden; jinak jen to, co přibylo od posledního zápisu
                var from = lastReadings.TryGetValue(sensor.Id, out var last) ? last.ToUniversalTime() : windowStart;
                var points = MeasuredReplay.Between(source, anchor, from, now).ToList();
                if (points.Count == 0)
                    continue;

                _dbContext.SensorReadings.AddRange(points.Select(p => new SensorReading(sensor.Id, p.Timestamp, p.Value)));

                var latest = points[^1];
                sensor.RecordReading(latest.Value, latest.Timestamp);
                channels.FirstOrDefault(c => c.SensorId == sensor.Id)?.RecordValue(latest.Value, latest.Timestamp);
                written += points.Count;
            }

            // Gateway „odpovídá“ s každou synchronizací — v Senzorech je online jako skutečná brána
            sensors[0].Device!.MarkSeen(now);

            await _dbContext.SaveChangesAsync(ct);
            _dbContext.ChangeTracker.Clear();

            if (written > 0)
                _auditTrigger.RequestFacility(sensors[0].FacilityId);

            return written;
        }
    }
}
