using MediatR;
using QuestPDF.Fluent;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Facilities.GetFacilities;
using SmartAudit.Api.Services.Reports;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.Reports.GetPortfolioReport
{
    public record GetPortfolioReportCommand(ViewerContext Viewer, string? District = null, bool OnlyIssues = false) : IRequest<Response<GetPortfolioReportDto>>;

    public record GetPortfolioReportDto(byte[] Content, string FileName);

    // PDF přehled portfolia — stejný výběr jako obrazovka Portfolio (role, okres, jen se zjištěním)
    public class GetPortfolioReportHandler : IRequestHandler<GetPortfolioReportCommand, Response<GetPortfolioReportDto>>
    {
        private readonly IMediator _mediator;

        public GetPortfolioReportHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Response<GetPortfolioReportDto>> Handle(GetPortfolioReportCommand request, CancellationToken ct)
        {
            var facilities = await _mediator.Send(new GetFacilitiesCommand(request.Viewer, District: request.District, OnlyIssues: request.OnlyIssues), ct);
            if (!facilities.IsSuccess || facilities.Data == null)
                return Response.Error<GetPortfolioReportDto>(facilities.Message ?? "Chyba na straně serveru", facilities.Status);

            var list = facilities.Data.ToList();
            var districtName = request.District == null ? null : list.FirstOrDefault()?.DistrictName ?? request.District;

            var data = new PortfolioReportData(list, districtName, request.OnlyIssues, request.Viewer.CanSeeMoney, DateTimeOffset.UtcNow);
            var pdf = new PortfolioReportDocument(data).GeneratePdf();

            var suffix = request.District == null ? "kraj" : request.District.ToLowerInvariant();

            return Response.Success(new GetPortfolioReportDto(pdf, $"smartaudit-portfolio-{suffix}.pdf"));
        }
    }
}
