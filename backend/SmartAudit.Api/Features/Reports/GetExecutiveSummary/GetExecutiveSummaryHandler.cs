using MediatR;
using QuestPDF.Fluent;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Facilities.GetFacilities;
using SmartAudit.Api.Services.Reports;
using SmartAudit.Core.Common;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Features.Reports.GetExecutiveSummary
{
    public record GetExecutiveSummaryCommand(ViewerContext Viewer) : IRequest<Response<GetExecutiveSummaryDto>>;

    public record GetExecutiveSummaryDto(byte[] Content, string FileName);

    // Jednostránkový souhrn pro Radu kraje — celý kraj a částky v Kč, proto jen pro roli Kraj
    public class GetExecutiveSummaryHandler : IRequestHandler<GetExecutiveSummaryCommand, Response<GetExecutiveSummaryDto>>
    {
        // Kolik budov navrhnout do pilotu — pět priorit se vejde na stránku a je to rozumný první krok
        private const int PilotSize = 5;

        private readonly IMediator _mediator;
        private readonly IDistrictMapSource _districtMapSource;
        private readonly AuditSettings _settings;

        public GetExecutiveSummaryHandler(IMediator mediator, IDistrictMapSource districtMapSource, AuditSettings settings)
        {
            _mediator = mediator;
            _districtMapSource = districtMapSource;
            _settings = settings;
        }

        public async Task<Response<GetExecutiveSummaryDto>> Handle(GetExecutiveSummaryCommand request, CancellationToken ct)
        {
            if (request.Viewer.Role != AppRoles.Region)
                return Response.Forbidden<GetExecutiveSummaryDto>("Souhrn pro Radu kraje je jen pro kraj");

            var facilities = await _mediator.Send(new GetFacilitiesCommand(request.Viewer), ct);
            if (!facilities.IsSuccess || facilities.Data == null)
                return Response.Error<GetExecutiveSummaryDto>(facilities.Message ?? "Chyba na straně serveru", facilities.Status);

            var districts = await _districtMapSource.GetDistrictsAsync(ct);

            var data = new ExecutiveSummaryData(facilities.Data.ToList(), districts, _settings.SensorKitCzkPerBuilding, PilotSize, DateTimeOffset.UtcNow);
            var pdf = new ExecutiveSummaryReportDocument(data).GeneratePdf();

            return Response.Success(new GetExecutiveSummaryDto(pdf, "smartaudit-souhrn-pro-radu-kraje.pdf"));
        }
    }
}
