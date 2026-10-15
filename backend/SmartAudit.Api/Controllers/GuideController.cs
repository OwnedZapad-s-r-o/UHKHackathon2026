using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Guide.AskGuide;
using SmartAudit.Api.Services.Guide;

namespace SmartAudit.Api.Controllers
{
    // Průvodce SmartAuditu — pro přihlášené i veřejnost; co smí vidět, určuje role z tokenu (bez tokenu = veřejnost)
    [Route("api/guide")]
    [ApiController]
    public class GuideController : ControllerBase
    {
        // Průvodce může volat placený model — nejvýš 15 dotazů za minutu na uživatele nebo IP adresu
        public const string RateLimitPolicy = "Guide";
        public const int RateLimitRequests = 15;
        public static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

        private readonly IMediator _mediator;

        public GuideController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("ask")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicy)]
        public async Task<IActionResult> Ask([FromBody] AskGuideRequest request, CancellationToken ct)
        {
            var context = new GuideAskContext(request.Context?.Page, request.Context?.FacilityId, request.Context?.DistrictCode);
            var command = new AskGuideCommand(request.Question ?? string.Empty, context, request.History ?? [], User.GetViewer());

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }
    }
}
