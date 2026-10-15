using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Facilities.AddBuildingPart;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Api.Services.Buildings;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Facilities.ReplaceBuildingPart
{
    // Drawn = obrys správce ručně upravil, jinak vybral jinou budovu z podkladové mapy
    public record ReplaceBuildingPartRequest(Geometry? Geometry, bool Drawn);

    public record ReplaceBuildingPartCommand(ViewerContext Viewer, Guid FacilityId, Guid PartId, Geometry? Geometry, bool Drawn) : IRequest<Response<AddBuildingPartDto>>;

    // Výměna doplněné budovy za jinou z mapy nebo uložení ručně upraveného obrysu — plocha a audit se přepočítají
    public class ReplaceBuildingPartHandler : IRequestHandler<ReplaceBuildingPartCommand, Response<AddBuildingPartDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IAuditTrigger _auditTrigger;

        public ReplaceBuildingPartHandler(SmartAuditDbContext dbContext, IAuditTrigger auditTrigger)
        {
            _dbContext = dbContext;
            _auditTrigger = auditTrigger;
        }

        public async Task<Response<AddBuildingPartDto>> Handle(ReplaceBuildingPartCommand request, CancellationToken ct)
        {
            var part = await _dbContext.FacilityBuildingParts
                .Include(p => p.Facility)
                .ThenInclude(f => f.Building)
                .FirstOrDefaultAsync(p => p.Id == request.PartId && p.FacilityId == request.FacilityId, ct);

            if (part == null)
                return Response.NotFound<AddBuildingPartDto>("Doplněná budova s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && part.Facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<AddBuildingPartDto>("K tomuto nemáte oprávnění");

            var cadastre = part.Facility.Building?.Footprint;
            var (checkedPart, error) = await BuildingPartRules.CheckAsync(_dbContext, part.FacilityId, cadastre ?? part.Facility.Location, cadastre, request.Geometry, part.Id, ct);
            if (checkedPart == null)
                return Response.ValidationFailed<AddBuildingPartDto>(error!);

            part.Replace(checkedPart.Footprint, checkedPart.AreaSquareMeters, request.Drawn ? AddBuildingPartHandler.DrawnSource : AddBuildingPartHandler.MapSource, DateTimeOffset.UtcNow);

            await _dbContext.SaveChangesAsync(ct);

            _auditTrigger.RequestFacility(part.FacilityId);

            return Response.Success(new AddBuildingPartDto(part.Id, part.AreaSquareMeters));
        }
    }
}
