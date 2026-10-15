using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using SmartAudit.Api.Services.Import;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Audit
{
    public interface IPreAuditService
    {
        // facilityIds = null → celý kraj. Vrací počet objektů, kterým se změnil profil fyzikálního modelu.
        Task<int> ApplyAsync(IReadOnlyCollection<Guid>? facilityIds, CancellationToken ct);
    }

    // Pre-audit z otevřených dat pro každý objekt (OpenDataPreAudit): sesbírá vstupy z ČÚZK, Data KHK, Registru smluv
    // a Open-Meteo, uloží výsledek a nastaví profil fyzikálního modelu telemetrie. Když se profil změní, smaže
    // modelovanou historii objektu — simulace ji v dalším kroku dopočítá znovu podle nového profilu.
    public class PreAuditService : IPreAuditService
    {
        public const string RoadsSourceKey = "silnice-kraj";
        public const string AmbulanceSourceKey = "zzs-zakladny";
        public const string FireStationSourceKey = "hzs-stanice";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

        // Nadmořská výška v místě budovy (Open-Meteo Elevation API, Copernicus DEM) — mění se jen s polohou, stačí jednou za běh
        private static readonly ConcurrentDictionary<string, double> ElevationCache = new();
        private const int ElevationBatch = 100;

        private readonly SmartAuditDbContext _dbContext;
        private readonly IOpenDataImportService _importService;
        private readonly IAuditTrigger _auditTrigger;
        private readonly IOpenMeteoService _openMeteo;
        private readonly AuditSettings _settings;
        private readonly ILogger<PreAuditService> _logger;

        public PreAuditService(SmartAuditDbContext dbContext, IOpenDataImportService importService, IAuditTrigger auditTrigger, IOpenMeteoService openMeteo, AuditSettings settings, ILogger<PreAuditService> logger)
        {
            _openMeteo = openMeteo;
            _dbContext = dbContext;
            _importService = importService;
            _auditTrigger = auditTrigger;
            _settings = settings;
            _logger = logger;
        }

        public async Task<int> ApplyAsync(IReadOnlyCollection<Guid>? facilityIds, CancellationToken ct)
        {
            var ids = facilityIds?.ToArray();
            if (ids is { Length: 0 })
                return 0;

            var facilities = await _dbContext.Facilities
                .Where(f => ids == null || ids.Contains(f.Id))
                .Include(f => f.Building)
                .Include(f => f.PreAudit)
                .Include(f => f.Metrics)
                .Include(f => f.Organization)
                .AsSplitQuery()
                .ToListAsync(ct);

            if (facilities.Count == 0)
                return 0;

            await LoadElevationsAsync(facilities, ct);
            var context = await LoadContextAsync(facilities, ct);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var changed = new List<Guid>();

            foreach (var facility in facilities)
            {
                var input = BuildInput(facility, context, today);
                var result = OpenDataPreAudit.Evaluate(input);

                var preAudit = facility.PreAudit;
                if (preAudit == null)
                {
                    preAudit = new FacilityPreAudit(facility.Id);
                    _dbContext.FacilityPreAudits.Add(preAudit);
                }

                preAudit.Update(
                    Math.Clamp(result.Score, 0, 100),
                    result.Status,
                    result.Profile,
                    result.DominantDriver,
                    JsonSerializer.Serialize(result.Parameters, JsonOptions),
                    JsonSerializer.Serialize(result.Findings.Select(f => new { f.Title, f.Detail, f.Tag, Severity = f.Severity.ToString(), f.Source, f.SourceKeys }), JsonOptions),
                    result.SpecificHeatLoss,
                    result.HeatLossWattsPerKelvin,
                    result.DegreeDays,
                    result.AnnualHeatDemandKwh,
                    result.AnnualHeatCostCzk,
                    input.HeatPriceCzkPerKwh,
                    input.HeatPriceFromContract,
                    result.RegulationSavingPercent,
                    result.RegulationPotentialCzk,
                    result.IqrfKit.Gateways,
                    result.IqrfKit.Sensors,
                    result.IqrfKit.HardwareCostCzk);

                if (facility.SimulationProfile != result.Profile)
                {
                    facility.SetSimulationProfile(result.Profile);
                    changed.Add(facility.Id);
                }
            }

            await _dbContext.SaveChangesAsync(ct);
            _dbContext.ChangeTracker.Clear();

            if (changed.Count > 0)
            {
                await MarkHotspotsAsync(changed, ct);
                await ResetModelHistoryAsync(changed, ct);
            }

            _logger.LogInformation("Pre-audit z otevřených dat: {Count} objektů, změna profilu u {Changed}", facilities.Count, changed.Count);

            return changed.Count;
        }

        #region Vstupy

        private static string LocationKey(Point location) => string.Create(CultureInfo.InvariantCulture, $"{location.Y:F4},{location.X:F4}");

        // Výška těžiště ORP v podhůří výrazně přeceňuje budovy ve městech — proto výška přímo v bodě budovy.
        // Bez sítě zůstane výška ORP (a pak správního centra ORP).
        private async Task LoadElevationsAsync(List<Facility> facilities, CancellationToken ct)
        {
            var missing = facilities
                .Select(f => (Key: LocationKey(f.Location), f.Location))
                .Where(p => !ElevationCache.ContainsKey(p.Key))
                .DistinctBy(p => p.Key)
                .ToList();

            foreach (var batch in missing.Chunk(ElevationBatch))
            {
                try
                {
                    var elevations = await _openMeteo.GetElevationAsync(batch.Select(p => new WeatherPoint(p.Key, p.Location.Y, p.Location.X)).ToList(), ct);
                    foreach (var (key, elevation) in elevations)
                        ElevationCache[key] = Math.Round(elevation);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException)
                {
                    _logger.LogWarning(ex, "Open-Meteo Elevation pro budovy nedostupné — pre-audit použije výšku ORP");
                    return;
                }
            }
        }

        private sealed record Context(
            Dictionary<Guid, List<ContractRecord>> Contracts,
            Dictionary<Guid, EffectiveHeatPrice> HeatPrices,
            decimal MedianHeatPrice,
            Dictionary<string, Region> Orps,
            HashSet<string> ThreatenedMunicipalities,
            Dictionary<string, double> MeanTemperature,
            double? RegionMeanTemperature,
            Dictionary<string, double> MeanPm10,
            Dictionary<Guid, double> AddedAreas,
            NearestIndex Roads,
            NearestIndex Rescue);

        private async Task<Context> LoadContextAsync(List<Facility> facilities, CancellationToken ct)
        {
            var allContracts = await _dbContext.FacilityContracts
                .AsNoTracking()
                .Select(c => new { c.Id, c.OrganizationId, c.ContractId, c.Title, c.Category, c.DateSigned, c.Commodity, c.UnitPriceCzkPerMwh })
                .ToListAsync(ct);

            var contracts = allContracts
                .GroupBy(c => c.OrganizationId)
                .ToDictionary(g => g.Key, g => g.Select(c => new ContractRecord(c.ContractId, c.Title, c.Category, c.DateSigned)).ToList());

            // Cena tepla každé organizace ze smluv a krajský medián z organizací, které cenu ve smlouvě mají
            var heatPrices = allContracts
                .Where(c => c.Category == ContractCategory.EnergySupply)
                .GroupBy(c => c.OrganizationId)
                .ToDictionary(g => g.Key, g => EnergyPriceExtractor.SelectHeatPrice(g.Select(c => new ContractPrice(c.Id, c.Commodity, c.UnitPriceCzkPerMwh, c.DateSigned)), _settings.HeatPriceCzkPerKwh));

            var contractPrices = heatPrices.Values.Where(p => p.FromContract).Select(p => p.CzkPerKwh).OrderBy(p => p).ToList();
            var median = contractPrices.Count >= 3 ? contractPrices[contractPrices.Count / 2] : _settings.HeatPriceCzkPerKwh;

            var regions = await _dbContext.Regions
                .AsNoTracking()
                .Where(r => r.Type == RegionType.Orp || (r.Type == RegionType.Obec && r.IsThreatened))
                .ToListAsync(ct);

            // Teplota a prašnost posledních 7 dní po ORP (Open-Meteo)
            var weekAgo = DateTimeOffset.UtcNow.AddDays(-7);
            var weather = await _dbContext.WeatherObservations
                .AsNoTracking()
                .Where(w => w.Timestamp >= weekAgo && w.Timestamp <= DateTimeOffset.UtcNow)
                .GroupBy(w => w.OrpCode)
                .Select(g => new { OrpCode = g.Key, Temperature = g.Average(w => w.Temperature), Pm10 = g.Average(w => w.Pm10) })
                .ToListAsync(ct);

            var addedAreas = await _dbContext.FacilityBuildingParts
                .AsNoTracking()
                .GroupBy(p => p.FacilityId)
                .Select(g => new { FacilityId = g.Key, Area = g.Sum(p => p.AreaSquareMeters) })
                .ToDictionaryAsync(g => g.FacilityId, g => g.Area, ct);

            var roads = (await _importService.LoadAsync(RoadsSourceKey, ct))
                .Where(f => f.Geometry is LineString or MultiLineString)
                .Select(f => (Geometry: f.Geometry!, Label: RoadLabel(f)))
                .ToList();

            var rescue = (await _importService.LoadAsync(AmbulanceSourceKey, ct))
                .Select(f => (Point: f.Point(Factory), Label: $"ZZS {f.Text("vyjezdova_zakladna") ?? f.Text("nazev_obce")}"))
                .Concat((await _importService.LoadAsync(FireStationSourceKey, ct))
                    .Select(f => (Point: f.Point(Factory), Label: $"HZS {f.Text("nazev_obce")}")))
                .Where(r => r.Point != null)
                .Select(r => (Geometry: (Geometry)r.Point!, r.Label))
                .ToList();

            return new Context(
                contracts,
                heatPrices,
                median,
                regions.Where(r => r.Type == RegionType.Orp).GroupBy(r => r.Code).ToDictionary(g => g.Key, g => g.First()),
                regions.Where(r => r.Type == RegionType.Obec).Select(r => r.Code).ToHashSet(),
                weather.ToDictionary(w => w.OrpCode, w => w.Temperature),
                weather.Count > 0 ? weather.Average(w => w.Temperature) : null,
                weather.Where(w => w.Pm10 != null).ToDictionary(w => w.OrpCode, w => w.Pm10!.Value),
                addedAreas,
                new NearestIndex(roads),
                new NearestIndex(rescue));
        }

        private PreAuditInput BuildInput(Facility facility, Context context, DateOnly today)
        {
            var building = facility.Building;
            var addedArea = context.AddedAreas.GetValueOrDefault(facility.Id);
            var builtUpArea = building?.EffectiveArea(addedArea) ?? (addedArea > 0 ? addedArea : null);
            var floors = building?.EffectiveFloors ?? BuildingEstimator.EstimateFloors(builtUpArea, facility.Category);
            var floorArea = BuildingEstimator.FloorArea(builtUpArea, floors, facility.Category);

            var organizationId = facility.OrganizationId;
            var contracts = organizationId is { } orgId ? context.Contracts.GetValueOrDefault(orgId) ?? [] : [];
            var registryKnown = facility.Organization?.ContractsSyncedAt != null;
            var evidence = ContractEvidence.From(contracts, registryKnown, today);

            var heatPrice = organizationId is { } priceOrg && context.HeatPrices.TryGetValue(priceOrg, out var price) && price.FromContract
                ? price
                : new EffectiveHeatPrice(context.MedianHeatPrice, false, null);

            context.Orps.TryGetValue(facility.OrpCode, out var orp);
            var pointElevation = ElevationCache.TryGetValue(LocationKey(facility.Location), out var atBuilding) ? atBuilding : (double?)null;
            var elevation = pointElevation ?? orp?.Elevation ?? ClimateModel.FallbackElevation(facility.OrpCode);

            var (occupants, occupantsLabel) = Occupants(facility);

            var location = facility.Location;
            var road = context.Roads.Nearest(location, 600);
            var rescue = context.Rescue.Nearest(location, 40_000);

            return new PreAuditInput(
                facility.Category,
                facility.Kinds,
                building?.EffectiveYear,
                building?.EffectiveEnergyCategory ?? EnergyCategory.Unknown,
                floorArea,
                floors,
                builtUpArea is > 0,
                evidence,
                facility.HeritageStatus,
                orp?.Name ?? facility.OrpName,
                elevation,
                pointElevation != null || orp?.Elevation != null,
                context.MeanTemperature.TryGetValue(facility.OrpCode, out var temperature) ? temperature : null,
                context.RegionMeanTemperature,
                heatPrice.CzkPerKwh,
                heatPrice.FromContract,
                context.MedianHeatPrice,
                occupants,
                occupantsLabel,
                road?.Meters,
                road?.Label,
                context.MeanPm10.TryGetValue(facility.OrpCode, out var pm10) ? pm10 : null,
                rescue == null ? null : Math.Round(rescue.Value.Meters / 1000, 1),
                rescue?.Label,
                context.ThreatenedMunicipalities.Contains(facility.MunicipalityCode));
        }

        // Počet osob v budově podle otevřených dat: žáci a studenti posledního školního roku, u domovů mládeže ubytovaní
        private static (int? Occupants, string? Label) Occupants(Facility facility)
        {
            var accommodation = facility.Kinds.Any(k => k is "Domov mládeže" or "Internát");
            var beds = facility.Metrics.Where(m => m.BedCapacity is > 0).OrderByDescending(m => m.SchoolYear).FirstOrDefault();

            if (accommodation && beds != null)
                return (Math.Max(beds.Residents ?? 0, beds.BedCapacity!.Value), "Lůžka domova mládeže");

            var students = facility.Metrics
                .Where(m => m.Students is > 0 && m.SchoolYear.Contains('/'))
                .OrderByDescending(m => m.SchoolYear)
                .FirstOrDefault();

            if (students != null)
                return (students.Students, $"Žáci a studenti {students.SchoolYear}");

            return beds != null ? (beds.BedCapacity, "Lůžková kapacita") : (null, null);
        }

        private static string RoadLabel(RawFeature feature)
        {
            var number = feature.Text("ozn_sil")?.TrimStart('0');
            var roadClass = feature.Text("ozn_trida");

            if (number == null)
                return "II./III. třídy";

            return roadClass?.StartsWith("II.", StringComparison.Ordinal) == true ? $"II/{number}" : $"III/{number}";
        }

        #endregion

        // Špatně větraná zóna podle nového profilu — stejné pravidlo jako při založení zón (ZoneLayout)
        private async Task MarkHotspotsAsync(List<Guid> facilityIds, CancellationToken ct)
        {
            var facilities = await _dbContext.Facilities
                .Where(f => facilityIds.Contains(f.Id))
                .Include(f => f.Zones)
                .ToListAsync(ct);

            foreach (var facility in facilities)
            {
                var zones = facility.Zones.OrderBy(z => z.Order).ToList();
                var hotspot = ZoneLayout.HotspotIndex(zones.Select(z => (z.Type, z.Wing, z.Floor)).ToList(), facility.Category, facility.SimulationProfile);

                for (var i = 0; i < zones.Count; i++)
                    zones[i].SetHotspot(i == hotspot);
            }

            await _dbContext.SaveChangesAsync(ct);
            _dbContext.ChangeTracker.Clear();
        }

        // Modelovaná historie objektů se změněným profilem patří ke starému profilu — smaže se a simulace ji dopočítá
        // (skutečná čidla IQRF se nemažou nikdy)
        private async Task ResetModelHistoryAsync(List<Guid> facilityIds, CancellationToken ct)
        {
            var sensorIds = await _dbContext.Sensors
                .Where(s => facilityIds.Contains(s.FacilityId) && s.Source == SensorSource.Simulated)
                .Select(s => s.Id)
                .ToListAsync(ct);

            if (sensorIds.Count > 0)
            {
                var from = DateTimeOffset.UtcNow.AddDays(-40);
                await _dbContext.SensorReadings.Where(r => r.Timestamp >= from && sensorIds.Contains(r.SensorId)).ExecuteDeleteAsync(ct);
            }

            foreach (var facilityId in facilityIds)
                _auditTrigger.RequestProvisioning(facilityId);
        }

        // Nejbližší prvek (silnice, stanice IZS) v metrech — souřadnice WGS84 přepočtené na lokální rovinu kraje
        private sealed class NearestIndex
        {
            private const double MetersPerDegreeLat = 111_200;
            private static readonly double MetersPerDegreeLon = 111_320 * Math.Cos(50.4 * Math.PI / 180);

            private readonly STRtree<(Geometry Geometry, string Label)> _tree = new();
            private readonly bool _empty;

            public NearestIndex(IEnumerable<(Geometry Geometry, string Label)> items)
            {
                foreach (var (geometry, label) in items)
                {
                    var projected = Project(geometry);
                    _tree.Insert(projected.EnvelopeInternal, (projected, label));
                }

                _empty = _tree.Count == 0;
                if (!_empty)
                    _tree.Build();
            }

            public (double Meters, string Label)? Nearest(Point location, double maxMeters)
            {
                if (_empty)
                    return null;

                var point = (Point)Project(location);
                var search = new Envelope(point.X - maxMeters, point.X + maxMeters, point.Y - maxMeters, point.Y + maxMeters);

                (double Meters, string Label)? best = null;
                foreach (var (geometry, label) in _tree.Query(search))
                {
                    var distance = geometry.Distance(point);
                    if (distance <= maxMeters && (best == null || distance < best.Value.Meters))
                        best = (Math.Round(distance), label);
                }

                return best;
            }

            private static Geometry Project(Geometry geometry)
            {
                var copy = geometry.Copy();
                copy.Apply(new ProjectFilter());
                copy.GeometryChanged();

                return copy;
            }

            private sealed class ProjectFilter : ICoordinateSequenceFilter
            {
                public bool Done => false;
                public bool GeometryChanged => true;

                public void Filter(CoordinateSequence seq, int i)
                {
                    seq.SetX(i, seq.GetX(i) * MetersPerDegreeLon);
                    seq.SetY(i, seq.GetY(i) * MetersPerDegreeLat);
                }
            }
        }
    }
}
