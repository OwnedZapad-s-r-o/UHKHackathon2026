namespace SmartAudit.Core.Domain.Settings
{
    // OpenAI (api.openai.com) pro AI analýzu objektu — použije se, když není nastavené Azure OpenAI.
    // Hodnoty přes proměnné prostředí (OPENAI_* v .env), v repu zůstávají prázdné
    public class OpenAiSettings
    {
        public const string SectionName = "OpenAiSettings";

        // Klíč z platform.openai.com → API keys (sk-…)
        public string ApiKey { get; set; } = string.Empty;

        // Model s podporou structured outputs a parametru temperature (např. gpt-4o, gpt-4o-mini, gpt-4.1)
        public string Model { get; set; } = "gpt-4o";

        // Embeddingy pro vyhledávání ve znalostech průvodce (RAG); prázdné = průvodce hledá jen podle slov
        public string EmbeddingModel { get; set; } = "text-embedding-3-small";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ApiKey)
            && !string.IsNullOrWhiteSpace(Model);
    }
}
