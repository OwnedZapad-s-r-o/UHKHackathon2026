using System.ClientModel;
using System.Text.Json;
using MediatR;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services;
using SmartAudit.Api.Services.Guide;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Guide.AskGuide
{
    // Tělo požadavku: otázka, odkud se uživatel ptá (stránka, vybraný objekt a okres) a jeho předchozí otázky pro navazující dotazy.
    // Předchozí odpovědi průvodce se záměrně neposílají — klient by je mohl podvrhnout a „naučit“ model jiná pravidla.
    public record AskGuideRequest(string Question, GuideContextRequest? Context, IReadOnlyCollection<string>? History);

    public record GuideContextRequest(string? Page, Guid? FacilityId, string? DistrictCode);

    public record AskGuideCommand(string Question, GuideAskContext Context, IReadOnlyCollection<string> History, ViewerContext Viewer) : IRequest<Response<AskGuideDto>>;

    public enum GuideMode
    {
        // Odpověď složil jazykový model nad nástroji a znalostmi
        Ai,
        // Záložní režim bez modelu (není nastavený, neodpověděl, nebo odpověď neprošla kontrolou)
        Offline,
        // Dotaz zastavily ochranné filtry
        Blocked,
    }

    public record AskGuideDto(
        string Answer,
        GuideMode Mode,
        IReadOnlyCollection<GuideFacilityCardDto> Facilities,
        IReadOnlyCollection<GuideDistrictCardDto> Districts,
        IReadOnlyCollection<GuideSourceDto> Sources,
        GuideMapActionDto? MapAction,
        IReadOnlyCollection<GuideLink> Links,
        IReadOnlyCollection<string> Suggestions,
        IReadOnlyCollection<GuideChart> Charts);

    // Rank = číslo karty i špendlíku v mapě
    public record GuideFacilityCardDto(
        int Rank,
        Guid Id,
        string Code,
        string Name,
        FacilityCategory Category,
        string Kind,
        string MunicipalityName,
        string DistrictCode,
        string DistrictName,
        double Latitude,
        double Longitude,
        AuditStatus Status,
        string Headline,
        double? Co2,
        double? Co2HoursOverLimit,
        double? Temperature,
        decimal? AnnualLossCzk,
        bool HasLiveSensors);

    public record GuideDistrictCardDto(string Code, string Name, int Facilities, int WithIssues, int Critical, decimal? AnnualLossCzk, double? AverageCo2);

    // Odkud odpověď čerpala — kapitola znalostí, datová sada nebo živá data aplikace
    public record GuideSourceDto(string Title, string Snippet, string Kind, string? Url);

    // Povel pro mapu: Facility (přelet a panel), Facilities (očíslované špendlíky), District, Region
    public record GuideMapActionDto(GuideFocus Type, IReadOnlyCollection<Guid> FacilityIds, string? DistrictCode, string? Panel);

    public class AskGuideHandler : IRequestHandler<AskGuideCommand, Response<AskGuideDto>>
    {
        private const int MaxHistory = 4;
        private const int SnippetLength = 180;

        // Odkazy, které dostane i veřejnost — ostatní stránky jsou za přihlášením
        private static readonly string[] PublicLinkPrefixes = ["/objekty", "/api/open/"];

        private readonly IGuideDataService _data;
        private readonly IGuideAgent _agent;
        private readonly IGuideFallback _fallback;
        private readonly IAiClientFactory _clients;
        private readonly IGuideChartBuilder _charts;
        private readonly ILogger<AskGuideHandler> _logger;

        public AskGuideHandler(IGuideDataService data, IGuideAgent agent, IGuideFallback fallback, IAiClientFactory clients, IGuideChartBuilder charts, ILogger<AskGuideHandler> logger)
        {
            _charts = charts;
            _data = data;
            _agent = agent;
            _fallback = fallback;
            _clients = clients;
            _logger = logger;
        }

        public async Task<Response<AskGuideDto>> Handle(AskGuideCommand request, CancellationToken ct)
        {
            _data.UseViewer(request.Viewer);

            // 1. vrstva: ochranné filtry ještě před modelem — zablokovaný dotaz se k modelu vůbec nedostane
            var verdict = GuideGuard.CheckInput(request.Question);
            if (!verdict.Allowed)
            {
                _logger.LogInformation("Průvodce zablokoval dotaz ({Reason})", verdict.Reason);
                return Response.Success(Blocked(verdict.Reason));
            }

            var question = request.Question.Trim();

            // Z historie jen otázky, které by filtry pustily — zablokovaná otázka nesmí proklouznout jako „kontext“
            var history = request.History
                .Where(h => GuideGuard.CheckInput(h).Allowed)
                .TakeLast(MaxHistory)
                .ToList();

            var (draft, mode) = await AnswerAsync(question, request.Context, history, ct);
            if (draft == null)
                return Response.Success(Blocked(GuardReason.OffTopic));

            var charts = await _charts.BuildAsync(draft, question, request.Viewer, ct);

            return Response.Success(ToDto(draft, mode, charts));
        }

        private async Task<(GuideDraft? Draft, GuideMode Mode)> AnswerAsync(string question, GuideAskContext context, IReadOnlyList<string> history, CancellationToken ct)
        {
            if (_clients.IsChatConfigured)
            {
                try
                {
                    var draft = await _agent.AskAsync(question, context, history, ct);

                    // 2. vrstva je systémový prompt: model sám označil dotaz jako mimo účel.
                    // Je-li dotaz zjevně o budovách (falešný poplach modelu), odpoví bezpečný záložní režim z dat.
                    if (draft == null)
                    {
                        if (!GuideFallback.LooksLikeDomainQuestion(question))
                            return (null, GuideMode.Blocked);

                        _logger.LogInformation("Model odmítl dotaz o budovách, odpovídá záložní režim");
                        return (await _fallback.AskAsync(question, context, ct), GuideMode.Offline);
                    }

                    // 3. vrstva: kontrola odpovědi — kód, recept nebo únik instrukcí se uživateli nikdy neukáže
                    if (GuideGuard.IsSafeOutput(draft.Answer, GuideAgent.Canary))
                        return (draft, GuideMode.Ai);

                    _logger.LogWarning("Odpověď modelu neprošla kontrolou průvodce, použije se záložní režim");
                }
                catch (Exception ex) when (IsAiFailure(ex) && !ct.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "Model průvodce neodpověděl, použije se záložní režim");
                }
            }

            return (await _fallback.AskAsync(question, context, ct), GuideMode.Offline);
        }

        private AskGuideDto Blocked(GuardReason reason)
        {
            var links = reason == GuardReason.Code ? [new GuideLink("Dokumentace otevřeného API", "/api/open/docs/")] : Array.Empty<GuideLink>();

            return new AskGuideDto(GuideGuard.RefusalMessage(reason), GuideMode.Blocked, [], [], [], null, links, GuideFallback.DefaultSuggestions(_data.CanSeeMoney), []);
        }

        private AskGuideDto ToDto(GuideDraft draft, GuideMode mode, IReadOnlyCollection<GuideChart> charts)
        {
            var facilities = draft.Facilities
                .Select((f, i) => new GuideFacilityCardDto(
                    i + 1, f.Id, f.Code, f.Name, f.Category, f.Kind, f.MunicipalityName, f.DistrictCode, f.DistrictName, f.Latitude, f.Longitude,
                    f.Status, f.Headline, f.Co2, f.Co2HoursOverLimit, f.Temperature, _data.CanSeeMoney ? f.AnnualLossCzk : null, f.HasLiveSensors))
                .ToList();

            var districts = draft.Districts
                .Select(d => new GuideDistrictCardDto(d.Code, d.Name, d.Facilities, d.WithIssues, d.Critical, _data.CanSeeMoney ? d.AnnualLossCzk : null, d.AverageCo2))
                .ToList();

            var sources = draft.Knowledge
                .Select(k => new GuideSourceDto(k.Chunk.Title, Snippet(k.Chunk.Text), k.Chunk.Source, k.Chunk.Url))
                .ToList();

            if (draft.UsedFacilityData)
                sources.Add(new GuideSourceDto("Portfolio objektů kraje", "Objekty z otevřených dat Královéhradeckého kraje, budovy z katastru ČÚZK a audit posledních 7 dní.", "Živá data aplikace", "https://www.datakhk.cz"));

            var links = draft.Knowledge
                .SelectMany(k => k.Chunk.Links)
                .Where(l => _data.CanSeeMoney || PublicLinkPrefixes.Any(p => l.Href.StartsWith(p, StringComparison.Ordinal)))
                .DistinctBy(l => l.Href)
                .Take(3)
                .ToList();

            var suggestions = draft.Suggestions.Count > 0 ? draft.Suggestions : GuideFallback.DefaultSuggestions(_data.CanSeeMoney);

            return new AskGuideDto(draft.Answer, mode, facilities, districts, sources, MapAction(draft), links, suggestions, charts);
        }

        private static GuideMapActionDto? MapAction(GuideDraft draft) => draft.Focus switch
        {
            GuideFocus.Facility or GuideFocus.Facilities => new GuideMapActionDto(draft.Focus, draft.Facilities.Select(f => f.Id).ToList(), draft.Facilities[0].DistrictCode, draft.Panel),
            GuideFocus.District => new GuideMapActionDto(GuideFocus.District, [], draft.DistrictCode, null),
            GuideFocus.Region => new GuideMapActionDto(GuideFocus.Region, [], null, null),
            _ => null,
        };

        private static string Snippet(string text) => text.Length <= SnippetLength ? text : text[..SnippetLength].TrimEnd() + "…";

        // SDK chyby sítě po opakovaných pokusech obalí do AggregateException; timeout průvodce = OperationCanceledException
        private static bool IsAiFailure(Exception ex)
        {
            return ex switch
            {
                ClientResultException or JsonException or HttpRequestException or OperationCanceledException or InvalidOperationException => true,
                AggregateException aggregate => aggregate.InnerExceptions.All(IsAiFailure),
                _ => false,
            };
        }
    }
}
