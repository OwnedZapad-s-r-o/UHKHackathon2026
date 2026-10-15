using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
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

            var features = await query
                .OrderBy(r => r.Name)
                .Select(r => new RegionFeatureDto(
                    "Feature",
                    r.Geometry,
                    new RegionPropertiesDto(r.Code, r.Name, r.Type, r.IsThreatened, r.ThreatLevel, r.GrantCount, r.GrantTotal)))
                .ToListAsync(ct);

            return Response.Success(new GetRegionsDto("FeatureCollection", features));
        }
    }
}
