using Contracts.Services;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI.Images;
using OpenAI.Audio;
using OpenAI.Moderations;
using System.ClientModel;

namespace Infrastructure.Services
{
    public class OpenAiProviderService : IOpenAiProviderService
    {
        private readonly OpenAIClient _client;
        private readonly OpenAISettings _settings;
        private readonly ChatClient _chatClient;
        private readonly EmbeddingClient _embeddingClient;
        private readonly ImageClient _imageClient;
        private readonly AudioClient _audioClient;
        private readonly ModerationClient _moderationClient;

        public string ProviderName => "OpenAI";

        public OpenAiProviderService(IOptions<OpenAISettings> options)
        {
            _settings = options.Value;

            // Create the OpenAI client with API key and optional organization
            var clientOptions = new OpenAIClientOptions();
            _client = new OpenAIClient(new ApiKeyCredential(_settings.ApiKey), clientOptions);

            // Initialize specialized clients
            _chatClient = _client.GetChatClient(_settings.DefaultModel);
            _embeddingClient = _client.GetEmbeddingClient(_settings.DefaultEmbeddingModel);
            _imageClient = _client.GetImageClient(_settings.DefaultImageModel);
            _audioClient = _client.GetAudioClient("whisper-1");
            _moderationClient = _client.GetModerationClient("text-moderation-latest");
        }

        public async Task<string> GenerateTextAsync(string prompt, string? model = null, float temperature = 0.7F, int maxTokens = 2048, CancellationToken cancellationToken = default)
        {
            var chatClient = string.IsNullOrEmpty(model)
                ? _chatClient
                : _client.GetChatClient(model);

            var chatOptions = new ChatCompletionOptions
            {
                Temperature = temperature,
                MaxOutputTokenCount = maxTokens
            };

            var messages = new List<ChatMessage>
            {
                new UserChatMessage(prompt)
            };

            var completion = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            return completion.Value.Content[0].Text;
        }

        public async Task<string> ChatAsync(IEnumerable<(string Role, string Content)> messages, string? model = null, float temperature = 0.7F, int maxTokens = 2048, CancellationToken cancellationToken = default)
        {
            var chatClient = string.IsNullOrEmpty(model)
                ? _chatClient
                : _client.GetChatClient(model);

            var chatOptions = new ChatCompletionOptions
            {
                Temperature = temperature,
                MaxOutputTokenCount = maxTokens
            };

            var chatMessages = messages.Select(m => m.Role.ToLower() switch
            {
                "system" => (ChatMessage)new SystemChatMessage(m.Content),
                "assistant" => new AssistantChatMessage(m.Content),
                "user" => new UserChatMessage(m.Content),
                _ => new UserChatMessage(m.Content)
            }).ToList();

            var completion = await chatClient.CompleteChatAsync(chatMessages, chatOptions, cancellationToken);
            return completion.Value.Content[0].Text;
        }

        public async Task<float[]> GetEmbeddingAsync(string text, string? model = null, CancellationToken cancellationToken = default)
        {
            var embeddingClient = string.IsNullOrEmpty(model)
                ? _embeddingClient
                : _client.GetEmbeddingClient(model);

            var embedding = await embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
            return embedding.Value.ToFloats().ToArray();
        }

        public async Task<byte[]> GenerateImageAsync(string prompt, int width = 512, int height = 512, string format = "png", CancellationToken cancellationToken = default)
        {
            // OpenAI supports specific sizes, map to closest supported size
            var size = MapToSupportedSize(width, height);

            var imageOptions = new ImageGenerationOptions
            {
                Size = size,
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            var imageGeneration = await _imageClient.GenerateImageAsync(prompt, imageOptions, cancellationToken);
            return imageGeneration.Value.ImageBytes.ToArray();
        }

        public async Task<string> TranscribeAudioAsync(Stream audioStream, string? language = null, string format = "text", CancellationToken cancellationToken = default)
        {
            // Read stream into BinaryData
            using var memoryStream = new MemoryStream();
            await audioStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            var options = new AudioTranscriptionOptions
            {
                Language = language,
                ResponseFormat = format switch
                {
                    "json" => AudioTranscriptionFormat.Simple,
                    "verbose_json" => AudioTranscriptionFormat.Verbose,
                    "srt" => AudioTranscriptionFormat.Srt,
                    "vtt" => AudioTranscriptionFormat.Vtt,
                    _ => AudioTranscriptionFormat.Text
                }
            };

            var transcription = await _audioClient.TranscribeAudioAsync(memoryStream, "audio.mp3", options, cancellationToken);
            return transcription.Value.Text;
        }

        public async Task<bool> IsSafeContentAsync(string input, CancellationToken cancellationToken = default)
        {
            var moderation = await _moderationClient.ClassifyTextAsync(input, cancellationToken);
            return !moderation.Value.Flagged;
        }

        public Task<IReadOnlyList<string>> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            // Return commonly available models
            var models = new List<string>
            {
                "gpt-4o",
                "gpt-4o-mini",
                "gpt-4-turbo",
                "gpt-4",
                "gpt-3.5-turbo",
                "text-embedding-3-small",
                "text-embedding-3-large",
                "text-embedding-ada-002",
                "dall-e-3",
                "dall-e-2",
                "whisper-1"
            };
            return Task.FromResult<IReadOnlyList<string>>(models);
        }

        public Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default)
        {
            // In a real implementation, you might want to call the models API
            // For now, return true for common models
            var commonModels = new[]
            {
                "gpt-4o", "gpt-4o-mini", "gpt-4-turbo", "gpt-4", "gpt-3.5-turbo",
                "text-embedding-3-small", "text-embedding-3-large", "text-embedding-ada-002",
                "dall-e-3", "dall-e-2", "whisper-1"
            };

            return Task.FromResult(commonModels.Contains(modelName));
        }

        private static GeneratedImageSize MapToSupportedSize(int width, int height)
        {
            // DALL-E 3 supports: 1024x1024, 1792x1024, 1024x1792
            // DALL-E 2 supports: 256x256, 512x512, 1024x1024

            if (width == height)
            {
                return GeneratedImageSize.W1024xH1024;
            }
            else if (width > height)
            {
                return GeneratedImageSize.W1792xH1024;
            }
            else
            {
                return GeneratedImageSize.W1024xH1792;
            }
        }
    }
}
