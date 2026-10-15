using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Sensors.CreateGateway
{
    // Tělo požadavku — přihlášeného uživatele doplní controller
    public record CreateGatewayRequest(Guid FacilityId, string Name, string GatewayId, int PollSeconds = 15, int[]? Nodes = null);

    public record CreateGatewayCommand(ViewerContext Viewer, Guid FacilityId, string Name, string GatewayId, int PollSeconds, int[] Nodes) : IRequest<Response<CreateGatewayDto>>;

    public record CreateGatewayDto(Guid Id);

    public class CreateGatewayHandler : IRequestHandler<CreateGatewayCommand, Response<CreateGatewayDto>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IIqrfGatewayState _state;

        public CreateGatewayHandler(SmartAuditDbContext dbContext, IIqrfGatewayState state)
        {
            _dbContext = dbContext;
            _state = state;
        }

        public async Task<Response<CreateGatewayDto>> Handle(CreateGatewayCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Response.ValidationFailed<CreateGatewayDto>("Zadejte název gatewaye");

            var gatewayId = request.GatewayId?.Trim() ?? string.Empty;
            if (gatewayId.Length == 0 || gatewayId.Length > 64 || gatewayId.Any(c => c is '/' or '+' or '#' || char.IsWhiteSpace(c)))
                return Response.ValidationFailed<CreateGatewayDto>("Id gatewaye musí být 1–64 znaků bez mezer a znaků / + #");

            if (request.PollSeconds < 5 || request.PollSeconds > 3600)
                return Response.ValidationFailed<CreateGatewayDto>("Interval dotazování musí být mezi 5 s a 1 h");

            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Select(f => new { f.Id, f.OrganizationId })
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId, ct);

            if (facility == null)
                return Response.NotFound<CreateGatewayDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<CreateGatewayDto>("K tomuto nemáte oprávnění");

            if (await _dbContext.SensorDevices.AnyAsync(d => d.GatewayId == gatewayId, ct))
                return Response.ValidationFailed<CreateGatewayDto>("Gateway s tímto id už je připojená");

            var device = new SensorDevice(facility.Id, request.Name, gatewayId, request.PollSeconds, request.Nodes ?? []);
            _dbContext.SensorDevices.Add(device);

            await _dbContext.SaveChangesAsync(ct);

            _state.ForgetUnknown(gatewayId);

            return Response.Success(new CreateGatewayDto(device.Id));
        }
    }
}
