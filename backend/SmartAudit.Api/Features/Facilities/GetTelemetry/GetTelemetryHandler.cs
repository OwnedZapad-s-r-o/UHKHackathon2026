using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Facilities.GetTelemetry
{
    public enum TelemetryBucket
    {
        Raw,
        Hour,
        Day,
    }

    public record GetTelemetryCommand(Guid FacilityId, ViewerContext Viewer, SensorMetric Metric, DateTimeOffset? From = null, DateTimeOffset? To = null, TelemetryBucket Bucket = TelemetryBucket.Hour)
        : IRequest<Response<GetTelemetryDto>>;

    public record GetTelemetryDto(SensorMetric Metric, string Unit, TelemetryBucket Bucket, DateTimeOffset From, DateTimeOffset To, IReadOnlyCollection<TelemetrySeriesDto> Series);

    public record TelemetrySeriesDto(Guid? ZoneId, string Name, int? Floor, bool IsHotspot, IReadOnlyCollection<TelemetryPointDto> Points);

    public record TelemetryPointDto(DateTimeOffset Timestamp, double Value);

    public class GetTelemetryHandler : IRequestHandler<GetTelemetryCommand, Response<GetTelemetryDto>>
    {
        private const int MaxDays = 31;

        private readonly SmartAuditDbContext _dbContext;

        public GetTelemetryHandler(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<GetTelemetryDto>> Handle(GetTelemetryCommand request, CancellationToken ct)
        {
            var to = request.To ?? DateTimeOffset.UtcNow;
            var from = request.From ?? to.AddHours(-48);

            if (from >= to)
                return Response.ValidationFailed<GetTelemetryDto>("Začátek období musí být před jeho koncem.");

            if (to - from > TimeSpan.FromDays(MaxDays))
                return Response.ValidationFailed<GetTelemetryDto>($"Období může mít nejvýš {MaxDays} dní.");

            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Select(f => new { f.Id, f.OrganizationId })
                .FirstOrDefaultAsync(f => f.Id == request.FacilityId, ct);

            if (facility == null)
                return Response.NotFound<GetTelemetryDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<GetTelemetryDto>("K tomuto nemáte oprávnění");

            var sensors = await _dbContext.Sensors
                .AsNoTracking()
                .Where(s => s.FacilityId == request.FacilityId && s.Metric == request.Metric)
                .Select(s => new
                {
                    s.Id,
                    s.ZoneId,
                    Name = s.Zone != null ? s.Zone.Name : "Celý objekt",
                    Floor = s.Zone != null ? s.Zone.Floor : (int?)null,
                    Order = s.Zone != null ? s.Zone.Order : 0,
                    IsHotspot = s.Zone != null && s.Zone.IsHotspot,
                })
                .ToListAsync(ct);

            var sensorIds = sensors.Select(s => s.Id).ToList();

            var readings = await _dbContext.SensorReadings
                .AsNoTracking()
                .Where(r => sensorIds.Contains(r.SensorId) && r.Timestamp >= from && r.Timestamp <= to)
                .OrderBy(r => r.Timestamp)
                .Select(r => new { r.SensorId, r.Timestamp, r.Value })
                .ToListAsync(ct);

            var bySensor = readings.GroupBy(r => r.SensorId).ToDictionary(g => g.Key, g => g.Select(r => new TelemetryPointDto(r.Timestamp, r.Value)).ToList());

            var series = sensors
                .OrderBy(s => s.Floor)
                .ThenBy(s => s.Order)
                .Select(s => new TelemetrySeriesDto(
                    s.ZoneId,
                    s.Name,
                    s.Floor,
                    s.IsHotspot,
                    Aggregate(bySensor.GetValueOrDefault(s.Id) ?? [], request.Bucket, request.Metric)))
                .ToList();

            var dto = new GetTelemetryDto(request.Metric, Unit(request.Metric, request.Bucket), request.Bucket, from, to, series);

            return Response.Success(dto);
        }

        private static IReadOnlyCollection<TelemetryPointDto> Aggregate(List<TelemetryPointDto> points, TelemetryBucket bucket, SensorMetric metric)
        {
            if (bucket == TelemetryBucket.Raw || points.Count == 0)
                return points;

            var hourly = points
                .GroupBy(p => new DateTimeOffset(p.Timestamp.UtcTicks - p.Timestamp.UtcTicks % TimeSpan.TicksPerHour, TimeSpan.Zero))
                .Select(g => new TelemetryPointDto(g.Key, Math.Round(g.Average(p => p.Value), 1)))
                .ToList();

            if (bucket == TelemetryBucket.Hour)
                return hourly;

            // Den podle místního času; u příkonu součet hodinových průměrů = kWh za den
            return hourly
                .GroupBy(p => DateOnly.FromDateTime(SchoolCalendar.ToLocal(p.Timestamp)))
                .Select(g => new TelemetryPointDto(
                    SchoolCalendar.LocalMidnightUtc(g.Key),
                    Math.Round(metric == SensorMetric.Power ? g.Sum(p => p.Value) : g.Average(p => p.Value), 1)))
                .ToList();
        }

        private static string Unit(SensorMetric metric, TelemetryBucket bucket)
        {
            return metric switch
            {
                SensorMetric.Temperature => "°C",
                SensorMetric.Co2 => "ppm",
                _ => bucket == TelemetryBucket.Day ? "kWh/den" : "kWh/h",
            };
        }
    }
}
