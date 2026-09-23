using System.ClientModel;
using System.Text.Json;
using MediatR;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Facilities.GetFacilityDetail;
using SmartAudit.Api.Services;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.AiAudit.GenerateAiAudit
{
    public record GenerateAiAuditCommand(Guid FacilityId, Guid UserId, ViewerContext Viewer) : IRequest<Response<GenerateAiAuditDto>>;

    public record GenerateAiAuditDto(Guid Id, string Model, string Summary, string RootCause, IReadOnlyCollection<AiRecommendation> Recommendations, string? LetterDraft, DateTimeOffset CreatedAt);

    public class GenerateAiAuditHandler : IRequestHandler<GenerateAiAuditCommand, Response<GenerateAiAuditDto>>
    {
        public const string NotConfiguredMessage = "AI analýza není nastavená — doplňte AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY a AZURE_OPENAI_DEPLOYMENT.";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly SmartAuditDbContext _dbContext;
        private readonly IMediator _mediator;
        private readonly IAiAuditService _aiService;
        private readonly ILogger<GenerateAiAuditHandler> _logger;

        public GenerateAiAuditHandler(SmartAuditDbContext dbContext, IMediator mediator, IAiAuditService aiService, ILogger<GenerateAiAuditHandler> logger)
        {
            _dbContext = dbContext;
            _mediator = mediator;
            _aiService = aiService;
            _logger = logger;
        }

        public async Task<Response<GenerateAiAuditDto>> Handle(GenerateAiAuditCommand request, CancellationToken ct)
        {
            if (!_aiService.IsConfigured)
                return Response.Unavailable<GenerateAiAuditDto>(NotConfiguredMessage);

            // Stejná data jako inspekční panel — zároveň ověří existenci objektu a oprávnění správce
            var detail = await _mediator.Send(new GetFacilityDetailCommand(request.FacilityId, request.Viewer), ct);
            if (!detail.IsSuccess || detail.Data == null)
                return Response.Error<GenerateAiAuditDto>(detail.Message ?? "Objekt s tímto id neexistuje", detail.Status);

            AiAuditResult result;
            try
            {
                result = await _aiService.AnalyzeAsync(detail.Data, ct);
            }
            // SDK chyby sítě po opakovaných pokusech obalí do AggregateException
            catch (Exception ex) when (IsAiServiceFailure(ex))
            {
                _logger.LogWarning(ex, "Azure OpenAI nevrátilo použitelnou odpověď pro objekt {FacilityId}", request.FacilityId);
                return Response.Unavailable<GenerateAiAuditDto>("AI služba teď neodpověděla, zkuste to prosím za chvíli.");
            }

            var analysis = new AiAnalysis(
                request.FacilityId,
                result.Model,
                result.Summary,
                result.RootCause,
                JsonSerializer.Serialize(result.Recommendations, JsonOptions),
                result.LetterDraft,
                request.UserId);

            _dbContext.AiAnalyses.Add(analysis);

            await _dbContext.SaveChangesAsync(ct);

            var dto = new GenerateAiAuditDto(analysis.Id, analysis.Model, analysis.Summary, analysis.RootCause, result.Recommendations.ToList(), analysis.LetterDraft, analysis.CreatedAt);

            return Response.Success(dto);
        }

        private static bool IsAiServiceFailure(Exception ex)
        {
            return ex switch
            {
                ClientResultException or JsonException or HttpRequestException => true,
                AggregateException aggregate => aggregate.InnerExceptions.All(IsAiServiceFailure),
                _ => false,
            };
        }
    }
}
