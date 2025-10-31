namespace Infrastructure.Configurations
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;

        /// <summary>
        /// Default chat model. Recommended: gpt-4o, gpt-4o-mini, gpt-4-turbo
        /// </summary>
        public string DefaultModel { get; set; } = "gpt-4o-mini";

        /// <summary>
        /// Default embedding model. Recommended: text-embedding-3-small, text-embedding-3-large
        /// </summary>
        public string DefaultEmbeddingModel { get; set; } = "text-embedding-3-small";

        /// <summary>
        /// Default image generation model. Options: dall-e-3, dall-e-2
        /// </summary>
        public string DefaultImageModel { get; set; } = "dall-e-3";

        /// <summary>
        /// Base URL for OpenAI API (can be customized for proxies or Azure OpenAI)
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";

        /// <summary>
        /// Maximum retry attempts for API calls
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 60;
    }
}


