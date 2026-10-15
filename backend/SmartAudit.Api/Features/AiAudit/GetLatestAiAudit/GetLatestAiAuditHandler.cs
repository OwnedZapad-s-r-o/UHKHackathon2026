using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Services;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Features.AiAudit.GetLatestAiAudit
{
    public record GetLatestAiAuditCommand(Guid FacilityId, ViewerContext Viewer) : IRequest<Response<GetLatestAiAuditDto>>;

    // Configured říká UI, jestli má nabídnout tlačítko, nebo hlášku o chybějícím klíči
    public record GetLatestAiAuditDto(bool Configured, AiAuditAnalysisDto? Analysis);

    public record AiAuditAnalysisDto(Guid Id, string Model, string Summary, string RootCause, IReadOnlyCollection<AiRecommendation> Recommendations, string? LetterDraft, DateTimeOffset CreatedAt);

    public class GetLatestAiAuditHandler : IRequestHandler<GetLatestAiAuditCommand, Response<GetLatestAiAuditDto>>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly SmartAuditDbContext _dbContext;
        private readonly IAiAuditService _aiService;

        public GetLatestAiAuditHandler(SmartAuditDbContext dbContext, IAiAuditService aiService)
        {
            _dbContext = dbContext;
            _aiService = aiService;
        }

        public async Task<Response<GetLatestAiAuditDto>> Handle(GetLatestAiAuditCommand request, CancellationToken ct)
        {
            var facility = await _dbContext.Facilities
                .AsNoTracking()
                .Where(f => f.Id == request.FacilityId)
                .Select(f => new { f.OrganizationId })
                .FirstOrDefaultAsync(ct);

            if (facility == null)
                return Response.NotFound<GetLatestAiAuditDto>("Objekt s tímto id neexistuje");

            if (request.Viewer.Role == AppRoles.Manager && facility.OrganizationId != request.Viewer.OrganizationId)
                return Response.Forbidden<GetLatestAiAuditDto>("K tomuto nemáte oprávnění");

            var latest = await _dbContext.AiAnalyses
                .AsNoTracking()
                .Where(a => a.FacilityId == request.FacilityId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);

            var analysis = latest == null ? null : new AiAuditAnalysisDto(
                latest.Id,
                latest.Model,
                latest.Summary,
                latest.RootCause,
                JsonSerializer.Deserialize<List<AiRecommendation>>(latest.RecommendationsJson, JsonOptions) ?? [],
                latest.LetterDraft,
                latest.CreatedAt);

            return Response.Success(new GetLatestAiAuditDto(_aiService.IsConfigured, analysis));
        }
    }
}
