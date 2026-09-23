using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Facilities.GetFacilities;
using SmartAudit.Api.Features.Facilities.GetFacilityDetail;
using SmartAudit.Api.Features.Facilities.GetFootprint;
using SmartAudit.Api.Features.Facilities.GetHeatmap;
using SmartAudit.Api.Features.Facilities.GetSection;
using SmartAudit.Api.Features.Facilities.GetTelemetry;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Controllers
{
    // Čtení je veřejné — nepřihlášený návštěvník vidí data jako role Veřejnost (bez Kč)
    [Route("api/facilities")]
    [ApiController]
    public class FacilitiesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FacilitiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] string? q, [FromQuery] FacilityCategory? category, [FromQuery] string? district, [FromQuery] string? orp, [FromQuery] AuditStatus? status, [FromQuery] bool onlyIssues, CancellationToken ct)
        {
            var command = new GetFacilitiesCommand(User.GetViewer(), q, category, district, orp, status, onlyIssues);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
        {
            var command = new GetFacilityDetailCommand(id, User.GetViewer());

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpGet]
        [Route("{id:guid}/telemetry")]
        public async Task<IActionResult> GetTelemetry(Guid id, [FromQuery] SensorMetric metric, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] TelemetryBucket? bucket, CancellationToken ct)
        {
            var command = new GetTelemetryCommand(id, User.GetViewer(), metric, from, to, bucket ?? TelemetryBucket.Hour);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpGet]
        [Route("{id:guid}/heatmap")]
        public async Task<IActionResult> GetHeatmap(Guid id, [FromQuery] SensorMetric metric, CancellationToken ct)
        {
            var command = new GetHeatmapCommand(id, User.GetViewer(), metric);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        // Obrys pro 3D zvýraznění v mapě — veřejná data katastru, cache 2 dny (server i prohlížeč)
        [HttpGet]
        [Route("{id:guid}/footprint")]
        [OutputCache(PolicyName = OpenDataController.FootprintCachePolicy)]
        [ResponseCache(Duration = OpenDataController.FootprintCacheSeconds, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetFootprint(Guid id, CancellationToken ct)
        {
            var command = new GetFootprintCommand(id, Buffered: true);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpGet]
        [Route("{id:guid}/section")]
        public async Task<IActionResult> GetSection(Guid id, CancellationToken ct)
        {
            var command = new GetSectionCommand(id, User.GetViewer());

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }
    }
}
