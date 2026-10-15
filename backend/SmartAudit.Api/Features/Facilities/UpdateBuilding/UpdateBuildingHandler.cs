using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Facilities.UpdateBuilding
{
    // null = vrátit údaj z ČÚZK / odhadu
    public record UpdateBuildingRequest(double? BuiltUpArea, int? Floors, int? ConstructionYear);

    public record UpdateBuildingCommand(ViewerContext Viewer, Guid FacilityId, double? BuiltUpArea, int? Floors, int? ConstructionYear) : IRequest<Response<bool>>;

    // Oprava údajů budovy pro výpočet ztráty, když ČÚZK nesedí (plocha, podlaží, rok výstavby → energetická kategorie).
    // Údaje z ČÚZK zůstávají uložené vedle — v panelu je vidět, co opravil správce.
    public class UpdateBuildingHandler : IRequestHandler<UpdateBuildingCommand, Response<bool>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IAuditTrigger _auditTrigger;
        private readonly IPreAuditService _preAudit;

        public UpdateBuildingHandler(SmartAuditDbContext dbContext, IAuditTrigger auditTrigger, IPreAuditService preAudit)
        {
            _dbContext = dbContext;
            _auditTrigger = auditTrigger;
            _preAudit = preAudit;
        }

        public async Task<Response<bool>> Handle(UpdateBuildingCommand request, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            if (request.BuiltUpArea is <= 0 or > 200_000)
                return Response.ValidationFailed<bool>("Zastavěná plocha musí být 1–200 000 m²");

            if (request.Floors is < 1 or > 40)
                return Response.ValidationFailed<bool>("Počet podlaží musí být 1–40");

            if (request.ConstructionYear is < 1000 || request.ConstructionYear > now.Year)
                return Response.ValidationFailed<bool>($"Rok výstavby musí být mezi 1000 a {now.Year}");

            var facility = await _dbContext.Facilities
                .Include(f => f.Building)
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId, ct);

            if (facility == null)
                return Response.NotFound<bool>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<bool>("K tomuto nemáte oprávnění");

            // Objekt, který ČÚZK nenašel, kartu budovy mít nemusí — opravené údaje se uloží do nové
            var building = facility.Building;
            if (building == null)
            {
                building = new FacilityBuilding(facility.Id);
                _dbContext.FacilityBuildings.Add(building);
            }

            building.SetOverrides(request.BuiltUpArea, request.Floors, request.ConstructionYear, now);

            await _dbContext.SaveChangesAsync(ct);

            // Opravená plocha nebo rok výstavby mění tepelnou obálku i hustotu osob — pre-audit se přepočítá hned
            await _preAudit.ApplyAsync([facility.Id], ct);

            _auditTrigger.RequestFacility(facility.Id);

            return Response.Success();
        }
    }
}
