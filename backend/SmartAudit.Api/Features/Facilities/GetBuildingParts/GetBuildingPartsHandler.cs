using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Facilities.GetBuildingParts
{
    public record GetBuildingPartsCommand(Guid FacilityId) : IRequest<Response<List<BuildingPartGeometryDto>>>;

    // Přesný (nerozšířený) obrys doplněné budovy — pro ruční úpravu obrysu v mapě
    public record BuildingPartGeometryDto(Guid Id, Geometry Footprint, double AreaSquareMeters, string Source);

    // Obrysy budov jsou veřejné jako katastr — bez kontroly role (upravit je může jen kraj a správce)
    public class GetBuildingPartsHandler : IRequestHandler<GetBuildingPartsCommand, Response<List<BuildingPartGeometryDto>>>
    {
        private readonly SmartAuditDbContext _dbContext;

        public GetBuildingPartsHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<List<BuildingPartGeometryDto>>> Handle(GetBuildingPartsCommand request, CancellationToken ct)
        {
            if (!await _dbContext.Facilities.AnyAsync(f => f.Id == request.FacilityId, ct))
                return Response.NotFound<List<BuildingPartGeometryDto>>("Objekt s tímto id neexistuje");

            var parts = await _dbContext.FacilityBuildingParts
                .AsNoTracking()
                .Where(p => p.FacilityId == request.FacilityId)
                .OrderBy(p => p.AddedAt)
                .Select(p => new BuildingPartGeometryDto(p.Id, p.Footprint, p.AreaSquareMeters, p.Source))
                .ToListAsync(ct);

            return Response.Success(parts);
        }
    }
}
