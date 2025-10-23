using Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AzureFileStorageService : IAzureFileStorageService
    {
        public Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<string>> DeleteManyAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Stream?> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, Stream?>> DownloadManyAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, bool>> ExistsManyAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFileUrlAsync(string fileName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadAsync(Stream fileStream, string fileName, string? contentType = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Stream>> UploadManyAsync(IEnumerable<Stream> files, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
