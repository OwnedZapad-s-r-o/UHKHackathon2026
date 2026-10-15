using System.Text.RegularExpressions;
using MediatR;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Facilities.GetTelemetry;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Guide
{
    // Jaký graf odpověď chce: pořadí objektů (sloupce) nebo průběh měření jednoho objektu (čáry)
    public enum GuideChartRequest
    {
        None,
        Ranking,
        Measurements,
    }

    public enum GuideChartMetric
    {
        Auto,
        Co2,
        Co2Hours,
        Temperature,
        Loss,
        Power,
    }

    public enum GuideChartKind
    {
        Ranking,
        Series,
    }

    public record GuideChartBar(int Rank, Guid FacilityId, string Label, double Value, AuditStatus Status);

    public record GuideChartPoint(DateTimeOffset Time, double Value);

    // Primary = hlavní čára (výrazná), ostatní jsou pro srovnání (tlumené)
    public record GuideChartLine(string Label, bool Primary, IReadOnlyList<GuideChartPoint> Points);

    // Referenční hranice v grafu (limit CO₂, útlumová teplota) — vždy s popiskem
    public record GuideChartThreshold(double Value, string Label, string Tone);

    public record GuideChart(GuideChartKind Kind, string Title, string Unit, IReadOnlyList<GuideChartBar> Bars, IReadOnlyList<GuideChartLine> Lines, IReadOnlyList<GuideChartThreshold> Thresholds, string? Note);

    public interface IGuideChartBuilder
    {
        Task<IReadOnlyList<GuideChart>> BuildAsync(GuideDraft draft, string question, ViewerContext viewer, CancellationToken ct);
    }

    // Grafy k odpovědi průvodce. Model jen říká, jaký graf chce — čísla bere aplikace z dat (výsledky hledání, telemetrie),
    // takže graf nemůže ukázat nic vymyšleného. Bez žádosti modelu se graf přidá sám tam, kde dává smysl (pořadí 3+ objektů,
    // „ukaž/graf/vývoj“ u jednoho objektu).
    public partial class GuideChartBuilder : IGuideChartBuilder
    {
        private static readonly TimeSpan SeriesWindow = TimeSpan.FromHours(48);

        private readonly IMediator _mediator;

        public GuideChartBuilder(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<GuideChart>> BuildAsync(GuideDraft draft, string question, ViewerContext viewer, CancellationToken ct)
        {
            var wantsChart = ChartWords().IsMatch(GuideText.Normalize(question));
            var request = draft.Chart;

            if (request == GuideChartRequest.None && draft.Focus == GuideFocus.Facilities && draft.Facilities.Count >= (wantsChart ? 2 : 3))
                request = GuideChartRequest.Ranking;
            if (request == GuideChartRequest.None && draft.Focus == GuideFocus.Facility && wantsChart)
                request = GuideChartRequest.Measurements;

            return request switch
            {
                GuideChartRequest.Ranking when draft.Facilities.Count >= 2 => Ranking(draft, viewer),
                GuideChartRequest.Measurements when draft.Facilities.Count >= 1 => await MeasurementsAsync(draft.Facilities[0], draft.ChartMetric, viewer, ct),
                _ => [],
            };
        }

        private static IReadOnlyList<GuideChart> Ranking(GuideDraft draft, ViewerContext viewer)
        {
            var metric = draft.ChartMetric == GuideChartMetric.Loss && !viewer.CanSeeMoney ? GuideChartMetric.Co2Hours : draft.ChartMetric;
            if (metric is GuideChartMetric.Auto or GuideChartMetric.Power)
                metric = viewer.CanSeeMoney && draft.Facilities.Any(f => f.AnnualLossCzk > 0) ? GuideChartMetric.Loss : GuideChartMetric.Co2Hours;

            var (title, unit, value) = metric switch
            {
                GuideChartMetric.Loss => ("Odhad roční ztráty", "Kč", (Func<GuideFacility, double?>)(f => (double?)f.AnnualLossCzk)),
                GuideChartMetric.Co2 => ("CO₂ teď v nejhorší místnosti", "ppm", f => f.Co2),
                GuideChartMetric.Temperature => ("Průměrná teplota teď", "°C", f => f.Temperature),
                _ => ("Hodiny nad limitem CO₂ za týden", "h", f => f.Co2HoursOverLimit),
            };

            var bars = draft.Facilities
                .Select((f, i) => (Facility: f, Rank: i + 1, Value: value(f)))
                .Where(x => x.Value != null)
                .Select(x => new GuideChartBar(x.Rank, x.Facility.Id, x.Facility.Name, Math.Round(x.Value!.Value, 1), x.Facility.Status))
                .ToList();

            // Samé nuly nic neříkají — graf by byl prázdný
            if (bars.Count < 2 || bars.All(b => b.Value == 0))
                return [];

            IReadOnlyList<GuideChartThreshold> thresholds = metric == GuideChartMetric.Co2
                ? [new(1000, "vyvětrat", "warning"), new(1500, "limit", "critical")]
                : [];

            var modelled = draft.Facilities.Any(f => !f.HasLiveSensors) && metric != GuideChartMetric.Loss;
            return [new GuideChart(GuideChartKind.Ranking, title, unit, bars, [], thresholds, modelled ? "Objekty bez čidla mají modelová data." : null)];
        }

        private async Task<IReadOnlyList<GuideChart>> MeasurementsAsync(GuideFacility facility, GuideChartMetric metric, ViewerContext viewer, CancellationToken ct)
        {
            SensorMetric[] metrics = metric switch
            {
                GuideChartMetric.Temperature => [SensorMetric.Temperature],
                GuideChartMetric.Power => [SensorMetric.Power],
                GuideChartMetric.Co2 or GuideChartMetric.Co2Hours => [SensorMetric.Co2],
                // Bez upřesnění: problém s vytápěním ukáže teplota, jinak vzduch
                _ => facility.Headline.Contains("CO₂", StringComparison.Ordinal) ? [SensorMetric.Co2, SensorMetric.Temperature] : [SensorMetric.Temperature, SensorMetric.Co2],
            };

            var charts = new List<GuideChart>();
            var to = DateTimeOffset.UtcNow;

            foreach (var sensorMetric in metrics)
            {
                var telemetry = await _mediator.Send(new GetTelemetryCommand(facility.Id, viewer, sensorMetric, to - SeriesWindow, to, TelemetryBucket.Hour), ct);
                if (!telemetry.IsSuccess || telemetry.Data == null)
                    continue;

                var chart = SeriesChart(telemetry.Data);
                if (chart != null)
                    charts.Add(chart);
            }

            return charts;
        }

        // Jedna veličina, jedna osa: průměr budovy a u CO₂ i nejhorší místnost (stejná jednotka, žádná druhá osa)
        private static GuideChart? SeriesChart(GetTelemetryDto telemetry)
        {
            var series = telemetry.Series.Where(s => s.Points.Count > 1).ToList();
            if (series.Count == 0)
                return null;

            var combined = telemetry.Metric == SensorMetric.Power
                ? Combine(series, values => values.Sum())
                : Combine(series, values => values.Average());

            var lines = new List<GuideChartLine>();
            if (telemetry.Metric == SensorMetric.Co2)
            {
                var worst = series.OrderByDescending(s => s.Points.Max(p => p.Value)).First();
                lines.Add(new GuideChartLine($"Nejhorší místnost: {worst.Name}", true, worst.Points.Select(p => new GuideChartPoint(p.Timestamp, Math.Round(p.Value))).ToList()));
                if (series.Count > 1)
                    lines.Add(new GuideChartLine("Průměr budovy", false, combined));
            }
            else
            {
                lines.Add(new GuideChartLine(telemetry.Metric == SensorMetric.Power ? "Příkon budovy" : "Průměr budovy", true, combined));
            }

            var (title, thresholds) = telemetry.Metric switch
            {
                SensorMetric.Co2 => ("CO₂ za posledních 48 hodin", (IReadOnlyList<GuideChartThreshold>)[new(1000, "vyvětrat", "warning"), new(1500, "limit 1 500 ppm", "critical")]),
                SensorMetric.Temperature => ("Teplota za posledních 48 hodin", [new(18, "útlum 18 °C", "neutral")]),
                _ => ("Příkon vytápění za posledních 48 hodin", []),
            };

            var live = series.Any(s => s.Live);
            return new GuideChart(GuideChartKind.Series, title, telemetry.Unit, [], lines, thresholds, live ? "Naměřeno čidly IQRF." : "Modelová data — budova zatím nemá čidla.");
        }

        private static List<GuideChartPoint> Combine(IEnumerable<TelemetrySeriesDto> series, Func<IEnumerable<double>, double> aggregate)
        {
            return series
                .SelectMany(s => s.Points)
                .GroupBy(p => p.Timestamp)
                .OrderBy(g => g.Key)
                .Select(g => new GuideChartPoint(g.Key, Math.Round(aggregate(g.Select(p => p.Value)), 1)))
                .ToList();
        }

        [GeneratedRegex(@"\b(graf\w*|vyvoj\w*|prubeh\w*|zobraz\w*|ukaz\w*|vizualiz\w*|krivk\w*|trend\w*|historie|za\s+(den|tyden|48))\b")]
        private static partial Regex ChartWords();
    }
}
