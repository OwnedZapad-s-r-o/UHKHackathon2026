using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Demo.CreateDemoGateway;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Demo.CreateDemoNode
{
    public record CreateDemoNodeRequest(Guid GatewayId, string? Template, DemoBehavior Behavior);

    public record CreateDemoNodeCommand(ViewerContext Viewer, Guid GatewayId, string Template, DemoBehavior Behavior) : IRequest<Response<DemoDeviceCreatedDto>>;

    // „Položí“ nové čidlo do dosahu virtuální gatewaye: objeví se v Čidlech v dosahu na detailu gatewaye
    // a do sítě se přidá kódem z nálepky (IQRF SmartConnect)
    public class CreateDemoNodeHandler : IRequestHandler<CreateDemoNodeCommand, Response<DemoDeviceCreatedDto>>
    {
        private readonly SmartAuditDbContext _dbContext;

        public CreateDemoNodeHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<DemoDeviceCreatedDto>> Handle(CreateDemoNodeCommand request, CancellationToken ct)
        {
            if (DemoNodeTemplates.Find(request.Template) == null)
                return Response.ValidationFailed<DemoDeviceCreatedDto>("Neznámý druh čidla");

            if (!Enum.IsDefined(request.Behavior))
                return Response.ValidationFailed<DemoDeviceCreatedDto>("Neznámé chování zařízení");

            var gateway = await _dbContext.SensorDevices
                .AsNoTracking()
                .Select(d => new { d.Id, d.GatewayId, d.IsVirtual, d.NodeAddresses, d.Facility.OrganizationId })
                .FirstOrDefaultAsync(d => d.Id == request.GatewayId, ct);

            if (gateway == null)
                return Response.NotFound<DemoDeviceCreatedDto>("Gateway s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && gateway.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<DemoDeviceCreatedDto>("K tomuto nemáte oprávnění");

            // Za skutečnou gateway (hardware, Python emulátor) aplikace neodpovídá — čidlo by do její sítě nikdy nepřišlo
            if (!gateway.IsVirtual)
                return Response.ValidationFailed<DemoDeviceCreatedDto>("Čidlo z demo správy jde položit jen k virtuální gatewayi");

            // Adresa = první volná za uzly gatewaye a čidly, která už jsou v jejím dosahu
            await DemoDeviceCatalog.ReloadAsync(_dbContext, ct);
            var taken = VirtualGateways.NodesOf(gateway.GatewayId, gateway.NodeAddresses).Select(n => n.Address)
                .Concat(gateway.NodeAddresses)
                .Concat(PairableDevices.NearbyOf(gateway.GatewayId).Select(n => n.Node.Address))
                .ToList();
            var address = taken.Count == 0 ? 1 : taken.Max() + 1;

            if (address > 239)
                return Response.ValidationFailed<DemoDeviceCreatedDto>("Síť gatewaye je plná");

            var serial = await DemoSerials.NewAsync(_dbContext, ct);
            var organizationId = request.Viewer.Role == AppRoles.Manager ? request.Viewer.OrganizationId : null;
            var node = DemoDevice.Node(serial, DemoNodeTemplates.NewCode(), request.Template, request.Behavior, gateway.GatewayId, address, organizationId);

            _dbContext.DemoDevices.Add(node);
            await _dbContext.SaveChangesAsync(ct);

            await DemoDeviceCatalog.ReloadAsync(_dbContext, ct);

            return Response.Success(new DemoDeviceCreatedDto(node.Id, node.Serial, node.Code));
        }
    }
}
