using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Status.GetStatus;

namespace SmartAudit.Api.Controllers
{
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StatusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // /health pro Docker, /api/status pro frontend (stav přípravy dat při prvním startu)
        [HttpGet]
        [Route("health")]
        [Route("api/status")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetStatusCommand(), ct);

            return result.ToActionResult();
        }
    }
}
