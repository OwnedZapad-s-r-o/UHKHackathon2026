using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.DataSources.GetDataSources;
using SmartAudit.Api.Features.Facilities.GetFacilityDetail;
using SmartAudit.Api.Features.Facilities.GetFootprint;
using SmartAudit.Api.Features.Facilities.GetTelemetry;
using SmartAudit.Api.Features.OpenData.GetOpenFacilities;
using SmartAudit.Api.Features.OpenData.GetOpenPreAudits;
using SmartAudit.Api.Services.OpenData;
using SmartAudit.Core.Common;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Controllers
{
    // Otevřené API (CC BY 4.0) — bez přihlášení, CORS pro libovolný web, data jako role Veřejnost (bez Kč)
    [Route("api/open/v1")]
    [ApiController]
    [AllowAnonymous]
    [EnableCors(CorsPolicy)]
    [ApiExplorerSettings(GroupName = DocumentName)]
    [Tags("Otevřená data")]
    public class OpenDataController : ControllerBase
    {
        public const string DocumentName = "v1";
        public const string CorsPolicy = "OpenData";

        // Stav objektů se přepočítává po pár minutách, půdorys z katastru se prakticky nemění
        public const string ListCachePolicy = "OpenDataList";
        public const string FootprintCachePolicy = "Footprint";
        public const int ListCacheSeconds = 5 * 60;
        public const int FootprintCacheSeconds = 2 * 24 * 60 * 60;

        private static readonly ViewerContext Public = new(AppRoles.Public, null);

        private readonly IMediator _mediator;
        private readonly OpenDataSettings _settings;

        public OpenDataController(IMediator mediator, OpenDataSettings settings)
        {
            _mediator = mediator;
            _settings = settings;
        }

        /// <summary>Lokální katalog otevřených dat (DCAT-AP-CZ, JSON-LD) — pro registraci v Národním katalogu otevřených dat</summary>
        [HttpGet]
        [Route("catalog")]
        [Produces(DcatCatalog.ContentType)]
        public IActionResult GetCatalog()
        {
            return Content(DcatCatalog.Catalog(PublicBaseUrl(), _settings.CatalogProviderIri), DcatCatalog.ContentType);
        }

        /// <summary>Záznam datové sady v katalogu (DCAT-AP-CZ, JSON-LD) — distribuce CSV, GeoJSON a JSON s podmínkami užití</summary>
        /// <param name="dataset">Identifikátor datové sady z katalogu</param>
        [HttpGet]
        [Route("catalog/datasets/{dataset}")]
        [Produces(DcatCatalog.ContentType)]
        public IActionResult GetCatalogDataset(string dataset)
        {
            return dataset switch
            {
                DcatCatalog.BuildingsDataset => Content(DcatCatalog.BuildingsDatasetRecord(PublicBaseUrl(), _settings.CatalogProviderIri), DcatCatalog.ContentType),
                DcatCatalog.PreAuditDataset => Content(DcatCatalog.PreAuditDatasetRecord(PublicBaseUrl(), _settings.CatalogProviderIri), DcatCatalog.ContentType),
                _ => NotFound(new { Message = "Datová sada s tímto identifikátorem v katalogu není" }),
            };
        }

        // IRI v katalogu musí být veřejné adresy — za proxy Next.js je API na interní adrese, veřejnou nese X-Forwarded-*
        private string PublicBaseUrl()
        {
            if (!string.IsNullOrWhiteSpace(_settings.PublicBaseUrl))
                return _settings.PublicBaseUrl.TrimEnd('/');

            var host = Request.Headers["X-Forwarded-Host"].FirstOrDefault()?.Split(',')[0].Trim() ?? Request.Host.Value;
            var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault()?.Split(',')[0].Trim() ?? Request.Scheme;

            return $"{scheme}://{host}";
        }

        /// <summary>Seznam objektů kraje (JSON)</summary>
        /// <param name="q">Hledání v názvu, kódu a obci</param>
        /// <param name="category">Druh objektu</param>
        /// <param name="district">Kód okresu (CZ0521 Hradec Králové, CZ0522 Jičín, CZ0523 Náchod, CZ0524 Rychnov n. Kn., CZ0525 Trutnov)</param>
        /// <param name="status">Stav auditu</param>
        [HttpGet]
        [Route("facilities")]
        [OutputCache(PolicyName = ListCachePolicy)]
        [ResponseCache(Duration = ListCacheSeconds, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType<IReadOnlyCollection<OpenFacilityDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFacilities([FromQuery] string? q, [FromQuery] FacilityCategory? category, [FromQuery] string? district, [FromQuery] AuditStatus? status, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOpenFacilitiesCommand(q, category, district, status), ct);

            return result.ToActionResult();
        }

        /// <summary>Objekty jako body v GeoJSON (WGS 84) — pro QGIS, Leaflet, MapLibre</summary>
        [HttpGet]
        [Route("facilities.geojson")]
        [OutputCache(PolicyName = ListCachePolicy)]
        [ResponseCache(Duration = ListCacheSeconds, Location = ResponseCacheLocation.Any)]
        [Produces("application/geo+json")]
        [ProducesResponseType<OpenFeatureCollectionDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFacilitiesGeoJson([FromQuery] string? q, [FromQuery] FacilityCategory? category, [FromQuery] string? district, [FromQuery] AuditStatus? status, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOpenFacilitiesCommand(q, category, district, status), ct);

            return result.IsSuccess && result.Data != null
                ? new ObjectResult(OpenFacilitiesFormats.ToGeoJson(result.Data)) { ContentTypes = { "application/geo+json" } }
                : result.ToActionResult();
        }

        /// <summary>Objekty jako CSV (RFC 4180, UTF-8)</summary>
        [HttpGet]
        [Route("facilities.csv")]
        [OutputCache(PolicyName = ListCachePolicy)]
        [ResponseCache(Duration = ListCacheSeconds, Location = ResponseCacheLocation.Any)]
        [Produces("text/csv")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFacilitiesCsv([FromQuery] string? q, [FromQuery] FacilityCategory? category, [FromQuery] string? district, [FromQuery] AuditStatus? status, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOpenFacilitiesCommand(q, category, district, status), ct);

            return result.IsSuccess && result.Data != null
                ? File(OpenFacilitiesFormats.ToCsv(result.Data), "text/csv; charset=utf-8", "smartaudit-objekty.csv")
                : result.ToActionResult();
        }

        /// <summary>Detail objektu — adresa, organizace, budova ČÚZK, obsazenost, stav auditu</summary>
        [HttpGet]
        [Route("facilities/{id:guid}")]
        [OutputCache(PolicyName = ListCachePolicy)]
        [ResponseCache(Duration = ListCacheSeconds, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType<GetFacilityDetailDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFacility(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetFacilityDetailCommand(id, Public), ct);

            return result.ToActionResult();
        }

        /// <summary>Půdorys budovy z ČÚZK jako GeoJSON Feature; cache 2 dny</summary>
        [HttpGet]
        [Route("facilities/{id:guid}/footprint")]
        [OutputCache(PolicyName = FootprintCachePolicy)]
        [ResponseCache(Duration = FootprintCacheSeconds, Location = ResponseCacheLocation.Any)]
        [Produces("application/geo+json")]
        [ProducesResponseType<GetFootprintDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFootprint(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetFootprintCommand(id), ct);

            return result.IsSuccess && result.Data != null
                ? new ObjectResult(result.Data) { ContentTypes = { "application/geo+json" } }
                : result.ToActionResult();
        }

        /// <summary>Časová řada měření (simulované senzory) — nejvýš 31 dní</summary>
        /// <param name="id">Id objektu</param>
        /// <param name="metric">Veličina: Temperature (°C), Co2 (ppm), Power (kWh/h, po dnech kWh/den)</param>
        /// <param name="from">Začátek (ISO 8601), výchozí před 48 h</param>
        /// <param name="to">Konec (ISO 8601), výchozí teď</param>
        /// <param name="bucket">Agregace: Raw, Hour, Day</param>
        [HttpGet]
        [Route("facilities/{id:guid}/telemetry")]
        [OutputCache(PolicyName = ListCachePolicy)]
        [ResponseCache(Duration = ListCacheSeconds, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType<GetTelemetryDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTelemetry(Guid id, [FromQuery] SensorMetric metric, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] TelemetryBucket? bucket, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTelemetryCommand(id, Public, metric, from, to, bucket ?? TelemetryBucket.Hour), ct);

            return result.ToActionResult();
        }

        /// <summary>Pre-audit budov z otevřených dat (JSON) — skóre, parametry A–E se zdroji, fyzika budovy, sada IQRF</summary>
        /// <param name="category">Druh objektu</param>
        /// <param name="district">Kód okresu</param>
        [HttpGet]
        [Route("preaudit")]
        [OutputCache(PolicyName = ListCachePolicy)]
        [ResponseCache(Duration = ListCacheSeconds, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType<IReadOnlyCollection<OpenPreAuditDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPreAudits([FromQuery] FacilityCategory? category, [FromQuery] string? district, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOpenPreAuditsCommand(category, district), ct);

            return result.ToActionResult();
        }

        /// <summary>Pre-audit budov jako CSV (RFC 4180, UTF-8)</summary>
        [HttpGet]
        [Route("preaudit.csv")]
        [OutputCache(PolicyName = ListCachePolicy)]
        [ResponseCache(Duration = ListCacheSeconds, Location = ResponseCacheLocation.Any)]
        [Produces("text/csv")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPreAuditsCsv([FromQuery] FacilityCategory? category, [FromQuery] string? district, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOpenPreAuditsCommand(category, district), ct);

            return result.IsSuccess && result.Data != null
                ? File(OpenPreAuditFormats.ToCsv(result.Data), "text/csv; charset=utf-8", "smartaudit-predaudit.csv")
                : result.ToActionResult();
        }

        /// <summary>Použité zdroje dat — sady Data KHK, ČÚZK, Open-Meteo, MŠMT, stav posledního stažení</summary>
        [HttpGet]
        [Route("datasets")]
        [OutputCache(PolicyName = ListCachePolicy)]
        [ResponseCache(Duration = ListCacheSeconds, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType<GetDataSourcesDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDatasets(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDataSourcesCommand(), ct);

            return result.ToActionResult();
        }
    }
}
