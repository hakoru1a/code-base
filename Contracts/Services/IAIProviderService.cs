namespace Contracts.Services;

public interface IAIProviderService
{
    string ProviderName { get; }

    Task<string> GenerateTextAsync(string prompt, string? model = null, float temperature = 0.7f, int maxTokens = 2048, CancellationToken cancellationToken = default);

    Task<string> ChatAsync(IEnumerable<(string Role, string Content)> messages, string? model = null, float temperature = 0.7f, int maxTokens = 2048, CancellationToken cancellationToken = default);

    Task<float[]> GetEmbeddingAsync(string text, string? model = null, CancellationToken cancellationToken = default);

    Task<byte[]> GenerateImageAsync(string prompt, int width = 512, int height = 512, string format = "png", CancellationToken cancellationToken = default);

    Task<string> TranscribeAudioAsync(Stream audioStream, string? language = null, string format = "text", CancellationToken cancellationToken = default);

    Task<bool> IsSafeContentAsync(string input, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListModelsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default);
}
