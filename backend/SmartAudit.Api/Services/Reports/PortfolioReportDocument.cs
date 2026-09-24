using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAudit.Api.Features.Facilities.GetFacilities;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services.Reports
{
    public record PortfolioReportData(IReadOnlyList<GetFacilityDto> Facilities, string? DistrictCode, string? DistrictName, bool OnlyIssues, bool MoneyVisible, IReadOnlyList<MapDistrict> Districts, DateTimeOffset GeneratedAt);

    // Přehled portfolia pro investiční odbor — souhrn, ztráty podle ORP, pořadí objektů
    public class PortfolioReportDocument : IDocument
    {
        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");

        private static readonly Dictionary<FacilityCategory, string> CategoryLabels = new()
        {
            [FacilityCategory.School] = "Školy",
            [FacilityCategory.Social] = "Sociální služby",
            [FacilityCategory.Health] = "Zdravotnictví",
            [FacilityCategory.Culture] = "Kultura",
        };

        private readonly PortfolioReportData _data;

        public PortfolioReportDocument(PortfolioReportData data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => new()
        {
            Title = "Přehled portfolia objektů Královéhradeckého kraje",
            Author = "SmartAudit KHK",
            Language = "cs-CZ",
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(style => style.FontFamily(ReportStyles.FontFamily).FontSize(9).FontColor(ReportStyles.Navy));

                page.Header().Element(header => ReportStyles.Header(header, "Přehled portfolia objektů", $"SmartAudit KHK · vygenerováno {_data.GeneratedAt.ToLocalTime().ToString("d. M. yyyy H:mm", Cs)}"));
                page.Content().PaddingTop(12).Column(Content);
                page.Footer().PaddingTop(8).Element(ReportStyles.Footer);
            });
        }

        private void Content(ColumnDescriptor column)
        {
            var facilities = _data.Facilities;
            var scope = _data.DistrictName == null ? "Celý Královéhradecký kraj" : $"Okres {_data.DistrictName}";
            if (_data.OnlyIssues)
                scope += " · jen objekty se zjištěním";

            column.Item().Text("Portfolio objektů").FontSize(18).Bold();
            column.Item().PaddingTop(2).Text($"{scope} · objekty zřizované krajem napojené na telemetrii").FontColor(ReportStyles.Muted);

            // Souhrn
            column.Item().PaddingTop(12).Row(row =>
            {
                row.Spacing(8);
                Kpi(row.RelativeItem(), "Objektů ve výběru", facilities.Count.ToString("N0", Cs));
                Kpi(row.RelativeItem(), "Kritických anomálií", facilities.Count(f => f.Status == AuditStatus.Critical).ToString(), "#991B1B");
                Kpi(row.RelativeItem(), "Varování", facilities.Count(f => f.Status == AuditStatus.Warning).ToString(), "#92400E");
                Kpi(row.RelativeItem(), "Odhad ztrát / rok", _data.MoneyVisible ? facilities.Sum(f => f.AnnualLossCzk ?? 0).ToString("C0", Cs) : "jen pro kraj", "#991B1B");
            });

            // Mapa okresů — barva podle podílu objektů se zjištěním, vybraný okres zvýrazněný
            column.Item().ShowEntire().Column(map =>
            {
                map.Item().SectionTitle(_data.DistrictName == null ? "Okresy kraje" : $"Okres {_data.DistrictName} v kraji");
                map.Item().Svg(ReportCharts.DistrictMap(
                    _data.Districts,
                    facilities.Select(f => new MapPoint(f.Longitude, f.Latitude, f.Status)).ToList(),
                    _data.DistrictCode));
            });

            // Druhy objektů
            column.Item().SectionTitle("Objekty podle druhu");
            column.Item().Row(row =>
            {
                row.Spacing(8);
                foreach (var (category, label) in CategoryLabels)
                {
                    var inCategory = facilities.Where(f => f.Category == category).ToList();
                    row.RelativeItem().Card().Column(card =>
                    {
                        card.Item().Text(label).FontSize(8).FontColor(ReportStyles.Muted);
                        card.Item().PaddingTop(2).Text($"{inCategory.Count} objektů").Bold();
                        card.Item().Text($"{inCategory.Count(f => f.Status != AuditStatus.Nominal)} se zjištěním").FontSize(8).FontColor(ReportStyles.Muted);
                    });
                }
            });

            // Ztráty podle ORP
            if (_data.MoneyVisible)
            {
                var byOrp = facilities
                    .Where(f => (f.AnnualLossCzk ?? 0) > 0)
                    .GroupBy(f => f.OrpName)
                    .Select(g => new ChartBar(g.Key, (double)g.Sum(f => f.AnnualLossCzk ?? 0)))
                    .OrderByDescending(b => b.Value)
                    .ToList();

                column.Item().ShowEntire().Column(chart =>
                {
                    chart.Item().SectionTitle("Odhad roční ztráty podle ORP");
                    chart.Item().Svg(ReportCharts.HorizontalBars(byOrp, "Kč"));
                });
            }

            // Pořadí objektů
            column.Item().SectionTitle(_data.MoneyVisible ? "Objekty podle odhadované ztráty" : "Objekty podle závažnosti");
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(22);
                    columns.ConstantColumn(72);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    if (_data.MoneyVisible)
                        columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    string[] labels = _data.MoneyVisible ? ["#", "Kód", "Objekt", "Okres", "Stav", "Ztráta / rok"] : ["#", "Kód", "Objekt", "Okres", "Stav"];
                    foreach (var label in labels)
                        header.Cell().BorderBottom(1).BorderColor(ReportStyles.Border).PaddingVertical(4).Text(label).FontSize(7.5f).FontColor(ReportStyles.Muted);
                });

                var ordered = facilities
                    .OrderBy(f => f.Status switch { AuditStatus.Critical => 0, AuditStatus.Warning => 1, _ => 2 })
                    .ThenByDescending(f => f.AnnualLossCzk ?? 0)
                    .ThenBy(f => f.Code)
                    .ToList();

                for (var i = 0; i < ordered.Count; i++)
                {
                    var facility = ordered[i];
                    var status = ReportStyles.Status(facility.Status);

                    Cell(table, (i + 1).ToString(), ReportStyles.Muted);
                    Cell(table, facility.Code, ReportStyles.Blue);
                    Cell(table, facility.Name, ReportStyles.Navy, bold: true);
                    Cell(table, facility.DistrictName, ReportStyles.Navy);
                    Cell(table, facility.Status == AuditStatus.Nominal ? status.Label : facility.Headline, status.Foreground);
                    if (_data.MoneyVisible)
                        Cell(table, facility.AnnualLossCzk is > 0 ? facility.AnnualLossCzk.Value.ToString("C0", Cs) : "—", ReportStyles.Navy, alignRight: true);
                }
            });
        }

        private static void Cell(TableDescriptor table, string text, string color, bool bold = false, bool alignRight = false)
        {
            // Buňka se nedělí mezi stránky — řádek zůstane celý
            var cell = table.Cell().ShowEntire().BorderBottom(0.5f).BorderColor(ReportStyles.Border).PaddingVertical(3.5f);
            var aligned = alignRight ? cell.AlignRight() : cell;
            var span = aligned.Text(text).FontSize(8).FontColor(color);
            if (bold)
                span.Bold();
        }

        private static void Kpi(IContainer container, string label, string value, string? color = null)
        {
            container.Card().Column(kpi =>
            {
                kpi.Item().Text(label).FontSize(8).FontColor(ReportStyles.Muted);
                kpi.Item().PaddingTop(3).Text(value).FontSize(13).Bold().FontColor(color ?? ReportStyles.Navy);
            });
        }
    }
}
