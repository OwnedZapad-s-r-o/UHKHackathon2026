using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Demo.CreateDemoGateway;
using SmartAudit.Api.Features.Demo.CreateDemoNode;
using SmartAudit.Api.Features.Demo.DeleteDemoDevice;
using SmartAudit.Api.Features.Demo.GetDemoOverview;
using SmartAudit.Core.Common;

namespace SmartAudit.Api.Controllers
{
    // Demo správa: výroba virtuálních gatewayí a čidel k párování a přehled, jak čidla opravila odhad ztráty
    [Route("api/demo")]
    [ApiController]
    [Authorize(Roles = AppRoles.Region + "," + AppRoles.Manager)]
    public class DemoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DemoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetOverview(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDemoOverviewCommand(User.GetViewer()), ct);

            return result.ToActionResult();
        }

        [HttpPost]
        [Route("gateways")]
        public async Task<IActionResult> CreateGateway([FromBody] CreateDemoGatewayRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateDemoGatewayCommand(User.GetViewer(), request.Templates ?? [], request.Behavior), ct);

            return result.ToActionResult();
        }

        [HttpPost]
        [Route("nodes")]
        public async Task<IActionResult> CreateNode([FromBody] CreateDemoNodeRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateDemoNodeCommand(User.GetViewer(), request.GatewayId, request.Template ?? string.Empty, request.Behavior), ct);

            return result.ToActionResult();
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteDemoDeviceCommand(User.GetViewer(), id), ct);

            return result.ToActionResult();
        }
    }
}
