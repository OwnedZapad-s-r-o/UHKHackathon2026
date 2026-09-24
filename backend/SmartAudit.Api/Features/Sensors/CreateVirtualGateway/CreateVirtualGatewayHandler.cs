using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Features.Sensors.CreateVirtualGateway
{
    // FacilityId null = první budova, kterou uživatel smí spravovat (správce: jeho škola)
    public record CreateVirtualGatewayRequest(Guid? FacilityId);

    public record CreateVirtualGatewayCommand(ViewerContext Viewer, Guid? FacilityId) : IRequest<Response<CreateVirtualGatewayDto>>;

    public record CreateVirtualGatewayDto(Guid Id, string GatewayId);

    // Jednoklikové demo bez hardwaru: virtuální „učebna“ se dvěma uzly (teplota, CO₂, vlhkost, světlo, pohyb),
    // nepřiřazená — přiřazení do místností si uživatel vyzkouší sám v průvodci
    public class CreateVirtualGatewayHandler : IRequestHandler<CreateVirtualGatewayCommand, Response<CreateVirtualGatewayDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IVirtualGatewayProvisioner _provisioner;

        public CreateVirtualGatewayHandler(SmartAuditDbContext dbContext, IVirtualGatewayProvisioner provisioner)
        {
            _dbContext = dbContext;
            _provisioner = provisioner;
        }

        public async Task<Response<CreateVirtualGatewayDto>> Handle(CreateVirtualGatewayCommand request, CancellationToken ct)
        {
            var facilities = _dbContext.Facilities.AsNoTracking();
            if (request.Viewer.Role == AppRoles.Manager)
                facilities = facilities.Where(f => f.OrganizationId == request.Viewer.OrganizationId);

            var facility = request.FacilityId is { } facilityId
                ? await facilities.Where(f => f.Id == facilityId).Select(f => new { f.Id, f.Code }).FirstOrDefaultAsync(ct)
                : await facilities.OrderBy(f => f.Category != FacilityCategory.School).ThenBy(f => f.Code).Select(f => new { f.Id, f.Code }).FirstOrDefaultAsync(ct);

            if (facility == null)
                return request.FacilityId == null
                    ? Response.NotFound<CreateVirtualGatewayDto>("Nemáte žádnou budovu, ke které by šla gateway připojit")
                    : Response.Forbidden<CreateVirtualGatewayDto>("K tomuto objektu nemáte oprávnění");

            var device = await _provisioner.CreateClassroomAsync(facility.Id, facility.Code, ct);

            return Response.Success(new CreateVirtualGatewayDto(device.Id, device.GatewayId));
        }
    }
}
