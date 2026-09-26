using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Facilities.DeleteBuildingPart
{
    public record DeleteBuildingPartCommand(ViewerContext Viewer, Guid FacilityId, Guid PartId) : IRequest<Response<bool>>;

    // Odebrání doplněné přilehlé budovy — její plocha přestane vstupovat do výpočtu ztráty
    public class DeleteBuildingPartHandler : IRequestHandler<DeleteBuildingPartCommand, Response<bool>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IAuditTrigger _auditTrigger;

        public DeleteBuildingPartHandler(SmartAuditDbContext dbContext, IAuditTrigger auditTrigger)
        {
            _dbContext = dbContext;
            _auditTrigger = auditTrigger;
        }

        public async Task<Response<bool>> Handle(DeleteBuildingPartCommand request, CancellationToken ct)
        {
            var part = await _dbContext.FacilityBuildingParts
                .Include(p => p.Facility)
                .FirstOrDefaultAsync(p => p.Id == request.PartId && p.FacilityId == request.FacilityId, ct);

            if (part == null)
                return Response.NotFound<bool>("Doplněná budova s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && part.Facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<bool>("K tomuto nemáte oprávnění");

            _dbContext.FacilityBuildingParts.Remove(part);

            await _dbContext.SaveChangesAsync(ct);

            _auditTrigger.RequestFacility(request.FacilityId);

            return Response.Success();
        }
    }
}
