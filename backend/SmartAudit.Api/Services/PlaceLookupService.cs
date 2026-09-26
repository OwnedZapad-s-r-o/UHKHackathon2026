using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;
using SmartAudit.Api.Services.Import;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services
{
    public record PlaceTerritory(string MunicipalityCode, string MunicipalityName, string OrpCode, string OrpName, string DistrictCode, string DistrictName);

    public record PlaceNeighbor(Guid Id, string Code, string Name, Guid? OrganizationId, double DistanceMeters, bool SameBuilding);

    public record PlaceContext(Point Location, PlaceTerritory? Territory, CuzkBuilding? Building, bool CadastreAvailable, HeritageStatus HeritageStatus, string? HeritageName, IReadOnlyList<PlaceNeighbor> Neighbors);

    public interface IPlaceLookupService
    {
        Task<PlaceContext> LookupAsync(Point location, CancellationToken ct);
    }

    // Co o jednom místě v kraji víme z dat, která už aplikace má: obec, ORP a okres (hranice z otevřených dat KHK),
    // budova z katastru ČÚZK, památková ochrana a objekty portfolia v okolí (aby správce nepřidal budovu podruhé).
    // Stejný výpočet používá náhled ve formuláři i uložení — co správce viděl, to se uloží.
    public class PlaceLookupService : IPlaceLookupService
    {
        private const string HeritageCacheKey = "place-lookup:heritage";

        // Objekty portfolia do ~80 m se ukážou jako „v okolí už je“
        private const double NeighborRadiusDegrees = 0.0011;
        private const double NeighborRadiusMeters = 80;

        private readonly SmartAuditDbContext _dbContext;
        private readonly ICuzkBuildingService _cuzkService;
        private readonly IOpenDataImportService _importService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PlaceLookupService> _logger;

        public PlaceLookupService(SmartAuditDbContext dbContext, ICuzkBuildingService cuzkService, IOpenDataImportService importService, IMemoryCache cache, ILogger<PlaceLookupService> logger)
        {
            _dbContext = dbContext;
            _cuzkService = cuzkService;
            _importService = importService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PlaceContext> LookupAsync(Point location, CancellationToken ct)
        {
            // Katastr je vzdálená služba — běží souběžně s dotazy do databáze
            var buildingTask = FindBuildingAsync(location, ct);

            var territory = await TerritoryAsync(location, ct);
            var heritage = (await HeritageAsync(ct)).Match(location);
            var (building, cadastreAvailable) = await buildingTask;
            var neighbors = await NeighborsAsync(location, building?.RuianCode, ct);

            return new PlaceContext(location, territory, building, cadastreAvailable, heritage.Status, heritage.Name, neighbors);
        }

        private async Task<PlaceTerritory?> TerritoryAsync(Point location, CancellationToken ct)
        {
            var municipality = await _dbContext.Regions
                .AsNoTracking()
                .Where(r => r.Type == RegionType.Obec && r.Geometry != null && r.Geometry.Contains(location))
                .Select(r => new { r.Code, r.Name, r.OrpCode })
                .FirstOrDefaultAsync(ct);

            if (municipality == null)
                return null;

            var orp = await _dbContext.Regions
                .AsNoTracking()
                .Where(r => r.Type == RegionType.Orp && r.Code == municipality.OrpCode)
                .Select(r => new { r.Code, r.Name })
                .FirstOrDefaultAsync(ct);

            var district = await _dbContext.Regions
                .AsNoTracking()
                .Where(r => r.Type == RegionType.Okres && r.Geometry != null && r.Geometry.Contains(location))
                .Select(r => new { r.Code, r.Name })
                .FirstOrDefaultAsync(ct);

            return new PlaceTerritory(municipality.Code, municipality.Name, orp?.Code ?? municipality.OrpCode ?? string.Empty, orp?.Name ?? string.Empty, district?.Code ?? string.Empty, district?.Name ?? string.Empty);
        }

        // Výpadek ČÚZK nesmí zablokovat přidání budovy — bez katastru se jen neukáže půdorys a odhad podlaží je obecný
        private async Task<(CuzkBuilding? Building, bool Available)> FindBuildingAsync(Point location, CancellationToken ct)
        {
            try
            {
                return (await _cuzkService.FindBuildingAsync(location, ct), true);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Xml.XmlException)
            {
                _logger.LogWarning(ex, "ČÚZK nevrátil budovu pro bod {X}, {Y}", location.X, location.Y);
                return (null, false);
            }
        }

        // Sady památek se mění jen s importem — v paměti na hodinu, ať posouvání špendlíku nečte pokaždé tisíce polygonů
        private async Task<HeritageIndex> HeritageAsync(CancellationToken ct)
        {
            return (await _cache.GetOrCreateAsync(HeritageCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return HeritageIndex.LoadAsync(_importService, ct);
            }))!;
        }

        private async Task<IReadOnlyList<PlaceNeighbor>> NeighborsAsync(Point location, string? ruianCode, CancellationToken ct)
        {
            var nearby = await _dbContext.Facilities
                .AsNoTracking()
                .Where(f => f.Location.IsWithinDistance(location, NeighborRadiusDegrees)
                    || (ruianCode != null && f.Building != null && f.Building.RuianCode == ruianCode))
                .Select(f => new
                {
                    f.Id,
                    f.Code,
                    f.Name,
                    f.OrganizationId,
                    f.Location,
                    RuianCode = f.Building != null ? f.Building.RuianCode : null,
                })
                .ToListAsync(ct);

            return nearby
                .Select(f => new PlaceNeighbor(f.Id, f.Code, f.Name, f.OrganizationId, Math.Round(DistanceMeters(f.Location, location)), ruianCode != null && f.RuianCode == ruianCode))
                .Where(f => f.SameBuilding || f.DistanceMeters <= NeighborRadiusMeters)
                .OrderByDescending(f => f.SameBuilding)
                .ThenBy(f => f.DistanceMeters)
                .Take(5)
                .ToList();
        }

        private static double DistanceMeters(Point a, Point b)
        {
            var dLat = (a.Y - b.Y) * 111_320;
            var dLon = (a.X - b.X) * 111_320 * Math.Cos(b.Y * Math.PI / 180);

            return Math.Sqrt(dLat * dLat + dLon * dLon);
        }
    }
}
