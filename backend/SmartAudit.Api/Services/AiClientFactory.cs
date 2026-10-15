using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services
{
    public interface IAiClientFactory
    {
        bool IsChatConfigured { get; }
        bool IsEmbeddingConfigured { get; }
        (ChatClient Chat, string Model) CreateChatClient();
        EmbeddingClient CreateEmbeddingClient();
    }

    // Klienti modelu pro AI analýzu i průvodce — Azure OpenAI má přednost, jinak přímo OpenAI (stejné rozhraní)
    public class AiClientFactory : IAiClientFactory
    {
        private readonly AzureOpenAiSettings _azureSettings;
        private readonly OpenAiSettings _openAiSettings;

        public AiClientFactory(AzureOpenAiSettings azureSettings, OpenAiSettings openAiSettings)
        {
            _azureSettings = azureSettings;
            _openAiSettings = openAiSettings;
        }

        public bool IsChatConfigured => _azureSettings.IsConfigured || _openAiSettings.IsConfigured;

        // Azure potřebuje vlastní nasazení embeddingového modelu, OpenAI stačí název modelu
        public bool IsEmbeddingConfigured => _azureSettings.IsConfigured
            ? !string.IsNullOrWhiteSpace(_azureSettings.EmbeddingDeployment)
            : _openAiSettings.IsConfigured && !string.IsNullOrWhiteSpace(_openAiSettings.EmbeddingModel);

        public (ChatClient Chat, string Model) CreateChatClient()
        {
            if (_azureSettings.IsConfigured)
                return (AzureClient().GetChatClient(_azureSettings.Deployment), _azureSettings.Deployment);

            return (new ChatClient(_openAiSettings.Model, new ApiKeyCredential(_openAiSettings.ApiKey)), _openAiSettings.Model);
        }

        public EmbeddingClient CreateEmbeddingClient()
        {
            if (_azureSettings.IsConfigured)
                return AzureClient().GetEmbeddingClient(_azureSettings.EmbeddingDeployment);

            return new EmbeddingClient(_openAiSettings.EmbeddingModel, new ApiKeyCredential(_openAiSettings.ApiKey));
        }

        private AzureOpenAIClient AzureClient() => new(new Uri(_azureSettings.Endpoint), new ApiKeyCredential(_azureSettings.ApiKey));
    }
}
