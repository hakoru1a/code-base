using Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OpenAiProvider : IOpenAiProviderService
    {
        public string ProviderName => throw new NotImplementedException();

        public Task<string> ChatAsync(IEnumerable<(string Role, string Content)> messages, string? model = null, float temperature = 0.7F, int maxTokens = 2048, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateImageAsync(string prompt, int width = 512, int height = 512, string format = "png", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateTextAsync(string prompt, string? model = null, float temperature = 0.7F, int maxTokens = 2048, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<float[]> GetEmbeddingAsync(string text, string? model = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsSafeContentAsync(string input, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<string>> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> TranscribeAudioAsync(Stream audioStream, string? language = null, string format = "text", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
