using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.AiAudit.GenerateAiAudit;
using SmartAudit.Api.Features.AiAudit.GetLatestAiAudit;
using SmartAudit.Core.Common;

namespace SmartAudit.Api.Controllers
{
    // AI analýza obsahuje Kč a návrh dopisu — jen pro kraj a správce budovy
    [Route("api/facilities/{id:guid}/ai-audit")]
    [ApiController]
    public class AiAuditController : ControllerBase
    {
        private const string AllowedRoles = AppRoles.Region + "," + AppRoles.Manager;

        // Nejvýš 5 generování za 10 minut na uživatele; opakovaný dotaz bez změny stavu budovy obslouží mezipaměť v handleru
        public const string RateLimitPolicy = "AiAudit";
        public const int RateLimitRequests = 5;
        public static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(10);

        private readonly IMediator _mediator;

        public AiAuditController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        [Authorize(Roles = AllowedRoles)]
        public async Task<IActionResult> GetLatest(Guid id, CancellationToken ct)
        {
            var command = new GetLatestAiAuditCommand(id, User.GetViewer());

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = AllowedRoles)]
        [EnableRateLimiting(RateLimitPolicy)]
        public async Task<IActionResult> Generate(Guid id, CancellationToken ct)
        {
            if (!User.TryGetUserId(out var userId))
                return Unauthorized("Neplatný token");

            var command = new GenerateAiAuditCommand(id, userId, User.GetViewer());

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }
    }
}
