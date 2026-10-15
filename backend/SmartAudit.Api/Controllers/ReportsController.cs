using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Reports.GetExecutiveSummary;
using SmartAudit.Api.Features.Reports.GetFacilityReport;
using SmartAudit.Api.Features.Reports.GetPortfolioReport;

namespace SmartAudit.Api.Controllers
{
    // PDF reporty — úspěch vrací soubor, chyba stejný JSON tvar jako ostatní endpointy
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private const string PdfContentType = "application/pdf";

        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("facility/{id:guid}.pdf")]
        public async Task<IActionResult> Facility(Guid id, CancellationToken ct)
        {
            var command = new GetFacilityReportCommand(id, User.GetViewer());

            var result = await _mediator.Send(command, ct);

            return result.IsSuccess && result.Data != null
                ? File(result.Data.Content, PdfContentType, result.Data.FileName)
                : result.ToActionResult();
        }

        // Jedna strana A4 pro Radu kraje: ztráta kraje, pět priorit, pilot IQRF s návratností, návrh usnesení
        [HttpGet]
        [Route("executive-summary.pdf")]
        public async Task<IActionResult> ExecutiveSummary(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetExecutiveSummaryCommand(User.GetViewer()), ct);

            return result.IsSuccess && result.Data != null
                ? File(result.Data.Content, PdfContentType, result.Data.FileName)
                : result.ToActionResult();
        }

        [HttpGet]
        [Route("portfolio.pdf")]
        public async Task<IActionResult> Portfolio([FromQuery] string? district, [FromQuery] bool onlyIssues, CancellationToken ct)
        {
            var command = new GetPortfolioReportCommand(User.GetViewer(), district, onlyIssues);

            var result = await _mediator.Send(command, ct);

            return result.IsSuccess && result.Data != null
                ? File(result.Data.Content, PdfContentType, result.Data.FileName)
                : result.ToActionResult();
        }
    }
}
