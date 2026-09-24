using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;
using NetTopologySuite.Simplify;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Features.Map.GetRegions
{
    public record GetRegionsCommand(RegionType Type, bool OnlyThreatened = false) : IRequest<Response<GetRegionsDto>>;

    // GeoJSON FeatureCollection — geometrie serializuje GeoJsonConverterFactory z Program.cs
    public record GetRegionsDto(string Type, IReadOnlyCollection<RegionFeatureDto> Features);

    public record RegionFeatureDto(string Type, Geometry? Geometry, RegionPropertiesDto Properties);

    public record RegionPropertiesDto(string Code, string Name, RegionType RegionType, bool IsThreatened, string? ThreatLevel, int? GrantCount, decimal? GrantTotal);

    public class GetRegionsHandler : IRequestHandler<GetRegionsCommand, Response<GetRegionsDto>>
    {
        // ~60 m — hranice okresu na mapě kraje ani v PDF nepozná rozdíl, data se zmenší z ~300 KB na desítky KB
        private const double SimplifyToleranceDegrees = 0.0008;

        // Souřadnice na 5 desetinných míst (~1 m) — víc je v JSONu jen šum
        private static readonly PrecisionModel FiveDecimals = new(100_000);

        private readonly SmartAuditDbContext _dbContext;

        public GetRegionsHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<GetRegionsDto>> Handle(GetRegionsCommand request, CancellationToken ct)
        {
            var query = _dbContext.Regions
                .AsNoTracking()
                .Where(r => r.Type == request.Type);

            if (request.OnlyThreatened)
                query = query.Where(r => r.IsThreatened);

            var regions = await query
                .OrderBy(r => r.Name)
                .ToListAsync(ct);

            var features = regions
                .Select(r => new RegionFeatureDto(
                    "Feature",
                    Simplify(r.Geometry),
                    new RegionPropertiesDto(r.Code, r.Name, r.Type, r.IsThreatened, r.ThreatLevel, r.GrantCount, r.GrantTotal)))
                .ToList();

            return Response.Success(new GetRegionsDto("FeatureCollection", features));
        }

        public static Geometry? Simplify(Geometry? geometry)
        {
            return geometry == null ? null : GeometryPrecisionReducer.Reduce(TopologyPreservingSimplifier.Simplify(geometry, SimplifyToleranceDegrees), FiveDecimals);
        }
    }
}
