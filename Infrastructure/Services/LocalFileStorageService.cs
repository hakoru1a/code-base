using Contracts.Services;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class LocalFileStorageService : ILocalFileStorageService
    {
        private readonly string _storagePath;
        private readonly string _baseUrl;

        public LocalFileStorageService(IOptions<LocalStorageSettings> options)
        {
            var settings = options.Value;
            _storagePath = settings.Path ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            _baseUrl = settings.BaseUrl ?? "/uploads";

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string? contentType = null, CancellationToken cancellationToken = default)
        {
            var safeFileName = Path.GetFileName(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
            var filePath = Path.Combine(_storagePath, uniqueFileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
            }

            return Path.Combine(_baseUrl, uniqueFileName).Replace("\\", "/");
        }

        public async Task<Stream?> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_storagePath, fileName);

            if (!File.Exists(filePath))
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                await fileStream.CopyToAsync(memoryStream, cancellationToken);
            }
            memoryStream.Position = 0;
            return memoryStream;
        }

        public Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var fileName = Path.GetFileName(fileUrl);
                var filePath = Path.Combine(_storagePath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_storagePath, fileName);
            return Task.FromResult(File.Exists(filePath));
        }

        public Task<string> GetFileUrlAsync(string fileName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Path.Combine(_baseUrl, fileName).Replace("\\", "/"));
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
    }
}
