using System.Globalization;
using MediatR;
using QuestPDF.Fluent;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Facilities.GetFacilityDetail;
using SmartAudit.Api.Features.Facilities.GetHeatmap;
using SmartAudit.Api.Features.Facilities.GetSection;
using SmartAudit.Api.Features.Facilities.GetTelemetry;
using SmartAudit.Api.Services.Reports;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Reports.GetFacilityReport
{
    public record GetFacilityReportCommand(Guid FacilityId, ViewerContext Viewer) : IRequest<Response<GetFacilityReportDto>>;

    public record GetFacilityReportDto(byte[] Content, string FileName);

    // PDF audit objektu — data bere ze stejných slicí jako inspekční panel, aby report a web ukazovaly totéž
    public class GetFacilityReportHandler : IRequestHandler<GetFacilityReportCommand, Response<GetFacilityReportDto>>
    {
        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");

        private readonly IMediator _mediator;
        private readonly IReportAiSource _aiSource;

        public GetFacilityReportHandler(IMediator mediator, IReportAiSource aiSource)
        {
            _mediator = mediator;
            _aiSource = aiSource;
        }

        public async Task<Response<GetFacilityReportDto>> Handle(GetFacilityReportCommand request, CancellationToken ct)
        {
            var detail = await _mediator.Send(new GetFacilityDetailCommand(request.FacilityId, request.Viewer), ct);
            if (!detail.IsSuccess || detail.Data == null)
                return Response.Error<GetFacilityReportDto>(detail.Message ?? "Objekt s tímto id neexistuje", detail.Status);

            var now = DateTimeOffset.UtcNow;
            var weekStart = SchoolCalendar.LocalMidnightUtc(DateOnly.FromDateTime(SchoolCalendar.ToLocal(now)).AddDays(-6));

            var temperature = await _mediator.Send(new GetTelemetryCommand(request.FacilityId, request.Viewer, SensorMetric.Temperature, now.AddHours(-48), now, TelemetryBucket.Hour), ct);
            var energy = await _mediator.Send(new GetTelemetryCommand(request.FacilityId, request.Viewer, SensorMetric.Power, weekStart, now, TelemetryBucket.Day), ct);
            var heatmap = await _mediator.Send(new GetHeatmapCommand(request.FacilityId, request.Viewer, SensorMetric.Power), ct);
            var section = await _mediator.Send(new GetSectionCommand(request.FacilityId, request.Viewer), ct);

            // AI analýza obsahuje Kč — veřejnost ji v reportu nedostane
            var aiAnalysis = request.Viewer.CanSeeMoney ? await _aiSource.GetLatestAsync(request.FacilityId, ct) : null;

            var energyPoints = energy.Data?.Series.FirstOrDefault()?.Points ?? [];

            var data = new FacilityReportData(
                detail.Data,
                AverageAcrossZones(temperature.Data),
                energyPoints
                    .Select((p, i) => new ChartBar(i == energyPoints.Count - 1 ? "dnes" : SchoolCalendar.ToLocal(p.Timestamp).ToString("ddd d. M.", Cs), p.Value, Muted: i == energyPoints.Count - 1))
                    .ToList(),
                heatmap.Data?.Days.Select(d => new HeatmapRow(d.Label, d.IsFreeDay, d.Hours)).ToList() ?? [],
                heatmap.Data?.Min,
                heatmap.Data?.Max,
                section.Data?.Floors ?? 1,
                section.Data?.Zones.Select(z => new SectionZone(z.Name, z.Floor, z.Temperature, z.Co2)).ToList() ?? [],
                section.Data?.Co2Limit ?? 1500,
                aiAnalysis,
                now);

            var pdf = new FacilityReportDocument(data).GeneratePdf();

            return Response.Success(new GetFacilityReportDto(pdf, $"smartaudit-audit-{detail.Data.Code}.pdf"));
        }

        // Jedna křivka za objekt — průměr zón v každé hodině (stejně jako graf ve webu)
        private static List<ChartPoint> AverageAcrossZones(GetTelemetryDto? telemetry)
        {
            if (telemetry == null)
                return [];

            return telemetry.Series
                .SelectMany(s => s.Points)
                .GroupBy(p => p.Timestamp)
                .OrderBy(g => g.Key)
                .Select(g => new ChartPoint(g.Key, g.Average(p => p.Value)))
                .ToList();
        }
    }
}
