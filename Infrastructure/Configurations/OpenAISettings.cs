namespace Infrastructure.Configurations
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gpt-4";
        public string DefaultEmbeddingModel { get; set; } = "text-embedding-ada-002";
        public string DefaultImageModel { get; set; } = "dall-e-3";
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    }
}


