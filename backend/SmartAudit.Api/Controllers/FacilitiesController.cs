using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Net.Http.Headers;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Facilities.AddBuildingPart;
using SmartAudit.Api.Features.Facilities.CreateFacility;
using SmartAudit.Api.Features.Facilities.DeleteBuildingPart;
using SmartAudit.Api.Features.Facilities.DeleteFacility;
using SmartAudit.Api.Features.Facilities.GetBuildingParts;
using SmartAudit.Api.Features.Facilities.GetContractAttachment;
using SmartAudit.Api.Features.Facilities.GetContracts;
using SmartAudit.Api.Features.Facilities.GetFacilities;
using SmartAudit.Api.Features.Facilities.GetFacilityDetail;
using SmartAudit.Api.Features.Facilities.GetFootprint;
using SmartAudit.Api.Features.Facilities.GetHeatmap;
using SmartAudit.Api.Features.Facilities.GetSection;
using SmartAudit.Api.Features.Facilities.GetTelemetry;
using SmartAudit.Api.Features.Facilities.LocatePlace;
using SmartAudit.Api.Features.Facilities.ReplaceBuildingPart;
using SmartAudit.Api.Features.Facilities.SuggestAddresses;
using SmartAudit.Api.Features.Facilities.UpdateBuilding;
using SmartAudit.Core.Common;
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

        /// <summary>Našeptávač adres v kraji pro přidání budovy (registr RÚIAN)</summary>
        [HttpGet]
        [Route("address-suggestions")]
        [Authorize(Roles = AppRoles.Manager)]
        public async Task<IActionResult> SuggestAddresses([FromQuery] string? q, CancellationToken ct)
        {
            var command = new SuggestAddressesCommand(q);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        /// <summary>Co víme o místě budovy: adresa, obec, ORP, okres, budova z katastru, památky, objekty v okolí</summary>
        [HttpGet]
        [Route("locate")]
        [Authorize(Roles = AppRoles.Manager)]
        public async Task<IActionResult> Locate([FromQuery] string? addressKey, [FromQuery] double? lat, [FromQuery] double? lon, CancellationToken ct)
        {
            var command = new LocatePlaceCommand(User.GetViewer(), addressKey, lat, lon);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = AppRoles.Manager)]
        public async Task<IActionResult> Create([FromBody] CreateFacilityRequest request, CancellationToken ct)
        {
            var command = new CreateFacilityCommand(
                User.GetViewer(),
                request.Name,
                request.Category,
                request.Kind,
                request.Latitude,
                request.Longitude,
                request.Street,
                request.HouseNumber,
                request.Psc,
                request.Floors,
                request.ConstructionYear);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        [HttpDelete]
        [Route("{id:guid}")]
        [Authorize(Roles = AppRoles.Manager)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var command = new DeleteFacilityCommand(id, User.GetViewer());

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
            var command = new GetFootprintCommand(id, Buffered: true, IncludeParts: true);

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

        /// <summary>Smlouvy organizace o energiích a správě budovy z Registru smluv (podle IČO)</summary>
        [HttpGet]
        [Route("{id:guid}/contracts")]
        [Authorize(Roles = AppRoles.Region + "," + AppRoles.Manager)]
        public async Task<IActionResult> GetContracts(Guid id, CancellationToken ct)
        {
            var command = new GetContractsCommand(id, User.GetViewer());

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }

        /// <summary>Originální PDF přílohy smlouvy z Registru smluv — inline, pro náhled v panelu</summary>
        [HttpGet]
        [Route("{id:guid}/contracts/{contractId:guid}/attachments/{attachmentId}")]
        [Authorize(Roles = AppRoles.Region + "," + AppRoles.Manager)]
        public async Task<IActionResult> GetContractAttachment(Guid id, Guid contractId, string attachmentId, CancellationToken ct)
        {
            var command = new GetContractAttachmentCommand(id, contractId, attachmentId, User.GetViewer());

            var result = await _mediator.Send(command, ct);

            if (!result.IsSuccess || result.Data == null)
                return result.ToActionResult();

            Response.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline") { FileNameStar = result.Data.FileName }.ToString();

            return File(result.Data.Content, "application/pdf");
        }

        // Přesné obrysy doplněných přilehlých budov — pro ruční úpravu obrysu v mapě
        [HttpGet]
        [Route("{id:guid}/building-parts")]
        public async Task<IActionResult> GetBuildingParts(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetBuildingPartsCommand(id), ct);

            return result.ToActionResult();
        }

        // Přilehlá budova, kterou katastr k objektu nevede — správce ji doplní kliknutím v mapě
        [HttpPost]
        [Route("{id:guid}/building-parts")]
        [Authorize(Roles = AppRoles.Region + "," + AppRoles.Manager)]
        public async Task<IActionResult> AddBuildingPart(Guid id, [FromBody] AddBuildingPartRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new AddBuildingPartCommand(User.GetViewer(), id, request.Geometry, request.Drawn), ct);

            return result.ToActionResult();
        }

        // Výměna doplněné budovy za jinou z mapy nebo ručně upravený obrys
        [HttpPut]
        [Route("{id:guid}/building-parts/{partId:guid}")]
        [Authorize(Roles = AppRoles.Region + "," + AppRoles.Manager)]
        public async Task<IActionResult> ReplaceBuildingPart(Guid id, Guid partId, [FromBody] ReplaceBuildingPartRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new ReplaceBuildingPartCommand(User.GetViewer(), id, partId, request.Geometry, request.Drawn), ct);

            return result.ToActionResult();
        }

        // Oprava údajů budovy pro výpočet ztráty (plocha, podlaží, rok výstavby); null = údaj z ČÚZK
        [HttpPut]
        [Route("{id:guid}/building")]
        [Authorize(Roles = AppRoles.Region + "," + AppRoles.Manager)]
        public async Task<IActionResult> UpdateBuilding(Guid id, [FromBody] UpdateBuildingRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateBuildingCommand(User.GetViewer(), id, request.BuiltUpArea, request.Floors, request.ConstructionYear), ct);

            return result.ToActionResult();
        }

        [HttpDelete]
        [Route("{id:guid}/building-parts/{partId:guid}")]
        [Authorize(Roles = AppRoles.Region + "," + AppRoles.Manager)]
        public async Task<IActionResult> DeleteBuildingPart(Guid id, Guid partId, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteBuildingPartCommand(User.GetViewer(), id, partId), ct);

            return result.ToActionResult();
        }
    }
}
