using System.Globalization;
using System.Security;
using System.Text;

namespace SmartAudit.Api.Services.Reports
{
    public record ChartPoint(DateTimeOffset Time, double Value);

    public record ChartBar(string Label, double Value, bool Muted = false);

    public record HeatmapRow(string Label, bool IsFreeDay, IReadOnlyList<double?> Hours);

    public record SectionZone(string Name, int Floor, double? Temperature, double? Co2);

    // SVG grafy pro PDF — stejná pravidla jako webové grafy (dataviz skill):
    // jedna série bez legendy, čára 2 px, sloupce ≤ 24 px se zaobleným koncem, sekvenční heatmapa v jednom odstínu,
    // hairline mřížka, text v neutrálních barvách, stav vždy s popiskem
    public static partial class ReportCharts
    {
        private const string Font = "Hradec, sans-serif";
        private const string Series = ReportStyles.Blue;
        private const string Grid = "#F1F5F9";
        private const string Axis = "#94A3B8";

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");

        public static string TemperatureLine(IReadOnlyList<ChartPoint> points, double setback)
        {
            const double width = 520, height = 170, left = 30, right = 8, top = 10, bottom = 22;
            var svg = Start(width, height);

            if (points.Count < 2)
                return Empty(svg, width, height, "Za posledních 48 hodin nejsou data.");

            var values = points.Select(p => p.Value).ToList();
            var min = Math.Floor(Math.Min(setback, values.Min()) / 2) * 2 - 1;
            var max = Math.Ceiling(values.Max() / 2) * 2 + 1;
            var plotW = width - left - right;
            var plotH = height - top - bottom;
            double X(int i) => left + (double)i / (points.Count - 1) * plotW;
            double Y(double v) => top + (1 - (v - min) / (max - min)) * plotH;

            for (var tick = Math.Ceiling(min / 2) * 2; tick <= max; tick += 2)
            {
                svg.Append(Line(left, Y(tick), width - right, Y(tick), Grid, 1));
                svg.Append(Text(left - 5, Y(tick) + 3, tick.ToString(Inv), 8, Axis, "end"));
            }

            svg.Append(Line(left, Y(setback), width - right, Y(setback), "#10B981", 1.2));
            svg.Append(Text(left + 4, Y(setback) - 4, $"Doporučený útlum ({setback.ToString(Cs)} °C)", 8, "#065F46"));

            var path = string.Join(" ", points.Select((p, i) => $"{(i == 0 ? "M" : "L")}{F(X(i))},{F(Y(p.Value))}"));
            svg.Append($"<path d=\"{path}\" fill=\"none\" stroke=\"{Series}\" stroke-width=\"2\" stroke-linejoin=\"round\" stroke-linecap=\"round\"/>");

            // Popisek jen u poslední hodnoty — ostatní nese osa
            var last = points[^1];
            svg.Append(Text(X(points.Count - 1) - 2, Y(last.Value) - 6, $"{last.Value.ToString("0.0", Cs)} °C", 8, ReportStyles.Navy, "end"));

            string[] labels = ["−48 h", "−36 h", "−24 h", "−12 h", "nyní"];
            for (var i = 0; i < labels.Length; i++)
                svg.Append(Text(left + i * plotW / 4, height - 6, labels[i], 8, Axis, i == 0 ? "start" : i == labels.Length - 1 ? "end" : "middle"));

            return End(svg);
        }

        public static string Columns(IReadOnlyList<ChartBar> bars, string unit)
        {
            const double width = 520, height = 150, top = 16, bottom = 20, barMax = 24;
            var svg = Start(width, height);

            if (bars.Count == 0)
                return Empty(svg, width, height, "Za sledované období nejsou data.");

            var max = Math.Max(1, bars.Max(b => b.Value));
            var slot = width / bars.Count;
            var barWidth = Math.Min(barMax, slot * 0.6);
            var plotH = height - top - bottom;

            svg.Append(Line(0, height - bottom, width, height - bottom, Grid, 1));

            for (var i = 0; i < bars.Count; i++)
            {
                var bar = bars[i];
                var h = Math.Max(1, bar.Value / max * plotH);
                var x = i * slot + (slot - barWidth) / 2;
                var y = height - bottom - h;

                svg.Append(RoundedTopBar(x, y, barWidth, h, bar.Muted ? "#A5B4C4" : Series));
                svg.Append(Text(x + barWidth / 2, y - 4, bar.Value.ToString("N0", Cs), 7.5, ReportStyles.Muted, "middle"));
                svg.Append(Text(x + barWidth / 2, height - 6, bar.Label, 8, Axis, "middle"));
            }

            svg.Append(Text(width, 9, unit, 8, Axis, "end"));

            return End(svg);
        }

