using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Core.Data;

namespace SmartAudit.Api.Services.Reports
{
    // Poslední uložená AI analýza objektu pro PDF — když žádná není, report sekci vynechá
    public interface IReportAiSource
    {
        Task<AiAnalysisSection?> GetLatestAsync(Guid facilityId, CancellationToken ct);
    }
    public class ReportAiSource : IReportAiSource
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly SmartAuditDbContext _dbContext;

        public ReportAiSource(SmartAuditDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AiAnalysisSection?> GetLatestAsync(Guid facilityId, CancellationToken ct)
        {
            var latest = await _dbContext.AiAnalyses
                .AsNoTracking()
                .Where(a => a.FacilityId == facilityId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (latest == null)
                return null;

            var recommendations = JsonSerializer.Deserialize<List<AiRecommendation>>(latest.RecommendationsJson, JsonOptions) ?? [];

            return new AiAnalysisSection(
                latest.Summary,
                latest.RootCause,
                recommendations.Select(r => new AiRecommendationSection(r.Title, r.Detail, r.Cost, r.Saving, r.Payback, r.HeritageSafe)).ToList(),
                latest.LetterDraft,
                latest.CreatedAt,
                latest.Model);
        }
    }
}
