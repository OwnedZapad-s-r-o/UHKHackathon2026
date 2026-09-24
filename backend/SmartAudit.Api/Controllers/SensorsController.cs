using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Sensors.AssignChannel;
using SmartAudit.Api.Features.Sensors.CreateGateway;
using SmartAudit.Api.Features.Sensors.DeleteGateway;
using SmartAudit.Api.Features.Sensors.GetGateway;
using SmartAudit.Api.Features.Sensors.GetGateways;
using SmartAudit.Api.Features.Sensors.PostGatewayMessage;
using SmartAudit.Core.Common;

namespace SmartAudit.Api.Controllers
{
    // Připojení skutečných čidel přes IQRF gateway — spravuje kraj a správce budovy (jen své objekty)
    [Route("api/sensors")]
    [ApiController]
    [Authorize(Roles = AppRoles.Region + "," + AppRoles.Manager)]
    public class SensorsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SensorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("gateways")]
        public async Task<IActionResult> GetGateways(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetGatewaysCommand(User.GetViewer()), ct);

            return result.ToActionResult();
        }

        [HttpGet]
        [Route("gateways/{id:guid}")]
        public async Task<IActionResult> GetGateway(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetGatewayCommand(id, User.GetViewer()), ct);

            return result.ToActionResult();
        }

        [HttpPost]
        [Route("gateways")]
        public async Task<IActionResult> CreateGateway([FromBody] CreateGatewayRequest request, CancellationToken ct)
        {
            var command = new CreateGatewayCommand(User.GetViewer(), request.FacilityId, request.Name, request.GatewayId, request.PollSeconds, request.Nodes ?? []);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpPost]
        [Route("gateways/{id:guid}/channels/{channelId:guid}/assign")]
        public async Task<IActionResult> AssignChannel(Guid id, Guid channelId, [FromBody] AssignChannelRequest request, CancellationToken ct)
        {
            var command = new AssignChannelCommand(User.GetViewer(), id, channelId, request.ZoneId, request.Metric);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpDelete]
        [Route("gateways/{id:guid}")]
        public async Task<IActionResult> DeleteGateway(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteGatewayCommand(id, User.GetViewer()), ct);

            return result.ToActionResult();
        }

        // Tělo = odpověď IQRF JSON API tak, jak ji gateway posílá do MQTT
        [HttpPost]
        [Route("iqrf/{gatewayId}/messages")]
        public async Task<IActionResult> PostGatewayMessage(string gatewayId, [FromBody] JsonElement payload, CancellationToken ct)
        {
            var command = new PostGatewayMessageCommand(User.GetViewer(), gatewayId, payload.GetRawText());

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }
    }
}
