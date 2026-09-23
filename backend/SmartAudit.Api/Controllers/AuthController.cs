using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Auth.DemoLogin;
using SmartAudit.Api.Features.Auth.Login;

namespace SmartAudit.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpPost]
        [Route("demo")]
        public async Task<IActionResult> Demo([FromBody] DemoLoginCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }
    }
}
