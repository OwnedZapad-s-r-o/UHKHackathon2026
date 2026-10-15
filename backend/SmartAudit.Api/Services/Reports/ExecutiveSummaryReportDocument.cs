using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAudit.Api.Features.Facilities.GetFacilities;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services.Reports
{
    public record ExecutiveSummaryData(
        IReadOnlyList<GetFacilityDto> Facilities,
        IReadOnlyList<MapDistrict> Districts,
        decimal SensorKitCzkPerBuilding,
        int PilotSize,
        DateTimeOffset GeneratedAt);

    // Souhrn pro Radu kraje na jednu stranu A4: kolik kraj ztrácí, kde, pět priorit, pilot IQRF s návratností a návrh usnesení.
    // Podrobnosti jsou v přehledu portfolia a auditech budov — tady jen to, o čem se hlasuje.
    public class ExecutiveSummaryReportDocument : IDocument
    {
        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");

        private readonly ExecutiveSummaryData _data;

        public ExecutiveSummaryReportDocument(ExecutiveSummaryData data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => new()
        {
            Title = "Souhrn pro Radu Královéhradeckého kraje — energetická efektivita budov",
            Author = "SmartAudit KHK",
            Language = "cs-CZ",
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(style => style.FontFamily(ReportStyles.FontFamily).FontSize(8.5f).FontColor(ReportStyles.Navy));

                page.Header().Element(header => ReportStyles.Header(header, "Souhrn pro Radu kraje", $"SmartAudit KHK · {_data.GeneratedAt.ToLocalTime().ToString("d. M. yyyy", Cs)}"));
                // Vždy jedna strana: kdyby obsah (dlouhé názvy budov) přetekl, zmenší se, místo aby pokračoval na další straně
                page.Content().PaddingTop(10).ScaleToFit().Column(Content);
                page.Footer().PaddingTop(6).Text("Ztráty jsou odhad z naměřené vnitřní teploty (kde čidlo zatím chybí, ze simulace) × tepelná ztráta budovy z ČÚZK × cena tepla. "
                    + "Zdroje: Data KHK, ČÚZK, Open-Meteo, MŠMT. Podrobnosti: přehled portfolia a PDF audit budovy v aplikaci SmartAudit KHK.")
                    .FontSize(6.5f).FontColor(ReportStyles.Muted);
            });
        }

        private void Content(ColumnDescriptor column)
        {
            var facilities = _data.Facilities;
            var withIssues = facilities.Where(f => f.Status != AuditStatus.Nominal).ToList();
            var totalLoss = facilities.Sum(f => f.AnnualLossCzk ?? 0);
            var priorities = facilities.Where(f => f.AnnualLossCzk > 0).OrderByDescending(f => f.AnnualLossCzk).Take(_data.PilotSize).ToList();
            var priorityLoss = priorities.Sum(f => f.AnnualLossCzk ?? 0);
            var pilotCost = _data.SensorKitCzkPerBuilding * priorities.Count;
            var payback = priorityLoss > 0 ? (double)(pilotCost / priorityLoss) : (double?)null;
            var topFinding = withIssues.GroupBy(f => f.Headline).OrderByDescending(g => g.Count()).FirstOrDefault();

            column.Item().Text("Kde kraj přichází o teplo a co s tím").FontSize(17).Bold();
            column.Item().PaddingTop(2).Text($"{facilities.Count} budov zřizovaných Královéhradeckým krajem — školy, domovy, nemocnice a kulturní zařízení. Vyhodnocení posledních 7 dní měření.")
                .FontColor(ReportStyles.Muted);

            // Čtyři čísla, o kterých se mluví
            column.Item().PaddingTop(10).Row(row =>
            {
                row.Spacing(6);
                Kpi(row.RelativeItem(), "Odhad ztráty za rok", Czk(totalLoss), "#991B1B", "přetápění mimo provoz a noc bez útlumu");
                Kpi(row.RelativeItem(), "Budovy se zjištěním", $"{withIssues.Count} z {facilities.Count}", "#92400E",
                    $"{withIssues.Count(f => f.Status == AuditStatus.Critical)} kritických, {withIssues.Count(f => f.Status == AuditStatus.Warning)} varování");
                Kpi(row.RelativeItem(), "Nejčastější zjištění", topFinding?.Key ?? "—", ReportStyles.Navy, topFinding == null ? null : $"{topFinding.Count()} budov");
                Kpi(row.RelativeItem(), $"Pilot IQRF na {priorities.Count} budovách", payback == null ? "—" : $"návratnost {payback.Value.ToString("0.0", Cs)} roku", "#065F46",
                    $"investice {Czk(pilotCost)}");
            });

            // Mapa okresů a pět priorit vedle sebe
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem(5).Column(map =>
                {
                    map.Item().Text("Podíl budov se zjištěním podle okresů").FontSize(10).Bold();
                    map.Item().PaddingTop(4).Svg(ReportCharts.DistrictMap(
                        _data.Districts,
                        facilities.Select(f => new MapPoint(f.Longitude, f.Latitude, f.Status)).ToList(),
                        null,
                        width: 300,
                        height: 210,
                        detailed: false));
                });

                row.RelativeItem(7).PaddingLeft(12).Column(top =>
                {
                    top.Item().Text($"{priorities.Count} priorit — budovy s nejvyšší ztrátou").FontSize(10).Bold();
                    top.Item().PaddingTop(1).Text(totalLoss > 0 ? $"Dohromady {Czk(priorityLoss)} za rok, tj. {priorityLoss / totalLoss:P0} ztráty celého kraje." : "Žádná budova nemá vyčíslenou ztrátu.")
                        .FontSize(7.5f).FontColor(ReportStyles.Muted);
                    top.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(14);
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(62);
                        });

                        table.Header(header =>
                        {
                            foreach (var label in new[] { "#", "Budova", "Zjištění", "Ztráta / rok" })
                                header.Cell().BorderBottom(1).BorderColor(ReportStyles.Border).PaddingBottom(3).Text(label).FontSize(7).FontColor(ReportStyles.Muted);
                        });

                        for (var i = 0; i < priorities.Count; i++)
                        {
                            var facility = priorities[i];
                            Cell(table, (i + 1).ToString(), ReportStyles.Muted);
                            table.Cell().BorderBottom(0.5f).BorderColor(ReportStyles.Border).PaddingVertical(3).Column(name =>
                            {
                                name.Item().Text(facility.Name).FontSize(7.5f).Bold();
                                name.Item().Text($"okres {facility.DistrictName}").FontSize(6.5f).FontColor(ReportStyles.Muted);
                            });
                            Cell(table, facility.Headline, ReportStyles.Status(facility.Status).Foreground);
                            Cell(table, Czk(facility.AnnualLossCzk ?? 0), "#991B1B", alignRight: true, bold: true);
                        }
                    });
                });
            });

            // Co audit zjistil — podle hlavního zjištění budovy, s počtem budov a jejich ztrátou
            var byFinding = withIssues
                .GroupBy(f => f.Headline)
                .Select(g => (Title: g.Key, Count: g.Count(), Loss: g.Sum(f => f.AnnualLossCzk ?? 0), Status: g.Max(f => f.Status)))
                .OrderByDescending(g => g.Loss)
                .ThenByDescending(g => g.Count)
                .Take(4)
                .ToList();

            if (byFinding.Count > 0)
            {
                column.Item().PaddingTop(12).Text("Co audit zjistil").FontSize(10).Bold();
                column.Item().PaddingTop(4).Row(row =>
                {
                    row.Spacing(6);
                    foreach (var finding in byFinding)
                    {
                        row.RelativeItem().Card().Column(card =>
                        {
                            card.Item().Text(finding.Title).FontSize(8).Bold().FontColor(ReportStyles.Status(finding.Status).Foreground);
                            card.Item().PaddingTop(2).Text($"{finding.Count} budov").FontSize(11).Bold();
                            card.Item().Text(finding.Loss > 0 ? $"{Czk(finding.Loss)} za rok" : "bez vyčíslené ztráty, vliv na zdraví a soustředění")
                                .FontSize(6.5f).FontColor(ReportStyles.Muted);
                        });
                    }
                });
            }

            // Návrh usnesení — to, o čem rada hlasuje
            column.Item().PaddingTop(12).Background(ReportStyles.Surface).Border(1).BorderColor(ReportStyles.Border).CornerRadius(6).Padding(10).Column(resolution =>
            {
                resolution.Item().Text("Návrh usnesení Rady Královéhradeckého kraje").FontSize(10).Bold();
                resolution.Item().PaddingTop(5).Text($"1. Schvaluje pilotní monitoring IQRF (teplota, CO₂, vlhkost) na {priorities.Count} prioritních budovách s investicí {Czk(pilotCost)} bez DPH.");
                resolution.Item().PaddingTop(2).Text("2. Ukládá ředitelům dotčených příspěvkových organizací do 30 dnů upravit časové programy a útlum vytápění podle zjištění auditu.");
                resolution.Item().PaddingTop(2).Text("3. Ukládá odboru investic vyhodnotit pilot po jedné topné sezóně podle naměřené úspory a navrhnout rozšíření na další budovy.");
                resolution.Item().PaddingTop(2).Text("4. Souhlasí se zveřejněním výsledků jako otevřených dat (CC BY 4.0) na portálu Data KHK.");
            });

            column.Item().PaddingTop(6).Text($"Návratnost pilotu předpokládá, že se zjištěná ztráta odstraní seřízením regulace (bez stavebních úprav). Cena sady čidel: {Czk(_data.SensorKitCzkPerBuilding)} bez DPH na budovu (gateway, 15 čidel do tříd, 3 do společných prostor, montáž).")
                .FontSize(7).FontColor(ReportStyles.Muted);
        }

        private static string Czk(decimal value) => value.ToString("C0", Cs);

        private static void Kpi(IContainer container, string label, string value, string color, string? note)
        {
            container.Card().Column(kpi =>
            {
                kpi.Item().Text(label).FontSize(7.5f).FontColor(ReportStyles.Muted);
                kpi.Item().PaddingTop(2).Text(value).FontSize(12).Bold().FontColor(color);
                if (note != null)
                    kpi.Item().PaddingTop(1).Text(note).FontSize(6.5f).FontColor(ReportStyles.Muted);
            });
        }

        private static void Cell(TableDescriptor table, string text, string color, bool alignRight = false, bool bold = false)
        {
            var cell = table.Cell().BorderBottom(0.5f).BorderColor(ReportStyles.Border).PaddingVertical(3).AlignMiddle();
            var span = (alignRight ? cell.AlignRight() : cell).Text(text).FontSize(7.5f).FontColor(color);
            if (bold)
                span.Bold();
        }
    }
}
