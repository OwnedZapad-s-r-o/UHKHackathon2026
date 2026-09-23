namespace SmartAudit.Api.Services.Reports
{
    // Poslední AI analýza objektu pro PDF — dokud AI není zapojená, report sekci vynechá
    public interface IReportAiSource
    {
        Task<AiAnalysisSection?> GetLatestAsync(Guid facilityId, CancellationToken ct);
    }
    public class NoReportAiSource : IReportAiSource
    {
        public Task<AiAnalysisSection?> GetLatestAsync(Guid facilityId, CancellationToken ct)
        {
            return Task.FromResult<AiAnalysisSection?>(null);
        }
    }
}
