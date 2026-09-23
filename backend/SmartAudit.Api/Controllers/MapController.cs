using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Map.GetRegions;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Controllers
{
    [Route("api/map")]
    [ApiController]
    public class MapController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MapController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("regions")]
        public async Task<IActionResult> GetRegions([FromQuery] RegionType type, [FromQuery] bool onlyThreatened, CancellationToken ct)
        {
            var command = new GetRegionsCommand(type, onlyThreatened);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }
    }
}
