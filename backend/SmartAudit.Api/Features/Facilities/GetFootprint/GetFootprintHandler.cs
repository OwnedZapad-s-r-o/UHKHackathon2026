using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAudit.Api.Extentions;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Features.Facilities.GetFootprint
{
    public record GetFootprintCommand(Guid FacilityId, ViewerContext Viewer) : IRequest<Response<GetFootprintDto>>;

    // GeoJSON Feature — geometrii serializuje GeoJsonConverterFactory z Program.cs; bez půdorysu je Geometry null
    public record GetFootprintDto(string Type, Geometry? Geometry, FootprintPropertiesDto Properties);

    public record FootprintPropertiesDto(Guid FacilityId, AuditStatus Status, double HeightMeters, int Floors, bool HeightEstimated);

    // Půdorys vybrané budovy z ČÚZK pro 3D zobrazení; ČÚZK výšku nevede, proto odhad podlaží × 3,3 m
    public class GetFootprintHandler : IRequestHandler<GetFootprintCommand, Response<GetFootprintDto>>
    {
        private const double FloorHeightMeters = 3.3;

        private readonly SmartAuditDbContext _dbContext;

        public GetFootprintHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<GetFootprintDto>> Handle(GetFootprintCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Where(f => f.Id == request.FacilityId)
                .Select(f => new
                {
                    f.OrganizationId,
                    Footprint = f.Building != null ? f.Building.Footprint : null,
                    Floors = f.Building != null ? f.Building.EstimatedFloors : 2,
                    Status = f.Assessment != null ? f.Assessment.Status : AuditStatus.Nominal,
                })
                .FirstOrDefaultAsync(ct);

            if (facility == null)
                return Response.NotFound<GetFootprintDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<GetFootprintDto>("K tomuto nemáte oprávnění");

            var properties = new FootprintPropertiesDto(request.FacilityId, facility.Status, facility.Floors * FloorHeightMeters, facility.Floors, true);

            return Response.Success(new GetFootprintDto("Feature", facility.Footprint, properties));
        }
    }
}
