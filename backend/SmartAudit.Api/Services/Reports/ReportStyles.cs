using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services.Reports
{
    // Vizuální styl PDF reportů — stejné barvy a písmo jako webová aplikace (prototyp HK)
    public static class ReportStyles
    {
        public const string FontFamily = "Hradec";

        // Barvy vizuálního stylu Hradce Králové
        public const string Navy = "#0B192C";
        public const string Blue = "#1E3E62";
        public const string Red = "#D90429";
        public const string Muted = "#64748B";
        public const string Border = "#E2E8F0";
        public const string Surface = "#F8FAFC";

        private static byte[]? _logo;

        public static byte[] Logo => _logo ?? throw new InvalidOperationException("Zdroje reportů nejsou načtené");

        // Písmo Hradec (TTF převedené z webových WOFF2) a černé logo HK — načítá se jednou při startu
        public static void Initialize(string contentRoot)
        {
            var resources = Path.Combine(contentRoot, "Resources");

            FontManager.RegisterFontFromFile(Path.Combine(resources, "Fonts", "Hradec-Regular.ttf"));
            FontManager.RegisterFontFromFile(Path.Combine(resources, "Fonts", "Hradec-Bold.ttf"));

            _logo = File.ReadAllBytes(Path.Combine(resources, "hk-logo-black.png"));
        }

        public static (string Label, string Foreground, string Background) Status(AuditStatus status)
        {
            return status switch
            {
                AuditStatus.Critical => ("Kritická anomálie", "#991B1B", "#FEF2F2"),
                AuditStatus.Warning => ("Varování", "#92400E", "#FFFBEB"),
                _ => ("Optimální chod", "#065F46", "#ECFDF5"),
            };
        }

        // Hradec nemá znak ₂ — CO₂ se skládá z „CO“ a dolního indexu
        public static void Co2(this TextDescriptor text)
        {
            text.Span("CO");
            text.Span("2").Subscript();
        }

        public static void SectionTitle(this IContainer container, string title)
        {
            container.PaddingTop(14).PaddingBottom(6).Text(title).FontSize(13).Bold().FontColor(Navy);
        }

        public static IContainer Card(this IContainer container)
        {
            return container.Border(1).BorderColor(Border).CornerRadius(8).Padding(10);
        }

        public static void Header(IContainer container, string title, string subtitle)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.ConstantItem(70).Image(Logo);
                    row.RelativeItem().AlignRight().AlignMiddle().Column(text =>
                    {
                        text.Item().AlignRight().Text(title).FontSize(11).Bold().FontColor(Navy);
                        text.Item().AlignRight().Text(subtitle).FontSize(8).FontColor(Muted);
                    });
                });
                column.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(Red);
            });
        }

        public static void Footer(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text("Zdroje dat: Data KHK (25 sad) · ČÚZK INSPIRE Budovy · Open-Meteo · kalendář MŠMT. Telemetrie je zatím simulovaná.")
                    .FontSize(7).FontColor(Muted);
                row.ConstantItem(60).AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(7).FontColor(Muted));
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }
    }
}
