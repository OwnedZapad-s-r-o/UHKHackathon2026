using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Facilities.GetHeatmap
{
    public record GetHeatmapCommand(Guid FacilityId, ViewerContext Viewer, SensorMetric Metric = SensorMetric.Power) : IRequest<Response<GetHeatmapDto>>;

    public record GetHeatmapDto(SensorMetric Metric, string Unit, double? Min, double? Max, IReadOnlyCollection<HeatmapDayDto> Days);

    public record HeatmapDayDto(DateOnly Date, string Label, bool IsFreeDay, IReadOnlyList<double?> Hours);

    // Provozní heatmapa hodina × den za posledních 7 dní (místní čas)
    public class GetHeatmapHandler : IRequestHandler<GetHeatmapCommand, Response<GetHeatmapDto>>
    {
        private static readonly CultureInfo Czech = CultureInfo.GetCultureInfo("cs-CZ");

        private readonly SmartAuditDbContext _dbContext;

        public GetHeatmapHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<GetHeatmapDto>> Handle(GetHeatmapCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Select(f => new { f.Id, f.OrganizationId, f.Category })
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId, ct);

            if (facility == null)
                return Response.NotFound<GetHeatmapDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<GetHeatmapDto>("K tomuto nemáte oprávnění");

            var today = DateOnly.FromDateTime(SchoolCalendar.ToLocal(DateTimeOffset.UtcNow));
            var firstDay = today.AddDays(-6);
            var from = SchoolCalendar.LocalMidnightUtc(firstDay);

            var readings = await _dbContext.SensorReadings
                .AsNoTracking()
                .Where(r => r.Timestamp >= from)
                .Join(_dbContext.Sensors.Where(s => s.FacilityId == request.FacilityId && s.Metric == request.Metric), r => r.SensorId, s => s.Id, (r, s) => new { r.SensorId, r.Timestamp, r.Value })
                .ToListAsync(ct);

            // Hodinový průměr každého senzoru, pak přes zóny: teplota průměr, CO₂ nejhorší zóna, příkon jediný měřič
            var cells = readings
                .Select(r => (r.SensorId, Local: SchoolCalendar.ToLocal(r.Timestamp), r.Value))
                .GroupBy(r => (r.SensorId, Day: DateOnly.FromDateTime(r.Local), r.Local.Hour))
                .Select(g => (g.Key.Day, g.Key.Hour, Value: g.Average(r => r.Value)))
                .GroupBy(c => (c.Day, c.Hour))
                .ToDictionary(g => g.Key, g => request.Metric == SensorMetric.Co2 ? g.Max(c => c.Value) : g.Average(c => c.Value));

            var days = Enumerable.Range(0, 7)
                .Select(i => firstDay.AddDays(i))
                .Select(day => new HeatmapDayDto(
                    day,
                    day.ToString("ddd d. M.", Czech),
                    SchoolCalendar.IsFreeDay(facility.Category, day),
                    Enumerable.Range(0, 24).Select(hour => cells.TryGetValue((day, hour), out var value) ? Math.Round(value, 1) : (double?)null).ToList()))
                .ToList();

            var values = cells.Values.ToList();
            var unit = SensorUnits.Of(request.Metric);

            var dto = new GetHeatmapDto(request.Metric, unit, values.Count > 0 ? Math.Round(values.Min(), 1) : null, values.Count > 0 ? Math.Round(values.Max(), 1) : null, days);

            return Response.Success(dto);
        }
    }
}