        public static string Heatmap(IReadOnlyList<HeatmapRow> rows, double? min, double? max, string unit)
        {
            const double width = 520, labelW = 58, cellH = 13, gap = 2, legendH = 26;
            var height = rows.Count * (cellH + gap) + legendH + 14;
            var svg = Start(width, height);
            var cellW = (width - labelW) / 24 - gap;
            string[] opacities = ["0.08", "0.25", "0.45", "0.65", "0.9"];

            for (var r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                var y = r * (cellH + gap);
                svg.Append(Text(0, y + cellH - 3, row.Label, 8, row.IsFreeDay ? ReportStyles.Navy : Axis, weight: row.IsFreeDay ? "700" : "400"));

                for (var h = 0; h < 24; h++)
                {
                    var x = labelW + h * (cellW + gap);
                    var value = row.Hours.Count > h ? row.Hours[h] : null;
                    var fill = value == null || min == null || max == null || max == min
                        ? $"fill=\"{Grid}\""
                        : $"fill=\"{Series}\" fill-opacity=\"{opacities[Math.Min(4, (int)((value.Value - min.Value) / (max.Value - min.Value) * 5))]}\"";
                    svg.Append($"<rect x=\"{F(x)}\" y=\"{F(y)}\" width=\"{F(cellW)}\" height=\"{F(cellH)}\" rx=\"2\" {fill}/>");
                }
            }

            var axisY = rows.Count * (cellH + gap) + 9;
            foreach (var hour in new[] { 0, 6, 12, 18 })
                svg.Append(Text(labelW + hour * (cellW + gap), axisY, $"{hour} h", 7.5, Axis));

            // Legenda škály: min → max v jednom odstínu
            var legendY = axisY + 8;
            var stepW = 36.0;
            svg.Append(Text(labelW, legendY + 8, Format(min), 7.5, Axis, "end"));
            for (var i = 0; i < opacities.Length; i++)
                svg.Append($"<rect x=\"{F(labelW + 4 + i * stepW)}\" y=\"{F(legendY)}\" width=\"{F(stepW)}\" height=\"8\" fill=\"{Series}\" fill-opacity=\"{opacities[i]}\"/>");
            svg.Append(Text(labelW + 8 + opacities.Length * stepW, legendY + 8, $"{Format(max)} {Escape(unit)}", 7.5, Axis));

            return End(svg);
        }

        // Řez budovou: podlaží shora dolů, zóna nad limitem CO₂ má stavovou barvu a textový popisek
        public static string BuildingSection(int floors, IReadOnlyList<SectionZone> zones, double co2Limit)
        {
            const double width = 520, rowH = 34, gap = 4, labelW = 50;
            var height = Math.Max(1, floors) * (rowH + gap);
            var svg = Start(width, height);

            for (var i = 0; i < floors; i++)
            {
                var floor = floors - 1 - i;
                var y = i * (rowH + gap);
                var onFloor = zones.Where(z => z.Floor == floor).ToList();

                svg.Append(Text(0, y + rowH / 2 + 3, floor == 0 ? "přízemí" : $"{floor}. NP", 8, Axis));

                if (onFloor.Count == 0)
                {
                    svg.Append($"<rect x=\"{labelW}\" y=\"{F(y)}\" width=\"{F(width - labelW)}\" height=\"{rowH}\" rx=\"5\" fill=\"none\" stroke=\"{ReportStyles.Border}\" stroke-dasharray=\"3 3\"/>");
                    svg.Append(Text(labelW + 8, y + rowH / 2 + 3, "bez senzoru", 8, Axis));
                    continue;
                }

                var zoneW = (width - labelW - (onFloor.Count - 1) * gap) / onFloor.Count;
                for (var z = 0; z < onFloor.Count; z++)
                {
                    var zone = onFloor[z];
                    var x = labelW + z * (zoneW + gap);
                    var (fill, stroke, ink, tag) = zone.Co2 switch
                    {
                        > 0 when zone.Co2 > co2Limit => ("#FEF2F2", "#FECACA", "#991B1B", " · nad limitem"),
                        > 1000 => ("#FFFBEB", "#FDE68A", "#92400E", " · zvýšené"),
                        _ => (ReportStyles.Surface, ReportStyles.Border, ReportStyles.Navy, ""),
                    };

                    svg.Append($"<rect x=\"{F(x)}\" y=\"{F(y)}\" width=\"{F(zoneW)}\" height=\"{rowH}\" rx=\"5\" fill=\"{fill}\" stroke=\"{stroke}\"/>");
                    svg.Append(Text(x + 6, y + 13, zone.Name + tag, 8, ink, weight: "700"));
                    svg.Append(Text(x + 6, y + 26, $"{Format(zone.Temperature)} °C · {Format(zone.Co2)} ppm", 8, ink));
                }
            }

            return End(svg);
        }

