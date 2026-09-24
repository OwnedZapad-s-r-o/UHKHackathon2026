using System.Text;
using NetTopologySuite.Geometries;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services.Reports
{
    // Mapa okresů pro PDF — stejná škála jako vrstva okresů ve webu (district-layer.tsx):
    // světlá = málo objektů se zjištěním, tmavá = hodně; vybraný okres červený obrys, ostatní ustoupí
    public static partial class ReportCharts
    {
        // Hranice tříd podílu objektů se zjištěním a sekvenční modrá škála (5 tříd)
        public static readonly double[] IssueShareBreaks = [0.15, 0.22, 0.29, 0.36];
        public static readonly string[] IssueShareColors = ["#EEF3F8", "#CBDAEA", "#9DB8D5", "#6A8FB8", "#3F6592"];

        private static readonly Dictionary<AuditStatus, string> StatusDot = new()
        {
            [AuditStatus.Nominal] = "#10B981",
            [AuditStatus.Warning] = "#F59E0B",
            [AuditStatus.Critical] = "#EF4444",
        };

        public static string IssueShareColor(double share)
        {
            var index = IssueShareBreaks.Count(b => share >= b);
            return IssueShareColors[index];
        }

        public static string DistrictMap(IReadOnlyList<MapDistrict> districts, IReadOnlyList<MapPoint> points, string? selectedCode, double width = 520, double height = 300, bool detailed = true)
        {
            var svg = Start(width, height);
            var shapes = districts.Where(d => !d.Geometry.IsEmpty).ToList();

            if (shapes.Count == 0)
                return Empty(svg, width, height, "Hranice okresů nejsou k dispozici.");

            var legendHeight = detailed ? 34.0 : 0;
            var envelope = new Envelope();
            foreach (var district in shapes)
                envelope.ExpandToInclude(district.Geometry.EnvelopeInternal);

            // Jednoduchá projekce: délka zkrácená kosinem střední šířky, aby kraj nebyl roztažený do šířky
            var lonScale = Math.Cos(envelope.Centre.Y * Math.PI / 180);
            var projectedWidth = envelope.Width * lonScale;
            var padding = 8.0;
            var scale = Math.Min((width - 2 * padding) / projectedWidth, (height - legendHeight - 2 * padding) / envelope.Height);
            var offsetX = (width - projectedWidth * scale) / 2;
            var offsetY = padding;

            (double X, double Y) Project(double lon, double lat) =>
                (offsetX + (lon - envelope.MinX) * lonScale * scale, offsetY + (envelope.MaxY - lat) * scale);

            var hasSelection = selectedCode != null && shapes.Any(d => d.Code == selectedCode);

            // Nejdřív ostatní okresy, vybraný nakonec — jeho obrys nesmí překrýt sousedé
            foreach (var district in shapes.OrderBy(d => d.Code == selectedCode))
            {
                var selected = district.Code == selectedCode;
                var fill = IssueShareColor(district.IssueShare);
                var opacity = hasSelection && !selected ? 0.35 : 1;
                var stroke = selected ? ReportStyles.Red : "#FFFFFF";
                var strokeWidth = selected ? 2.2 : 1;

                svg.Append($"<path d=\"{PathData(district.Geometry, Project)}\" fill=\"{fill}\" fill-opacity=\"{F(opacity)}\" fill-rule=\"evenodd\" stroke=\"{stroke}\" stroke-width=\"{F(strokeWidth)}\" stroke-linejoin=\"round\"/>");
            }

            foreach (var point in points.OrderBy(p => p.Status))
            {
                var (x, y) = Project(point.Longitude, point.Latitude);
                var radius = detailed ? 2.4 : 4;
                svg.Append($"<circle cx=\"{F(x)}\" cy=\"{F(y)}\" r=\"{F(radius)}\" fill=\"{StatusDot[point.Status]}\" stroke=\"#FFFFFF\" stroke-width=\"0.8\"/>");
            }

            if (detailed)
            {
                foreach (var district in shapes)
                {
                    var anchor = district.Geometry.InteriorPoint;
                    var (x, y) = Project(anchor.X, anchor.Y);
                    var dimmed = hasSelection && district.Code != selectedCode;
                    var color = dimmed ? ReportStyles.Muted : ReportStyles.Navy;

                    svg.Append(Text(x, y - 3, district.Name, 8.5, color, "middle", "700"));
                    svg.Append(Text(x, y + 8, $"{district.Issues} z {district.Total} se zjištěním", 7, color, "middle"));
                }

                AppendLegend(svg, height - legendHeight + 8);
            }

            return End(svg);
        }

        private static void AppendLegend(StringBuilder svg, double top)
        {
            svg.Append(Text(0, top + 8, "Podíl objektů se zjištěním", 7.5, ReportStyles.Muted));

            string[] labels = ["< 15 %", "15–22 %", "22–29 %", "29–36 %", "≥ 36 %"];
            for (var i = 0; i < IssueShareColors.Length; i++)
            {
                var x = 118 + i * 56;
                svg.Append($"<rect x=\"{F(x)}\" y=\"{F(top)}\" width=\"12\" height=\"10\" rx=\"2\" fill=\"{IssueShareColors[i]}\" stroke=\"#E2E8F0\" stroke-width=\"0.5\"/>");
                svg.Append(Text(x + 16, top + 8, labels[i], 7, ReportStyles.Navy));
            }

            var statuses = new (AuditStatus Status, string Label)[] { (AuditStatus.Critical, "kritická"), (AuditStatus.Warning, "varování"), (AuditStatus.Nominal, "v pořádku") };
            for (var i = 0; i < statuses.Length; i++)
            {
                var x = 118 + i * 70;
                svg.Append($"<circle cx=\"{F(x + 5)}\" cy=\"{F(top + 20)}\" r=\"3\" fill=\"{StatusDot[statuses[i].Status]}\"/>");
                svg.Append(Text(x + 12, top + 23, statuses[i].Label, 7, ReportStyles.Navy));
            }
            svg.Append(Text(0, top + 23, "Objekty", 7.5, ReportStyles.Muted));
        }

        private static string PathData(Geometry geometry, Func<double, double, (double X, double Y)> project)
        {
            var path = new StringBuilder();

            for (var i = 0; i < geometry.NumGeometries; i++)
            {
                if (geometry.GetGeometryN(i) is not Polygon polygon)
                    continue;

                AppendRing(path, polygon.ExteriorRing, project);
                foreach (var hole in polygon.InteriorRings)
                    AppendRing(path, hole, project);
            }

            return path.ToString();
        }

        private static void AppendRing(StringBuilder path, LineString ring, Func<double, double, (double X, double Y)> project)
        {
            var coordinates = ring.Coordinates;
            for (var i = 0; i < coordinates.Length; i++)
            {
                var (x, y) = project(coordinates[i].X, coordinates[i].Y);
                path.Append(i == 0 ? 'M' : 'L').Append(F(x)).Append(' ').Append(F(y)).Append(' ');
            }
            path.Append("Z ");
        }
    }
}
