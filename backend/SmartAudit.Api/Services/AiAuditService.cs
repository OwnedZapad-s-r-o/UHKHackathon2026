using System.Globalization;
using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using SmartAudit.Api.Features.Facilities.GetFacilityDetail;
using SmartAudit.Core.Domain.Enums;

namespace SmartAudit.Api.Services
{
    public record AiRecommendation(string Title, string Detail, string Cost, string Saving, string Payback, bool HeritageSafe);

    public record AiAuditResult(string Model, string Summary, string RootCause, IReadOnlyList<AiRecommendation> Recommendations, string LetterDraft);

    // Smlouva organizace z Registru smluv, jak ji dostane model — číslo a odkaz, aby šla v doporučení dohledat
    public record AiContractContext(string Title, ContractCategory Category, string PartnerName, string? ContractNumber, string RegistryUrl, decimal? AmountWithoutVat, decimal? AmountWithVat, DateOnly? DateSigned, double? UnitPriceCzkPerMwh);

    public interface IAiAuditService
    {
        bool IsConfigured { get; }
        Task<AiAuditResult> AnalyzeAsync(GetFacilityDetailDto facility, IReadOnlyCollection<AiContractContext> contracts, CancellationToken ct);
    }

    // AI analýza objektu přes Azure OpenAI — model dostane jen data z aplikace a pevný katalog opatření,
    // výstup je strukturovaný JSON (ne volný text), aby čísla šla ověřit a zobrazit v UI i v PDF
    public class AiAuditService : IAiAuditService
    {
        private static readonly CultureInfo Cs = CultureInfo.GetCultureInfo("cs-CZ");
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        // Orientační katalog opatření (SMARTAUDIT_FUNKCE.md, F29) — model z něj vybírá, ceny nevymýšlí
        private const string MeasureCatalog = """
            - Seřízení ekvitermní regulace a časových programů MaR — servisní zásah, cca 0–15 000 Kč; úspora typicky 5–15 % tepla; bez zásahu do obálky.
            - Víkendový a noční útlum v MaR — cca 0 Kč (nastavení); úspora podle délky útlumu; bez zásahu do obálky.
            - Posunutí ranního náběhu vytápění — cca 0 Kč (nastavení); bez zásahu do obálky.
            - Termostatické hlavice — cca 15 000 Kč na podlaží; bez zásahu do obálky.
            - CO₂ semafor / čidla s indikací a pravidla větrání o přestávkách — cca 3 000–5 000 Kč za místnost; bez zásahu do obálky.
            - Decentrální rekuperační jednotka do učebny — cca 400 000 Kč za místnost; zásah do fasády (prostup) — v památkové ochraně jen se souhlasem.
            - Výměna oken — statisíce Kč; zásah do obálky — v památkové ochraně jen se souhlasem.
            - Zateplení obálky budovy — miliony Kč; zásah do obálky — v památkové ochraně jen se souhlasem.
            """;

        private const string SystemPrompt = """
            Jsi energetický auditor Královéhradeckého kraje. Píšeš česky, věcně a stručně pro úředníka odboru investic a ředitele školy.
            Pravidla:
            - Používej výhradně čísla z dodaného kontextu. Nic nevymýšlej; když údaj chybí, napiš „údaj chybí“.
            - Opatření vybírej jen z katalogu opatření, můžeš je upřesnit pro konkrétní objekt. Seřaď je od nejlevnějšího s nejrychlejší návratností.
            - Návratnost počítej jen z uvedené roční ztráty v Kč a ceny z katalogu. Když ztráta chybí nebo je 0, napiš „nelze vyčíslit“.
            - U objektu v památkové ochraně označ opatření se zásahem do obálky heritageSafe = false.
            - Dopis je krátký formální dopis správci budovy (na vědomí zřizovateli) s konkrétními zjištěními a čísly. Místo jmen použij zástupné [jméno], [funkce].
            - Telemetrie je zatím simulovaná — neuváděj to jako problém objektu.
            - Pokud organizace disponuje servisní smlouvou na kotelnu, vytápění nebo MaR (sekce SMLOUVY, druh „servis“), doporuč seřízení
              a nastavení útlumu uplatnit v rámci této smlouvy — uveď dodavatele a číslo smlouvy (nebo odkaz do registru) a jako náklad
              „v rámci servisní smlouvy“. Smlouvy, které v kontextu nejsou, nevymýšlej.
            - Pokud je v SMLOUVÁCH stavební smlouva na zateplení, okna nebo snížení energetické náročnosti, zmiň ji jako souvislost
              a stejné opatření znovu nenavrhuj.
            """;

