namespace SmartAudit.Core.Data.Tables
{
    // Výsledek AI analýzy objektu — ukládá se, aby byl vidět v panelu i v PDF bez dalšího volání modelu
    public class AiAnalysis
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        // Obsah analýzy
        public string Model { get; private set; } = string.Empty;
        public string Summary { get; private set; } = string.Empty;
        public string RootCause { get; private set; } = string.Empty;
        public string RecommendationsJson { get; private set; } = "[]";
        public string? LetterDraft { get; private set; }

        // Otisk vstupu (stav, zjištění a ztráta z auditu) — stejný otisk = stejná odpověď modelu, nevolat ho znovu
        public string? InputFingerprint { get; private set; }


        // Cizí klíče
        public Guid FacilityId { get; private set; }
        public Guid? CreatedByUserId { get; private set; }


        // Časové údaje
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;


        // Navigační vlastnosti
        public Facility Facility { get; set; } = null!;

        private AiAnalysis() { }

        public AiAnalysis(Guid facilityId, string model, string summary, string rootCause, string recommendationsJson, string? letterDraft, Guid? createdByUserId, string? inputFingerprint = null)
        {
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model AI analýzy nesmí být prázdný.", nameof(model));

            if (string.IsNullOrWhiteSpace(summary))
                throw new ArgumentException("Shrnutí AI analýzy nesmí být prázdné.", nameof(summary));

            FacilityId = facilityId;
            Model = model;
            Summary = summary;
            RootCause = rootCause;
            RecommendationsJson = string.IsNullOrWhiteSpace(recommendationsJson) ? "[]" : recommendationsJson;
            LetterDraft = letterDraft;
            CreatedByUserId = createdByUserId;
            InputFingerprint = inputFingerprint;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
