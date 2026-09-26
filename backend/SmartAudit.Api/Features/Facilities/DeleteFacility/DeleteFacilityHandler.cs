using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Facilities.DeleteFacility
{
    public record DeleteFacilityCommand(Guid FacilityId, ViewerContext Viewer) : IRequest<Response<bool>>;

    // Správce může odebrat jen budovu, kterou sám přidal — objekty z otevřených dat kraje spravuje import.
    // Zóny, čidla, měření, gatewaye i vyhodnocení se smažou s budovou (kaskáda v databázi).
    public class DeleteFacilityHandler : IRequestHandler<DeleteFacilityCommand, Response<bool>>
    {
        private readonly SmartAuditDbContext _dbContext;

        public DeleteFacilityHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<bool>> Handle(DeleteFacilityCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities.FirstOrDefaultAsync(f => f.Id == request.FacilityId, ct);

            if (facility == null)
                return Response.NotFound<bool>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role != AppRoles.Manager || facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<bool>("K tomuto nemáte oprávnění");

            if (!facility.SourceKey.StartsWith(Facility.ManagerSourcePrefix))
                return Response.ValidationFailed<bool>("Objekt pochází z otevřených dat kraje — odebrat jde jen budovu, kterou jste přidali sami");

            _dbContext.Facilities.Remove(facility);

            await _dbContext.SaveChangesAsync(ct);

            return Response.Success();
        }
    }
}
