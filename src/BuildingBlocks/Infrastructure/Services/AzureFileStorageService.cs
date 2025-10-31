using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Contracts.Services;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class AzureFileStorageService : IAzureFileStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureFileStorageService(IOptions<AzureStorageSettings> options)
        {
            var settings = options.Value;
            var blobServiceClient = new BlobServiceClient(settings.ConnectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName);
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string? contentType = null, CancellationToken cancellationToken = default)
        {
            var blobName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType ?? "application/octet-stream"
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            }, cancellationToken);

            return blobClient.Uri.ToString();
        }

        public async Task<Stream?> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobName = GetBlobNameFromUrl(fileUrl);
                var blobClient = _containerClient.GetBlobClient(blobName);

                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Azure.RequestFailedException)
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobName = GetBlobNameFromUrl(fileUrl);
                var blobClient = _containerClient.GetBlobClient(blobName);
                var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                return response.Value;
            }
            catch (Azure.RequestFailedException)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var blobName = GetBlobNameFromUrl(fileUrl);
                var blobClient = _containerClient.GetBlobClient(blobName);
                var response = await blobClient.ExistsAsync(cancellationToken);
                return response.Value;
            }
            catch (Azure.RequestFailedException)
            {
                return false;
            }
        }

        public Task<string> GetFileUrlAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            return Task.FromResult(blobClient.Uri.ToString());
        }

        public async Task<IReadOnlyList<Stream>> UploadManyAsync(IEnumerable<Stream> files, CancellationToken cancellationToken = default)
        {
            var results = new List<Stream>();
            var fileList = files.ToList();

            for (int i = 0; i < fileList.Count; i++)
            {
                var fileName = $"file_{i}_{Guid.NewGuid()}";
                await UploadAsync(fileList[i], fileName, null, cancellationToken);
                results.Add(fileList[i]);
            }

            return results;
        }

        public async Task<IDictionary<string, Stream?>> DownloadManyAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, Stream?>();

            foreach (var fileUrl in fileUrls)
            {
                var stream = await DownloadAsync(fileUrl, cancellationToken);
                results[fileUrl] = stream;
            }

            return results;
        }

        public async Task<IReadOnlyList<string>> DeleteManyAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default)
        {
            var deletedFiles = new List<string>();

            foreach (var fileUrl in fileUrls)
            {
                var deleted = await DeleteAsync(fileUrl, cancellationToken);
                if (deleted)
                {
                    deletedFiles.Add(fileUrl);
                }
            }

            return deletedFiles;
        }

        public async Task<IDictionary<string, bool>> ExistsManyAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, bool>();

            foreach (var fileUrl in fileUrls)
            {
                var exists = await ExistsAsync(fileUrl, cancellationToken);
                results[fileUrl] = exists;
            }

            return results;
        }

        private string GetBlobNameFromUrl(string fileUrl)
        {
            var uri = new Uri(fileUrl);
            return uri.Segments[^1];
        }
    }
}
