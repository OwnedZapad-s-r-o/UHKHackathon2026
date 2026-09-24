using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAudit.Api.Features.Map.GetRegions;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services.Reports
{
    public record MapDistrict(string Code, string Name, Geometry Geometry, int Total, int Issues)
    {
        public double IssueShare => Total == 0 ? 0 : (double)Issues / Total;
    }

    public record MapPoint(double Longitude, double Latitude, AuditStatus Status);

    // Okresy kraje s podílem objektů se zjištěním — vždy za celý kraj, aby barvy seděly s mapou ve webu i při filtru okresu
    public interface IDistrictMapSource
    {
        Task<IReadOnlyList<MapDistrict>> GetDistrictsAsync(CancellationToken ct);
    }

    public class DistrictMapSource : IDistrictMapSource
    {
        private readonly SmartAuditDbContext _dbContext;

        public DistrictMapSource(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<MapDistrict>> GetDistrictsAsync(CancellationToken ct)
        {
            var regions = await _dbContext.Regions
                .AsNoTracking()
                .Where(r => r.Type == RegionType.Okres && r.Geometry != null)
                .OrderBy(r => r.Name)
                .ToListAsync(ct);

            var counts = await _dbContext.Facilities
                .AsNoTracking()
                .GroupBy(f => f.DistrictCode)
                .Select(g => new
                {
                    Code = g.Key,
                    Total = g.Count(),
                    Issues = g.Count(f => f.Assessment != null && f.Assessment.Status != AuditStatus.Nominal),
                })
                .ToDictionaryAsync(c => c.Code, ct);

            return regions
                .Select(r => new MapDistrict(
                    r.Code,
                    r.Name,
                    GetRegionsHandler.Simplify(r.Geometry)!,
                    counts.GetValueOrDefault(r.Code)?.Total ?? 0,
                    counts.GetValueOrDefault(r.Code)?.Issues ?? 0))
                .ToList();
        }
    }
}
