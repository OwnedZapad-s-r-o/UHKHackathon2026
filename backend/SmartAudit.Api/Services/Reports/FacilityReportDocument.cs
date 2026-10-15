using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAudit.Api.Features.Facilities.GetFacilityDetail;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services.Reports
{
    public record AiAnalysisSection(string Summary, string RootCause, IReadOnlyList<AiRecommendationSection> Recommendations, string? LetterDraft, DateTimeOffset CreatedAt, string Model);

    public record AiRecommendationSection(string Title, string Detail, string? Cost, string? Saving, string? Payback, bool HeritageSafe);

    public record FacilityReportData(
        GetFacilityDetailDto Facility,
        IReadOnlyList<ChartPoint> Temperature,
        IReadOnlyList<ChartBar> DailyEnergy,
        IReadOnlyList<HeatmapRow> Heatmap,
        double? HeatmapMin,
        double? HeatmapMax,
        int Floors,
        IReadOnlyList<SectionZone> Zones,
        double Co2Limit,
        AiAnalysisSection? AiAnalysis,
        IReadOnlyList<MapDistrict> Districts,
        DateTimeOffset GeneratedAt);

    // Energetický audit jednoho objektu — karta, zjištění, grafy telemetrie, budova a kontext z otevřených dat
    public class FacilityReportDocument : IDocument
    {
        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");

        // Schematický řez budovou je zatím skrytý (dispozici z otevřených dat neznáme); implementace zůstává
        private static readonly bool ShowBuildingSection = false;

        private readonly FacilityReportData _data;

        public FacilityReportDocument(FacilityReportData data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => new()
        {
            Title = $"Energetický audit — {_data.Facility.Name}",
            Author = "SmartAudit",
            Language = "cs-CZ",
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(style => style.FontFamily(ReportStyles.FontFamily).FontSize(9.5f).FontColor(ReportStyles.Navy));

                page.Header().Element(header => ReportStyles.Header(header, "Energetický audit objektu", $"SmartAudit · vygenerováno {_data.GeneratedAt.ToLocalTime().ToString("d. M. yyyy H:mm", Cs)}"));
                page.Content().PaddingTop(12).Column(Content);
                page.Footer().PaddingTop(8).Element(ReportStyles.Footer);
            });
        }

        private void Content(ColumnDescriptor column)
        {
            var facility = _data.Facility;
            var assessment = facility.Assessment;
            var status = ReportStyles.Status(assessment?.Status ?? AuditStatus.Nominal);

            // Karta objektu
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(title =>
                {
                    title.Item().Text(facility.Code).FontSize(8).FontColor(ReportStyles.Blue);
                    title.Item().PaddingTop(2).Text(facility.Name).FontSize(18).Bold();
                    title.Item().PaddingTop(2).Text(facility.Address.Label).FontSize(9).FontColor(ReportStyles.Muted);
                });
                // Stav a lokalizační mapka — kde v kraji objekt leží (okres zvýrazněný)
                row.ConstantItem(150).Column(side =>
                {
                    side.Item().AlignRight().Width(120)
                        .Background(status.Background).Border(1).BorderColor(status.Foreground).CornerRadius(10).PaddingVertical(4).PaddingHorizontal(8)
                        .AlignCenter().Text(status.Label).FontSize(8.5f).Bold().FontColor(status.Foreground);
                    side.Item().PaddingTop(6).Svg(ReportCharts.DistrictMap(
                        _data.Districts,
                        [new MapPoint(facility.Longitude, facility.Latitude, assessment?.Status ?? AuditStatus.Nominal)],
                        facility.Address.DistrictCode,
                        150,
                        88,
                        detailed: false));
                });
            });

            // Ukazatele
            column.Item().PaddingTop(12).Row(row =>
            {
                row.Spacing(8);
                Kpi(row.RelativeItem(), "Aktuální teplota", Format(assessment?.Temperature, "°C", "N1"));
                row.RelativeItem().Card().Column(kpi =>
                {
                    kpi.Item().Text(text =>
                    {
                        text.DefaultTextStyle(style => style.FontSize(8).FontColor(ReportStyles.Muted));
                        text.Co2();
                        text.Span(" v nejhorší zóně");
                    });
                    kpi.Item().PaddingTop(3).Text(Format(assessment?.Co2, "ppm")).FontSize(13).Bold();
                });
                Kpi(row.RelativeItem(), "Spotřeba za 7 dní", Format(assessment?.WeeklyEnergyKwh, "kWh"));
                Kpi(row.RelativeItem(), "Odhad roční ztráty", assessment?.AnnualLossCzk is { } loss ? loss.ToString("C0", Cs) : "jen pro kraj", assessment?.AnnualLossCzk > 0);
            });

            // Vyhodnocení — režim dat musí být v reportu pro Radu kraje vidět stejně jako ve webu
            var live = facility.DataMode == "LiveIqrf";
            column.Item().SectionTitle(live ? "Vyhodnocení — živé měření IQRF (MQTT)" : "Vyhodnocení — fyzikální model z otevřených dat");
            column.Item().PaddingBottom(4).Text(live
                    ? "Teplotu měří čidla MICRORISC IQRF přes bránu a MQTT; zjištění pre-auditu z otevřených dat jsou hypotézy, které měření ověřuje."
                    : "Hodnoty jsou deterministicky počítány z reálné podlahové plochy a stáří budovy (ČÚZK INSPIRE), kapacity osob a investic (Data KHK), cen energií (Registr smluv) a živého počasí (Open-Meteo). Po spárování brány MICRORISC IQRF se model automaticky přepne na živou telemetrii.")
                .FontSize(7.5f).FontColor(ReportStyles.Muted);
            column.Item().Background(status.Background).Border(1).BorderColor(status.Foreground).CornerRadius(8).Padding(10).Column(evaluation =>
            {
                // Bez zjištění shrnutí, jinak jednotlivá zjištění (shrnutí je jen jejich spojení — neopakovat)
                if (assessment is not { Findings.Count: > 0 })
                {
                    evaluation.Item().Text(assessment?.Headline ?? "Čeká na vyhodnocení").Bold().FontColor(status.Foreground);
                    evaluation.Item().PaddingTop(4).Text(assessment?.Summary ?? "—").FontColor(status.Foreground);
                    return;
                }

                foreach (var finding in assessment.Findings)
                {
                    evaluation.Item().PaddingBottom(4).Row(row =>
                    {
                        row.RelativeItem().Column(item =>
                        {
                            item.Item().Text(finding.Title).Bold().FontColor(status.Foreground);
                            item.Item().Text(finding.Detail).FontSize(8.5f).FontColor(status.Foreground);
                            if (finding.Source != null)
                                item.Item().Text($"Zdroj: {finding.Source}").FontSize(7.5f).Italic().FontColor(status.Foreground);
                        });
                        row.ConstantItem(80).AlignRight().Text(finding.Tag).FontSize(8).FontColor(status.Foreground);
                    });
                }
            });

            if (facility.PreAudit != null)
                column.Item().Element(PreAuditSection);

            if (facility.IqrfRoi != null)
                column.Item().Element(IqrfSection);

            // Grafy
            column.Item().SectionTitle("Teplota za 48 h (průměr zón)");
            column.Item().Svg(ReportCharts.TemperatureLine(_data.Temperature, 18));

            column.Item().SectionTitle("Spotřeba energie po dnech");
            column.Item().Svg(ReportCharts.Columns(_data.DailyEnergy, "kWh / den"));

            column.Item().ShowEntire().Column(heatmap =>
            {
                heatmap.Item().SectionTitle("Provozní heatmapa — příkon hodina × den");
                heatmap.Item().Svg(ReportCharts.Heatmap(_data.Heatmap, _data.HeatmapMin, _data.HeatmapMax, "kWh/h"));
            });

            if (ShowBuildingSection)
            {
                column.Item().ShowEntire().Column(section =>
                {
                    section.Item().SectionTitle("Řez budovou");
                    section.Item().Svg(ReportCharts.BuildingSection(_data.Floors, _data.Zones, _data.Co2Limit));
                    section.Item().PaddingTop(4).Text("Schéma podle druhu zařízení a odhadu podlaží — otevřená data vnitřní dispozici budov neobsahují.")
                        .FontSize(7.5f).FontColor(ReportStyles.Muted);
                });
            }

            // Budova a kontext z otevřených dat
            column.Item().ShowEntire().Column(context =>
            {
                context.Item().SectionTitle("Budova a kontext z otevřených dat");
                context.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                    });

                    foreach (var (label, value) in ContextRows())
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(ReportStyles.Border).PaddingVertical(4).Text(label).FontSize(8.5f).FontColor(ReportStyles.Muted);
                        table.Cell().BorderBottom(0.5f).BorderColor(ReportStyles.Border).PaddingVertical(4).Text(value).FontSize(8.5f).Bold();
                    }
                });
            });

            if (_data.AiAnalysis != null)
                column.Item().Element(AiSection);

            column.Item().Element(SourcesSection);
        }

        // Pět parametrů pre-auditu se zdroji — z čeho stav objektu vznikl
        private void PreAuditSection(IContainer container)
        {
            var preAudit = _data.Facility.PreAudit!;

            container.ShowEntire().Column(column =>
            {
                column.Item().SectionTitle($"Pre-audit z otevřených dat — {preAudit.Score}/100 bodů");
                column.Item().Text($"Hlavní příčina: {preAudit.DominantDriver} · měrná ztráta {preAudit.SpecificHeatLoss.ToString("0.0#", Cs)} W/m²K · {preAudit.DegreeDays.ToString("N0", Cs)} denostupňů · potřeba tepla {(preAudit.AnnualHeatDemandKwh / 1000).ToString("N0", Cs)} MWh/rok")
                    .FontSize(8).FontColor(ReportStyles.Muted);
                column.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(22);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(40);
                        columns.RelativeColumn(6);
                    });

                    foreach (var parameter in preAudit.Parameters)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(ReportStyles.Border).PaddingVertical(4).Text(parameter.Key).Bold();
                        table.Cell().BorderBottom(0.5f).BorderColor(ReportStyles.Border).PaddingVertical(4).Column(name =>
                        {
                            name.Item().Text(parameter.Name).FontSize(8.5f).Bold();
                            name.Item().Text(parameter.Value).FontSize(8).FontColor(ReportStyles.Muted);
                        });
                        table.Cell().BorderBottom(0.5f).BorderColor(ReportStyles.Border).PaddingVertical(4).Text($"{parameter.Score}/{parameter.MaxScore}").FontSize(8.5f).Bold();
                        table.Cell().BorderBottom(0.5f).BorderColor(ReportStyles.Border).PaddingVertical(4).Column(text =>
                        {
                            text.Item().Text(parameter.Explanation).FontSize(8);
                            text.Item().Text($"Zdroje: {string.Join(", ", parameter.Sources.Select(s => s.Title))}").FontSize(7).FontColor(ReportStyles.Muted);
                        });
                    }
                });
            });
        }

        // Návratnost osazení budovy čidly MICRORISC IQRF — pro Radu kraje: kolik to stojí a za kolik měsíců se to vrátí
        private void IqrfSection(IContainer container)
        {
            var roi = _data.Facility.IqrfRoi!;

            container.ShowEntire().Column(column =>
            {
                column.Item().SectionTitle("Návratnost nasazení čidel MICRORISC IQRF");
                column.Item().Row(row =>
                {
                    row.Spacing(8);
                    Kpi(row.RelativeItem(), "Sada IQRF (bez sekání kabelů)", $"{roi.EstimatedHardwareCostCzk.ToString("C0", Cs)}");
                    Kpi(row.RelativeItem(), "Roční úspora", roi.EstimatedAnnualSavingsCzk is { } savings ? savings.ToString("C0", Cs) : "jen pro kraj");
                    Kpi(row.RelativeItem(), "Návratnost", roi.PaybackMonths is { } months ? $"{months.ToString("0.#", Cs)} měs." : "—", roi.PaybackMonths is <= 6);
                });
                column.Item().PaddingTop(4).Text($"{roi.Gateways}× IQRF Gateway IQD-GW04 à {roi.GatewayUnitCzk.ToString("C0", Cs)} + {roi.Sensors}× bezdrátové čidlo teploty/CO₂ à {roi.SensorUnitCzk.ToString("C0", Cs)} podle podlahové plochy z ČÚZK. Úspora: {roi.SavingsBasis}.")
                    .FontSize(7.5f).FontColor(ReportStyles.Muted);
            });
        }

        // Čl. 7.3 pravidel — klikací odkazy na zdrojové datové sady
        private void SourcesSection(IContainer container)
        {
            var facility = _data.Facility;

            container.ShowEntire().Column(column =>
            {
                column.Item().SectionTitle("Zdroje otevřených dat u tohoto objektu");
                foreach (var dataset in facility.LinkedDatasets)
                {
                    column.Item().PaddingBottom(2).Text(text =>
                    {
                        text.DefaultTextStyle(style => style.FontSize(7.5f));
                        text.Span($"{dataset.Provider} · ").FontColor(ReportStyles.Muted);
                        text.Hyperlink(dataset.Title, dataset.CatalogUrl).FontColor(ReportStyles.Blue).Underline();
                    });
                }

                foreach (var link in facility.ObjectLinks)
                {
                    column.Item().PaddingBottom(2).Text(text =>
                    {
                        text.DefaultTextStyle(style => style.FontSize(7.5f));
                        text.Span($"{link.Provider} · ").FontColor(ReportStyles.Muted);
                        text.Hyperlink(link.Label, link.Url).FontColor(ReportStyles.Blue).Underline();
                    });
                }
            });
        }

        private void AiSection(IContainer container)
        {
            var ai = _data.AiAnalysis!;

            container.Column(column =>
            {
                column.Item().SectionTitle("AI analýza a doporučená opatření");
                column.Item().Text($"Model {ai.Model} · {ai.CreatedAt.ToLocalTime().ToString("d. M. yyyy H:mm", Cs)} · návrh k ověření energetickým specialistou")
                    .FontSize(7.5f).FontColor(ReportStyles.Muted);
                column.Item().PaddingTop(4).Text(ai.Summary);
                column.Item().PaddingTop(4).Text(text =>
                {
                    text.Span("Pravděpodobná příčina: ").Bold();
                    text.Span(ai.RootCause);
                });

                foreach (var recommendation in ai.Recommendations)
                {
                    column.Item().PaddingTop(6).Card().Column(item =>
                    {
                        item.Item().Text(recommendation.Title).Bold();
                        item.Item().PaddingTop(2).Text(recommendation.Detail).FontSize(8.5f);
                        item.Item().PaddingTop(3).Text(text =>
                        {
                            text.DefaultTextStyle(style => style.FontSize(8).FontColor(ReportStyles.Muted));
                            text.Span($"Náklady: {recommendation.Cost ?? "—"} · Úspora: {recommendation.Saving ?? "—"} · Návratnost: {recommendation.Payback ?? "—"}");
                            if (!recommendation.HeritageSafe)
                                text.Span(" · vyžaduje stanovisko památkové péče").FontColor("#92400E");
                        });
                    });
                }

                if (!string.IsNullOrWhiteSpace(ai.LetterDraft))
                {
                    column.Item().ShowEntire().Column(letter =>
                    {
                        letter.Item().SectionTitle("Návrh dopisu správci / zřizovateli");
                        letter.Item().Card().Text(ai.LetterDraft).FontSize(8.5f);
                    });
                }
            });
        }

        private IEnumerable<(string Label, string Value)> ContextRows()
        {
            var facility = _data.Facility;
            var building = facility.Building;

            yield return ("Zřizující odbor", facility.Department);
            yield return ("Organizace", facility.Organization is { } organization ? $"{organization.Name} (IČO {organization.Ico})" : "—");

            if (building is { Found: true })
            {
                yield return ("Rok výstavby (ČÚZK)", $"{building.ConstructionYear?.ToString() ?? "neuveden"} · {building.EnergyCategory}");
                yield return ("Zastavěná plocha · podlaží (odhad)", $"{Format(building.BuiltUpArea, "m²")} · {building.EstimatedFloors}");
                yield return ("Stavební objekt RÚIAN", building.RuianCode ?? "—");
            }

            var students = facility.Occupancy.Where(o => o.Students != null && o.SchoolYear != "neuvedeno").ToList();
            if (students.Count > 0)
                yield return ("Počet žáků", string.Join(" → ", students.OrderBy(o => o.SchoolYear).Select(o => $"{o.Students} ({o.SchoolYear})")));

            if (facility.Occupancy.FirstOrDefault(o => o.OccupancyPercent != null) is { } dormitory)
                yield return ("Obsazenost domova mládeže", $"{dormitory.OccupancyPercent?.ToString("0.#", Cs)} % ({dormitory.Residents}/{dormitory.BedCapacity} lůžek)");

            if (facility.Occupancy.FirstOrDefault(o => o.DemandRatio != null) is { } admissions)
                yield return ("Přihlášky na jedno místo", admissions.DemandRatio?.ToString("0.##", Cs) ?? "—");

            yield return ("Ohrožená oblast (HSOO)", facility.Territory.IsThreatenedArea ? facility.Territory.ThreatLevel ?? "ano" : "ne");

            if (facility.Territory.OrpGrants is { } grants)
                yield return ($"Dotace do ORP {grants.OrpName} 2021–2025", $"{grants.Count} dotací · {((grants.Total ?? 0) / 1_000_000m).ToString("0.#", Cs)} mil. Kč");

            yield return ("Památková ochrana", facility.Heritage.Status == HeritageStatus.None ? "ne" : $"{facility.Heritage.Name} — {facility.Heritage.Constraint}");

            if (facility.MunicipalityContact is { } contact)
                yield return ($"Obec {contact.Municipality}", $"datová schránka {contact.DataBox ?? "—"} · {contact.Email ?? "—"}");
        }

        private static void Kpi(IContainer container, string label, string value, bool alert = false)
        {
            container.Card().Column(kpi =>
            {
                kpi.Item().Text(label).FontSize(8).FontColor(ReportStyles.Muted);
                kpi.Item().PaddingTop(3).Text(value).FontSize(13).Bold().FontColor(alert ? "#991B1B" : ReportStyles.Navy);
            });
        }

        // Celá čísla bez desetinných míst (ppm, kWh, m²), teplota na desetiny
        private static string Format(double? value, string unit, string format = "N0")
        {
            return value == null ? "—" : $"{value.Value.ToString(format, Cs)} {unit}";
        }
    }
}
