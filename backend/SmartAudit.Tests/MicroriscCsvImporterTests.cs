using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Tests
{
    // Skutečná měření MICRORISC (Jičín) ze seed/microrisc: 3 prostory × teplota a relativní vlhkost
    public class MicroriscCsvImporterTests
    {
        private static readonly string Directory = FindDataDirectory();

        private static string FindDataDirectory()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "seed", MicroriscReference.DataDirectory);
                if (System.IO.Directory.Exists(candidate))
                    return candidate;
            }

            throw new DirectoryNotFoundException("Složka seed/microrisc nebyla nalezena.");
        }

        [Fact]
        public void Loads_all_six_exports_for_three_storage_zones()
        {
            var series = MicroriscCsvImporter.LoadDirectory(Directory);

            Assert.Equal(6, series.Count);
            Assert.Equal(["Mezisklad", "Sklad 1", "Sklad 2"], series.Select(s => s.Zone).Distinct().Order());
            Assert.All(MicroriscReference.Zones, zone =>
            {
                Assert.Single(series, s => s.Zone == zone.Name && s.Metric == SensorMetric.Temperature);
                Assert.Single(series, s => s.Zone == zone.Name && s.Metric == SensorMetric.Humidity);
            });
            Assert.All(series, s => Assert.True(s.Points.Count > 2000, $"{s.FileName}: {s.Points.Count} bodů"));
        }

        [Fact]
        public void Values_and_units_match_the_export()
        {
            var series = MicroriscCsvImporter.LoadDirectory(Directory);

            var temperatures = series.Where(s => s.Metric == SensorMetric.Temperature).SelectMany(s => s.Points).Select(p => p.Value).ToList();
            var humidity = series.Where(s => s.Metric == SensorMetric.Humidity).SelectMany(s => s.Points).Select(p => p.Value).ToList();

            Assert.All(series.Where(s => s.Metric == SensorMetric.Temperature), s => Assert.Equal("°C", s.Unit));
            Assert.All(series.Where(s => s.Metric == SensorMetric.Humidity), s => Assert.Equal("%", s.Unit));
            Assert.InRange(temperatures.Min(), 12.0, 13.0);
            Assert.InRange(temperatures.Max(), 22.0, 22.2);
            Assert.InRange(humidity.Min(), 30, 100);
            Assert.InRange(humidity.Max(), 30, 100);
        }

        [Fact]
        public void Local_prague_time_is_converted_to_utc()
        {
            var sklad2 = MicroriscCsvImporter.LoadDirectory(Directory).Single(s => s.Zone == "Sklad 2" && s.Metric == SensorMetric.Temperature);

            // 2026-10-08 16:36:01 v Jičíně (letní čas, UTC+2)
            Assert.Equal(new DateTimeOffset(2026, 10, 8, 14, 36, 1, TimeSpan.Zero), sklad2.Points[0].Timestamp);
            Assert.Equal(21.1, sklad2.Points[0].Value);
            Assert.True(sklad2.Points.Zip(sklad2.Points.Skip(1)).All(p => p.First.Timestamp < p.Second.Timestamp));
        }

        [Fact]
        public void Parses_utf16_tab_separated_export_and_ignores_garbage()
        {
            const string csv = "\"Time\"\t\"Sklad 9\"\r\n2026-10-08 10:00:00\t53.5 %H\r\nnesmysl\r\n2026-10-08 10:04:00\t54 %H\r\n";
            using var stream = new MemoryStream([.. Encoding.Unicode.GetPreamble(), .. Encoding.Unicode.GetBytes(csv)]);

            var series = MicroriscCsvImporter.Parse("Relativni vlhkost-data.csv", stream);

            Assert.NotNull(series);
            Assert.Equal("Sklad 9", series.Zone);
            Assert.Equal(SensorMetric.Humidity, series.Metric);
            Assert.Equal([53.5, 54.0], series.Points.Select(p => p.Value));
        }

        [Fact]
        public void Replay_covers_the_last_week_with_real_values_shifted_by_whole_weeks()
        {
            var series = MicroriscCsvImporter.LoadDirectory(Directory);
            var anchor = series.Min(s => s.Points[0].Timestamp);
            var source = series.Single(s => s.Zone == "Sklad 1" && s.Metric == SensorMetric.Temperature);
            var now = new DateTimeOffset(2026, 10, 23, 9, 50, 0, TimeSpan.Zero);

            var replayed = MeasuredReplay.Between(source, anchor, now - MeasuredReplay.Period, now).ToList();

            // Celý týden: stejný počet bodů jako export (± bod na švu týdne)
            Assert.InRange(replayed.Count, source.Points.Count - 1, source.Points.Count + 1);
            Assert.All(replayed, p => Assert.InRange(p.Timestamp, now - MeasuredReplay.Period, now));
            Assert.Equal(replayed.Count, replayed.Select(p => p.Timestamp).Distinct().Count());

            // Každý bod je skutečná hodnota z exportu posunutá o celý počet týdnů — sedí den v týdnu i denní doba
            var byTime = source.Points.ToDictionary(p => p.Timestamp, p => p.Value);
            Assert.All(replayed, p =>
            {
                var weeks = Math.Round((p.Timestamp - anchor).TotalDays / 7, MidpointRounding.ToZero);
                var original = Enumerable.Range(-1, 3).Select(k => p.Timestamp - TimeSpan.FromDays(7 * (weeks + k))).First(byTime.ContainsKey);
                Assert.Equal(byTime[original], p.Value);
                Assert.Equal(original.DayOfWeek, p.Timestamp.DayOfWeek);
                Assert.Equal(original.TimeOfDay, p.Timestamp.TimeOfDay);
            });
        }

        [Fact]
        public void Replay_continues_after_the_last_written_point()
        {
            var source = MicroriscCsvImporter.LoadDirectory(Directory).Single(s => s.Zone == "Mezisklad" && s.Metric == SensorMetric.Humidity);
            var anchor = new DateTimeOffset(2026, 10, 8, 14, 36, 1, TimeSpan.Zero);
            var now = new DateTimeOffset(2026, 10, 23, 9, 50, 0, TimeSpan.Zero);

            var first = MeasuredReplay.Between(source, anchor, now - MeasuredReplay.Period, now).ToList();
            var next = MeasuredReplay.Between(source, anchor, first[^1].Timestamp, now.AddMinutes(10)).ToList();

            Assert.InRange(next.Count, 1, 3);
            Assert.All(next, p => Assert.True(p.Timestamp > first[^1].Timestamp));
            Assert.Equal(MeasuredReplay.Latest(source, anchor, now.AddMinutes(10))!.Timestamp, next[^1].Timestamp);
        }

        [Fact]
        public void Gps_point_lies_inside_the_cadastral_footprint()
        {
            var footprint = new WKTReader().Read(MicroriscReference.FootprintWkt);
            var point = new Point(MicroriscReference.Longitude, MicroriscReference.Latitude);

            Assert.True(footprint.Contains(point));
            Assert.Equal(MicroriscReference.Zones.Length, MicroriscReference.Zones.Select(z => z.NodeAddress).Distinct().Count());
        }
    }
}
