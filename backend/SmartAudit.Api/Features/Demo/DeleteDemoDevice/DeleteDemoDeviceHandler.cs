using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Demo.DeleteDemoDevice
{
    public record DeleteDemoDeviceCommand(ViewerContext Viewer, Guid Id) : IRequest<Response<bool>>;

    // Vypnutí demo zařízení, které ještě nikdo nespároval. Spárovanou gateway je potřeba nejdřív odebrat na stránce Senzory.
    public class DeleteDemoDeviceHandler : IRequestHandler<DeleteDemoDeviceCommand, Response<bool>>
    {
        private readonly SmartAuditDbContext _dbContext;
        private readonly IPairingRegistry _registry;

        public DeleteDemoDeviceHandler(SmartAuditDbContext dbContext, IPairingRegistry registry)
        {
            _dbContext = dbContext;
            _registry = registry;
        }

        public async Task<Response<bool>> Handle(DeleteDemoDeviceCommand request, CancellationToken ct)
        {
            var device = await _dbContext.DemoDevices.FirstOrDefaultAsync(d => d.Id == request.Id, ct);

            if (device == null)
                return Response.NotFound<bool>("Zařízení s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && device.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<bool>("K tomuto nemáte oprávnění");

            if (device.Kind == DemoDeviceKind.Gateway)
            {
                var gatewayId = PairableDevices.GatewayIdOf(device.Serial);
                if (await _dbContext.SensorDevices.AnyAsync(d => d.GatewayId == gatewayId, ct))
                    return Response.ValidationFailed<bool>("Gateway je spárovaná. Nejdřív ji odeberte na stránce Senzory.");
            }
            else if (await _dbContext.SensorDevices.AnyAsync(d => d.GatewayId == device.ParentGatewayId && d.NodeAddresses.Contains(device.NodeAddress!.Value), ct))
            {
                return Response.ValidationFailed<bool>("Čidlo už je v síti gatewaye a odebrat nejde.");
            }

            _dbContext.DemoDevices.Remove(device);
            await _dbContext.SaveChangesAsync(ct);

            await DemoDeviceCatalog.ReloadAsync(_dbContext, ct);
            _registry.Forget(device.Serial);

            return Response.Success();
        }
    }
}
