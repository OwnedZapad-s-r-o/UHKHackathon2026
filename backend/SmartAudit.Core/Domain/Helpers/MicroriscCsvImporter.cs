using System.Globalization;
using System.Text;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Core.Domain.Helpers
{
    public record MeasuredPoint(DateTimeOffset Timestamp, double Value);

    // Jedna časová řada z exportu MICRORISC: prostor (sloupec v hlavičce), veličina a body v UTC
    public record MeasuredSeries(string FileName, string Zone, SensorMetric Metric, string Unit, IReadOnlyList<MeasuredPoint> Points);

    // Export telemetrie IQRF z MICRORISC s.r.o. (Grafana): UTF-16 LE s BOM, oddělovač tabulátor,
    // hlavička "Time" ⇥ "Sklad 1", řádek 2026-10-08 16:40:11 ⇥ 53.5 %H (nebo 21.1 °C). Čas je místní (Europe/Prague).
    public static class MicroriscCsvImporter
    {
        private static readonly TimeZoneInfo Prague = TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague");
        private const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static IReadOnlyList<MeasuredSeries> LoadDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                return [];

            return Directory.EnumerateFiles(directory, "*.csv")
                .Order(StringComparer.Ordinal)
                .Select(path =>
                {
                    using var stream = File.OpenRead(path);
                    return Parse(Path.GetFileName(path), stream);
                })
                .OfType<MeasuredSeries>()
                .ToList();
        }

        // null = soubor není export teploty ani vlhkosti
        public static MeasuredSeries? Parse(string fileName, Stream content)
        {
            // Kódování podle BOM (export je UTF-16 LE), bez BOM UTF-8
            using var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var header = reader.ReadLine();
            if (header == null)
                return null;

            var columns = header.Split('\t');
            if (columns.Length < 2)
                return null;

            var zone = columns[1].Trim().Trim('"').Trim();
            var points = new List<MeasuredPoint>();
            string? unit = null;

            for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                var parts = line.Split('\t');
                if (parts.Length < 2)
                    continue;

                if (!DateTime.TryParseExact(parts[0].Trim().Trim('"'), TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var local))
                    continue;

                // "21.1 °C" / "53.5 %H" — číslo, mezera, jednotka
                var valueParts = parts[1].Trim().Trim('"').Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length == 0 || !double.TryParse(valueParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    continue;

                unit ??= valueParts.Length > 1 ? valueParts[1].Trim() : null;

                var offset = Prague.GetUtcOffset(local);
                points.Add(new MeasuredPoint(new DateTimeOffset(local, offset).ToUniversalTime(), value));
            }

            var metric = MetricOf(fileName, unit);
            if (metric == null || points.Count == 0)
                return null;

            var ordered = points
                .GroupBy(p => p.Timestamp)
                .Select(g => g.First())
                .OrderBy(p => p.Timestamp)
                .ToList();

            return new MeasuredSeries(fileName, zone, metric.Value, metric == SensorMetric.Temperature ? "°C" : "%", ordered);
        }

        private static SensorMetric? MetricOf(string fileName, string? unit)
        {
            if (fileName.StartsWith("Teplota", StringComparison.OrdinalIgnoreCase) || unit?.EndsWith('C') == true)
                return SensorMetric.Temperature;

            if (fileName.Contains("vlhkost", StringComparison.OrdinalIgnoreCase) || unit?.StartsWith('%') == true)
                return SensorMetric.Humidity;

            return null;
        }
    }

    // Přehrání týdne skutečných měření v aktuálním čase. Export pokrývá přesně 7 dní, takže posun o celé týdny
    // zachová den v týdnu, denní dobu, tvar křivek i mezery v měření — hodnoty se nemění, jen se opakuje týden.
    public static class MeasuredReplay
    {
        public static readonly TimeSpan Period = TimeSpan.FromDays(7);

        // Body řady posunuté o celé týdny tak, aby padly do intervalu (from, to]. anchor = začátek exportu (společný pro všechny řady).
        public static IEnumerable<MeasuredPoint> Between(MeasuredSeries series, DateTimeOffset anchor, DateTimeOffset from, DateTimeOffset to)
        {
            if (to <= from || series.Points.Count == 0)
                yield break;

            var firstCycle = (long)Math.Floor((from - anchor).Ticks / (double)Period.Ticks) - 1;
            var lastCycle = (long)Math.Floor((to - anchor).Ticks / (double)Period.Ticks) + 1;
            var previous = DateTimeOffset.MinValue;

            for (var cycle = firstCycle; cycle <= lastCycle; cycle++)
            {
                var shift = TimeSpan.FromTicks(Period.Ticks * cycle);

                foreach (var point in series.Points)
                {
                    var timestamp = point.Timestamp + shift;

                    // Poslední bod týdne se může potkat s prvním bodem dalšího opakování — každý čas jen jednou
                    if (timestamp <= from || timestamp > to || timestamp <= previous)
                        continue;

                    previous = timestamp;
                    yield return new MeasuredPoint(timestamp, point.Value);
                }
            }
        }

        // Poslední skutečná hodnota k okamžiku now (pro „Místnosti teď“ a kanál gatewaye)
        public static MeasuredPoint? Latest(MeasuredSeries series, DateTimeOffset anchor, DateTimeOffset now)
        {
            return Between(series, anchor, now - Period, now).LastOrDefault();
        }
    }
}