        private const string ResponseSchema = """
            {
              "type": "object",
              "properties": {
                "summary": { "type": "string", "description": "Shrnutí stavu objektu ve 2–4 větách" },
                "rootCause": { "type": "string", "description": "Nejpravděpodobnější příčina zjištění" },
                "recommendations": {
                  "type": "array",
                  "items": {
                    "type": "object",
                    "properties": {
                      "title": { "type": "string" },
                      "detail": { "type": "string" },
                      "cost": { "type": "string", "description": "Orientační náklady v Kč podle katalogu" },
                      "saving": { "type": "string", "description": "Odhad úspory za rok" },
                      "payback": { "type": "string", "description": "Návratnost, nebo „nelze vyčíslit“" },
                      "heritageSafe": { "type": "boolean" }
                    },
                    "required": ["title", "detail", "cost", "saving", "payback", "heritageSafe"],
                    "additionalProperties": false
                  }
                },
                "letterDraft": { "type": "string" }
              },
              "required": ["summary", "rootCause", "recommendations", "letterDraft"],
              "additionalProperties": false
            }
            """;

        private readonly IAiClientFactory _clients;

        public AiAuditService(IAiClientFactory clients)
        {
            _clients = clients;
        }

        public bool IsConfigured => _clients.IsChatConfigured;

        public async Task<AiAuditResult> AnalyzeAsync(GetFacilityDetailDto facility, IReadOnlyCollection<AiContractContext> contracts, CancellationToken ct)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("AI analýza není nastavená");

            var (chat, model) = _clients.CreateChatClient();

            var options = new ChatCompletionOptions
            {
                Temperature = 0.2f,
                MaxOutputTokenCount = 1800,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("smartaudit_ai_audit", BinaryData.FromString(ResponseSchema), jsonSchemaIsStrict: true),
            };

            ChatMessage[] messages = [new SystemChatMessage(SystemPrompt), new UserChatMessage(BuildContext(facility, contracts))];

            ChatCompletion completion = await chat.CompleteChatAsync(messages, options, ct);

            var output = JsonSerializer.Deserialize<AiOutput>(completion.Content[0].Text, JsonOptions)
                ?? throw new InvalidOperationException("Model vrátil prázdnou odpověď");

            return new AiAuditResult(
                model,
                output.Summary,
                output.RootCause,
                output.Recommendations.Select(r => new AiRecommendation(r.Title, r.Detail, r.Cost, r.Saving, r.Payback, r.HeritageSafe)).ToList(),
                output.LetterDraft);
        }

