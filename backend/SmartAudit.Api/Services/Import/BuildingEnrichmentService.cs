using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Import
{
    public interface IBuildingEnrichmentService
    {
        Task<int> EnrichAsync(bool refresh, CancellationToken ct);
    }

    // Karta budovy z ČÚZK INSPIRE (F20): půdorys, plocha, rok výstavby → energetická kategorie, odhad podlaží
    public class BuildingEnrichmentService : IBuildingEnrichmentService
    {
        public const string SourceKey = "cuzk-budovy";
        private const string SnapshotFile = "cuzk-budovy.json";

        private record SnapshotEntry(string? Wkt, double? Area, int? Year, string? Use, string? Condition, string? Parcel, string? Ruian);

        private readonly SmartAuditDbContext _dbContext;
        private readonly ICuzkBuildingService _cuzkService;
        private readonly IOpenDataImportService _importService;
        private readonly SnapshotStore _snapshots;
        private readonly OpenDataSettings _settings;
        private readonly ILogger<BuildingEnrichmentService> _logger;

        public BuildingEnrichmentService(SmartAuditDbContext dbContext, ICuzkBuildingService cuzkService, IOpenDataImportService importService, SnapshotStore snapshots, OpenDataSettings settings, ILogger<BuildingEnrichmentService> logger)
        {
            _dbContext = dbContext;
            _cuzkService = cuzkService;
            _importService = importService;
            _snapshots = snapshots;
            _settings = settings;
            _logger = logger;
        }

        public async Task<int> EnrichAsync(bool refresh, CancellationToken ct)
        {
            var facilities = await _dbContext.Facilities
                .Include(f => f.Building)
                .Where(f => refresh || f.Building == null)
                .ToListAsync(ct);

            if (facilities.Count == 0)
                return 0;

            var snapshot = await ReadSnapshotAsync(ct);
            var fetched = new ConcurrentDictionary<string, SnapshotEntry>();
            var failures = 0;

            using var throttle = new SemaphoreSlim(Math.Max(1, _settings.CuzkParallelism));

            await Task.WhenAll(facilities.Select(async facility =>
            {
                await throttle.WaitAsync(ct);
                try
                {
                    var building = await _cuzkService.FindBuildingAsync(facility.Location, ct);
                    fetched[facility.SourceKey] = building == null
                        ? new SnapshotEntry(null, null, null, null, null, null, null)
                        : new SnapshotEntry(new WKTWriter().Write(building.Footprint), building.BuiltUpArea, building.ConstructionYear, building.CurrentUse, building.Condition, building.ParcelRef, building.RuianCode);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Xml.XmlException)
                {
                    Interlocked.Increment(ref failures);
                    _logger.LogDebug(ex, "ČÚZK nevrátil budovu pro {Facility}", facility.SourceKey);
                }
                finally
                {
                    throttle.Release();
                }
            }));

            var reader = new WKTReader(new NtsGeometryServices(new PrecisionModel(), 4326));
            var found = 0;

            foreach (var facility in facilities)
            {
                if (!fetched.TryGetValue(facility.SourceKey, out var entry) && !snapshot.TryGetValue(facility.SourceKey, out entry))
                    entry = new SnapshotEntry(null, null, null, null, null, null, null);

                var footprint = entry.Wkt == null ? null : reader.Read(entry.Wkt);
                if (footprint != null)
                    footprint.SRID = 4326;

                var building = facility.Building;
                if (building == null)
                {
                    building = new FacilityBuilding(facility.Id);
                    _dbContext.FacilityBuildings.Add(building);
                }

                building.SetCadastre(footprint, entry.Area, entry.Year, entry.Use, entry.Condition, entry.Parcel, entry.Ruian);

                var floors = BuildingEstimator.EstimateFloors(entry.Area, facility.Category);
                building.SetEstimates(BuildingEstimator.CategoryFromYear(entry.Year), floors);

                if (footprint != null)
                    found++;
            }

            await _dbContext.SaveChangesAsync(ct);

            var status = failures == facilities.Count ? (snapshot.Count > 0 ? ImportStatus.Snapshot : ImportStatus.Failed) : ImportStatus.Live;

            if (_settings.WriteSnapshots && !fetched.IsEmpty)
            {
                foreach (var (key, entry) in fetched)
                    snapshot[key] = entry;

                await _snapshots.WriteAsync(SnapshotFile, JsonSerializer.Serialize(snapshot), ct);
            }

            await _importService.RecordExternalImportAsync(SourceKey, status, found, failures > 0 ? $"{failures} dotazů selhalo" : null, ct);

            _logger.LogInformation("ČÚZK: půdorys nalezen u {Found} z {Total} objektů ({Failures} chyb)", found, facilities.Count, failures);

            return found;
        }

        private async Task<Dictionary<string, SnapshotEntry>> ReadSnapshotAsync(CancellationToken ct)
        {
            var json = await _snapshots.ReadAsync(SnapshotFile, ct);

            return json == null ? new() : JsonSerializer.Deserialize<Dictionary<string, SnapshotEntry>>(json) ?? new();
        }
    }
}
