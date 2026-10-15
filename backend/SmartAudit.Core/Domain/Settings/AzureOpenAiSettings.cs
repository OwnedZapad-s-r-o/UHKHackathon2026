namespace SmartAudit.Core.Domain.Settings
{
    // Azure OpenAI pro AI analýzu objektu — hodnoty přes proměnné prostředí (AZURE_OPENAI_* v .env), v repu zůstávají prázdné
    public class AzureOpenAiSettings
    {
        public const string SectionName = "AzureOpenAiSettings";

        // https://<název-zdroje>.openai.azure.com/
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;

        // Název nasazení modelu v Azure AI Foundry (např. gpt-4o)
        public string Deployment { get; set; } = string.Empty;

        public bool IsConfigured =>
            Uri.TryCreate(Endpoint, UriKind.Absolute, out _)
            && !string.IsNullOrWhiteSpace(ApiKey)
            && !string.IsNullOrWhiteSpace(Deployment);
    }
}
