using MediatR;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Sensors.CreateDemoNetwork
{
    public record CreateDemoNetworkCommand(ViewerContext Viewer) : IRequest<Response<CreateDemoNetworkDto>>;

    public record CreateDemoNetworkDto(int Created, IReadOnlyCollection<ProvisionedGateway> Gateways);

    // Testovací síť kraje jedním klikem: pět virtuálních gatewayí v pěti okresech (katalog VirtualGateways.Network).
    // Opakované spuštění nic nezdvojí — existující gatewaye se jen vrátí.
    public class CreateDemoNetworkHandler : IRequestHandler<CreateDemoNetworkCommand, Response<CreateDemoNetworkDto>>
    {
        private readonly IVirtualGatewayProvisioner _provisioner;

        public CreateDemoNetworkHandler(IVirtualGatewayProvisioner provisioner)
        {
            _provisioner = provisioner;
        }

        public async Task<Response<CreateDemoNetworkDto>> Handle(CreateDemoNetworkCommand request, CancellationToken ct)
        {
            if (request.Viewer.Role != AppRoles.Region)
                return Response.Forbidden<CreateDemoNetworkDto>("Testovací síť celého kraje může spustit jen kraj");

            var gateways = new List<ProvisionedGateway>();
            foreach (var preset in VirtualGateways.Network)
            {
                if (await _provisioner.EnsurePresetAsync(preset, ct) is { } gateway)
                    gateways.Add(gateway);
            }

            if (gateways.Count == 0)
                return Response.NotFound<CreateDemoNetworkDto>("Budovy testovací sítě v portfoliu nejsou — je import dat hotový?");

            return Response.Success(new CreateDemoNetworkDto(gateways.Count(g => g.Created), gateways));
        }
    }
}
