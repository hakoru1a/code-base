using Contracts.Services;
using Infrastructure.Configurations;
using Infrastructure.Extentions;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class OpenAiProvider : IOpenAiProviderService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _settings;

        public string ProviderName => "OpenAI";

        public OpenAiProvider(IOptions<OpenAISettings> options, IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            if (!string.IsNullOrEmpty(_settings.Organization))
            {
                _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", _settings.Organization);
            }
        }

        public async Task<string> GenerateTextAsync(string prompt, string? model = null, float temperature = 0.7F, int maxTokens = 2048, CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                model = model ?? _settings.DefaultModel,
                messages = new[] {
                    new { role = "user", content = prompt }
                },
                temperature,
                max_tokens = maxTokens
            };

            var response = await SendRequestAsync<dynamic>("/chat/completions", requestBody, cancellationToken);
            return response.choices[0].message.content.ToString();
        }

        public async Task<string> ChatAsync(IEnumerable<(string Role, string Content)> messages, string? model = null, float temperature = 0.7F, int maxTokens = 2048, CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                model = model ?? _settings.DefaultModel,
                messages = messages.Select(m => new { role = m.Role.ToLower(), content = m.Content }).ToList(),
                temperature,
                max_tokens = maxTokens
            };

            var response = await SendRequestAsync<dynamic>("/chat/completions", requestBody, cancellationToken);
            return response.choices[0].message.content.ToString();
        }

        public async Task<float[]> GetEmbeddingAsync(string text, string? model = null, CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                model = model ?? _settings.DefaultEmbeddingModel,
                input = text
            };

            var response = await SendRequestAsync<dynamic>("/embeddings", requestBody, cancellationToken);
            var embedding = response.data[0].embedding;
            return JsonSerializer.Deserialize<float[]>(embedding.ToString()) ?? Array.Empty<float>();
        }

        public async Task<byte[]> GenerateImageAsync(string prompt, int width = 512, int height = 512, string format = "png", CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                model = _settings.DefaultImageModel,
                prompt,
                n = 1,
                size = $"{width}x{height}",
                response_format = "url"
            };

            var response = await SendRequestAsync<dynamic>("/images/generations", requestBody, cancellationToken);
            string imageUrl = response.data[0].url.ToString();

            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(imageUrl, cancellationToken);
        }

        public async Task<string> TranscribeAudioAsync(Stream audioStream, string? language = null, string format = "text", CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(audioStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
            content.Add(streamContent, "file", "audio.mp3");
            content.Add(new StringContent("whisper-1"), "model");
            if (!string.IsNullOrEmpty(language))
            {
                content.Add(new StringContent(language), "language");
            }

            var response = await _httpClient.PostAsync("/audio/transcriptions", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDoc = JsonDocument.Parse(result);
            return jsonDoc.RootElement.GetProperty("text").GetString() ?? string.Empty;
        }

        public async Task<bool> IsSafeContentAsync(string input, CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                input
            };

            var response = await SendRequestAsync<dynamic>("/moderations", requestBody, cancellationToken);
            return !((bool)response.results[0].flagged);
        }

        public Task<IReadOnlyList<string>> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            var models = new List<string>
            {
                "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo", "text-embedding-ada-002", "dall-e-3"
            };
            return Task.FromResult<IReadOnlyList<string>>(models);
        }

        public Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        private async Task<T> SendRequestAsync<T>(string endpoint, object requestBody, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJson(endpoint, requestBody);
            return await response.ReadContentAs<T>();
        }
    }
}
