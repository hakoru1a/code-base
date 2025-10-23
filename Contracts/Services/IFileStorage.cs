namespace Contracts.Services;

public interface IFileStorage
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string? contentType = null, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<string> GetFileUrlAsync(string fileName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Stream>> UploadManyAsync(IEnumerable<Stream> files, CancellationToken cancellationToken = default);
    Task<IDictionary<string, Stream?>> DownloadManyAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> DeleteManyAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default);
    Task<IDictionary<string, bool>> ExistsManyAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default);
}