        // Vodorovné sloupce pro portfolio (ztráta podle ORP) — hodnota na konci sloupce
        public static string HorizontalBars(IReadOnlyList<ChartBar> bars, string unit)
        {
            const double width = 520, rowH = 16, gap = 5, labelW = 130, valueW = 90;
            var height = Math.Max(1, bars.Count) * (rowH + gap);
            var svg = Start(width, height);

            if (bars.Count == 0)
                return Empty(svg, width, 40, "Žádné ztráty ve výběru.");

            var max = Math.Max(1, bars.Max(b => b.Value));
            var plotW = width - labelW - valueW;

            for (var i = 0; i < bars.Count; i++)
            {
                var bar = bars[i];
                var y = i * (rowH + gap);
                var w = Math.Max(1, bar.Value / max * plotW);

                svg.Append(Text(labelW - 6, y + rowH - 4, bar.Label, 8, ReportStyles.Navy, "end"));
                svg.Append($"<path d=\"M{F(labelW)},{F(y)} h{F(w - 4)} a4,4 0 0 1 4,4 v{F(rowH - 8)} a4,4 0 0 1 -4,4 h{F(-(w - 4))} z\" fill=\"{Series}\"/>");
                svg.Append(Text(labelW + w + 6, y + rowH - 4, $"{bar.Value.ToString("N0", Cs)} {unit}", 8, ReportStyles.Muted));
            }

            return End(svg);
        }

        #region SVG pomocné

        private static StringBuilder Start(double width, double height)
        {
            return new StringBuilder($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{F(width)}\" height=\"{F(height)}\" viewBox=\"0 0 {F(width)} {F(height)}\">");
        }

        private static string End(StringBuilder svg) => svg.Append("</svg>").ToString();

        private static string Empty(StringBuilder svg, double width, double height, string message)
        {
            svg.Append(Text(width / 2, height / 2, message, 9, Axis, "middle"));
            return End(svg);
        }

        private static string Line(double x1, double y1, double x2, double y2, string color, double strokeWidth)
        {
            return $"<line x1=\"{F(x1)}\" y1=\"{F(y1)}\" x2=\"{F(x2)}\" y2=\"{F(y2)}\" stroke=\"{color}\" stroke-width=\"{F(strokeWidth)}\"/>";
        }

        private static string RoundedTopBar(double x, double y, double w, double h, string color)
        {
            var r = Math.Min(4, Math.Min(w / 2, h));
            return $"<path d=\"M{F(x)},{F(y + h)} V{F(y + r)} Q{F(x)},{F(y)} {F(x + r)},{F(y)} H{F(x + w - r)} Q{F(x + w)},{F(y)} {F(x + w)},{F(y + r)} V{F(y + h)} Z\" fill=\"{color}\"/>";
        }

        private static string Text(double x, double y, string text, double size, string color, string anchor = "start", string weight = "400")
        {
            return $"<text x=\"{F(x)}\" y=\"{F(y)}\" font-family=\"{Font}\" font-size=\"{F(size)}\" font-weight=\"{weight}\" fill=\"{color}\" text-anchor=\"{anchor}\">{Escape(text)}</text>";
        }

        private static string Escape(string text) => SecurityElement.Escape(text) ?? string.Empty;

        private static string Format(double? value) => value == null ? "—" : value.Value.ToString("0.#", Cs);

        private static string F(double value) => value.ToString("0.##", Inv);

        #endregion
    }
}
