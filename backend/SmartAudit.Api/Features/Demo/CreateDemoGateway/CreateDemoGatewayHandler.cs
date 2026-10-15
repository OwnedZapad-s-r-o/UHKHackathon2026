using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Demo.CreateDemoGateway
{
    public record CreateDemoGatewayRequest(string[]? Templates, DemoBehavior Behavior);

    public record CreateDemoGatewayCommand(ViewerContext Viewer, string[] Templates, DemoBehavior Behavior) : IRequest<Response<DemoDeviceCreatedDto>>;

    public record DemoDeviceCreatedDto(Guid Id, string Serial, string Code);

    // „Vyrobí“ novou gateway: dostane MID a heslo na nálepku a do 10 s se začne ohlašovat v Nových zařízeních online
    public class CreateDemoGatewayHandler : IRequestHandler<CreateDemoGatewayCommand, Response<DemoDeviceCreatedDto>>
    {
        private readonly SmartAuditDbContext _dbContext;

        public CreateDemoGatewayHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<DemoDeviceCreatedDto>> Handle(CreateDemoGatewayCommand request, CancellationToken ct)
        {
            if (request.Templates.Length is < 1 or > 3)
                return Response.ValidationFailed<DemoDeviceCreatedDto>("Vyberte 1 až 3 uzly, které gateway bude mít");

            if (request.Templates.Any(t => DemoNodeTemplates.Find(t) == null))
                return Response.ValidationFailed<DemoDeviceCreatedDto>("Neznámý druh uzlu");

            if (!Enum.IsDefined(request.Behavior))
                return Response.ValidationFailed<DemoDeviceCreatedDto>("Neznámé chování zařízení");

            var serial = await DemoSerials.NewAsync(_dbContext, ct);
            var organizationId = request.Viewer.Role == AppRoles.Manager ? request.Viewer.OrganizationId : null;
            var device = DemoDevice.Gateway(serial, DemoNodeTemplates.GatewayModel, DemoNodeTemplates.NewCode(), request.Templates, request.Behavior, organizationId);

            _dbContext.DemoDevices.Add(device);
            await _dbContext.SaveChangesAsync(ct);

            await DemoDeviceCatalog.ReloadAsync(_dbContext, ct);

            return Response.Success(new DemoDeviceCreatedDto(device.Id, device.Serial, device.Code));
        }
    }

    // MID, který ještě nikdo nemá — ani v pevném katalogu, ani v demo správě
    public static class DemoSerials
    {
        public static async Task<string> NewAsync(SmartAuditDbContext dbContext, CancellationToken ct)
        {
            while (true)
            {
                var serial = DemoNodeTemplates.NewSerial();
                if (PairableDevices.FindGateway(serial) == null && !await dbContext.DemoDevices.AnyAsync(d => d.Serial == serial, ct))
                    return serial;
            }
        }
    }
}