        // Kontext pro model — stejná data, jaká vidí uživatel v inspekčním panelu
        private static string BuildContext(GetFacilityDetailDto facility, IReadOnlyCollection<AiContractContext> contracts)
        {
            var text = new StringBuilder();
            var assessment = facility.Assessment;
            var building = facility.Building;

            text.AppendLine($"OBJEKT: {facility.Name} ({facility.Code}), {string.Join(", ", facility.Kinds)}");
            text.AppendLine($"Adresa: {facility.Address.Label}; okres {facility.Address.District}, ORP {facility.Address.Orp}");
            text.AppendLine($"Organizace: {facility.Organization?.Name ?? "neuvedena"}; {facility.Department}");

            if (building is { Found: true })
                text.AppendLine($"Budova (ČÚZK): rok výstavby {building.ConstructionYear?.ToString() ?? "neuveden"}, energetická kategorie {building.EnergyCategory}, zastavěná plocha {building.BuiltUpArea?.ToString("N0", Cs) ?? "?"} m², odhad podlaží {building.EstimatedFloors}");

            foreach (var occupancy in facility.Occupancy.Take(3))
                text.AppendLine($"Obsazenost {occupancy.SchoolYear}: žáci {occupancy.Students?.ToString() ?? "-"}, lůžka {occupancy.BedCapacity?.ToString() ?? "-"}, ubytovaní {occupancy.Residents?.ToString() ?? "-"}, přihlášky/místa {occupancy.DemandRatio?.ToString("0.##", Cs) ?? "-"}");

            text.AppendLine();
            text.AppendLine($"STAV AUDITU: {assessment?.Headline ?? "nevyhodnoceno"}");
            if (assessment != null)
            {
                foreach (var finding in assessment.Findings)
                    text.AppendLine($"- Zjištění ({finding.Tag}): {finding.Title} — {finding.Detail}");

                text.AppendLine($"Ukazatele 7 dní: hodin nad limitem CO₂ {assessment.Co2HoursOverLimit:0}, teplota ve dni útlumu 8–18 h {Format(assessment.FreeDayMeanTemperature)} °C, teplota ve 2 h ráno {Format(assessment.NightMeanTemperature)} °C, spotřeba {assessment.WeeklyEnergyKwh:N0} kWh");
                text.AppendLine($"Odhad roční ztráty: {(assessment.AnnualLossCzk is { } loss ? loss.ToString("N0", Cs) + " Kč" : "údaj chybí")}; potenciál úspory {Format(assessment.SavingPercent)} %");
            }

            text.AppendLine();
            text.AppendLine($"KONTEXT: hospodářsky ohrožená obec: {(facility.Territory.IsThreatenedArea ? facility.Territory.ThreatLevel ?? "ano" : "ne")}");
            text.AppendLine(facility.Heritage.Status == HeritageStatus.None
                ? "Památková ochrana: ne"
                : $"Památková ochrana: {facility.Heritage.Name} — {facility.Heritage.Constraint}");
            if (facility.Territory.OrpGrants is { } grants)
                text.AppendLine($"Dotace do ORP 2021–2025: {grants.Count} dotací, {((grants.Total ?? 0) / 1_000_000m).ToString("0.#", Cs)} mil. Kč");

            text.AppendLine();
            text.AppendLine("SMLOUVY ORGANIZACE V REGISTRU SMLUV (energie a provoz budovy):");
            if (contracts.Count == 0)
                text.AppendLine("- žádné nenalezeny");

            foreach (var contract in contracts)
            {
                var amount = contract.AmountWithoutVat is { } withoutVat ? $"{withoutVat.ToString("N0", Cs)} Kč bez DPH"
                    : contract.AmountWithVat is { } withVat ? $"{withVat.ToString("N0", Cs)} Kč vč. DPH" : "hodnota neuvedena";
                var price = contract.UnitPriceCzkPerMwh is { } unitPrice ? $", cena energie {unitPrice.ToString("N0", Cs)} Kč/MWh" : "";

                text.AppendLine($"- [{ContractLabel(contract.Category)}] {contract.Title}; dodavatel {contract.PartnerName}; č. {contract.ContractNumber ?? "neuvedeno"}; uzavřeno {contract.DateSigned?.ToString("d. M. yyyy", Cs) ?? "neuvedeno"}; {amount}{price}; {contract.RegistryUrl}");
            }

            text.AppendLine();
            text.AppendLine("KATALOG OPATŘENÍ:");
            text.AppendLine(MeasureCatalog);

            return text.ToString();
        }

        private static string ContractLabel(ContractCategory category) => category switch
        {
            ContractCategory.EnergySupply => "dodávka energie",
            ContractCategory.FacilityManagement => "servis",
            ContractCategory.Reconstruction => "stavební úpravy",
            _ => "jiné",
        };

        private static string Format(double? value) => value == null ? "údaj chybí" : value.Value.ToString("0.#", Cs);

        private sealed record AiOutput(string Summary, string RootCause, List<AiOutputRecommendation> Recommendations, string LetterDraft);

        private sealed record AiOutputRecommendation(string Title, string Detail, string Cost, string Saving, string Payback, bool HeritageSafe);
    }
}
