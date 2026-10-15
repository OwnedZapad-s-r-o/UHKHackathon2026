using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Api.Services.Buildings;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Facilities.AddBuildingPart
{
    // Tělo požadavku: GeoJSON geometrie budovy (deserializuje GeoJsonConverterFactory z Program.cs).
    // Drawn = obrys nakreslil správce ručně, jinak je to budova z podkladové mapy (OpenStreetMap).
    public record AddBuildingPartRequest(Geometry? Geometry, bool Drawn);

    public record AddBuildingPartCommand(ViewerContext Viewer, Guid FacilityId, Geometry? Geometry, bool Drawn) : IRequest<Response<AddBuildingPartDto>>;

    public record AddBuildingPartDto(Guid Id, double AreaSquareMeters);

    // Správce doplní k objektu přilehlou budovu, kterou katastr ČÚZK nevede (křídlo zapsané zvlášť nebo chybějící v INSPIRE).
    // Plocha se přičte do výpočtu ztráty a audit se přepočítá.
    public class AddBuildingPartHandler : IRequestHandler<AddBuildingPartCommand, Response<AddBuildingPartDto>>
    {
        public const string MapSource = "OpenStreetMap — doplnil správce";
        public const string DrawnSource = "Obrys upravil správce";

        private readonly SmartAuditDbContext _dbContext;
        private readonly IAuditTrigger _auditTrigger;

        public AddBuildingPartHandler(SmartAuditDbContext dbContext, IAuditTrigger auditTrigger)
        {
            _dbContext = dbContext;
            _auditTrigger = auditTrigger;
        }

        public async Task<Response<AddBuildingPartDto>> Handle(AddBuildingPartCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Where(f => f.Id == request.FacilityId)
                .Select(f => new { f.Id, f.OrganizationId, f.Location, Footprint = f.Building != null ? f.Building.Footprint : null })
                .FirstOrDefaultAsync(ct);

            if (facility == null)
                return Response.NotFound<AddBuildingPartDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<AddBuildingPartDto>("K tomuto nemáte oprávnění");

            var (checkedPart, error) = await BuildingPartRules.CheckAsync(_dbContext, facility.Id, facility.Footprint ?? facility.Location, facility.Footprint, request.Geometry, null, ct);
            if (checkedPart == null)
                return Response.ValidationFailed<AddBuildingPartDto>(error!);

            var part = new FacilityBuildingPart(facility.Id, checkedPart.Footprint, checkedPart.AreaSquareMeters, request.Drawn ? DrawnSource : MapSource);
            _dbContext.FacilityBuildingParts.Add(part);

            await _dbContext.SaveChangesAsync(ct);

            _auditTrigger.RequestFacility(facility.Id);

            return Response.Success(new AddBuildingPartDto(part.Id, part.AreaSquareMeters));
        }
    }
}
