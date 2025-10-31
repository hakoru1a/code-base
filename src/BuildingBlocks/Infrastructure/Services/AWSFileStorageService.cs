using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Contracts.Services;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class AWSFileStorageService : IAWSFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public AWSFileStorageService(IOptions<AWSStorageSettings> options)
        {
            var settings = options.Value;
            _bucketName = settings.BucketName;

            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(settings.Region)
            };

            if (!string.IsNullOrEmpty(settings.ServiceUrl))
            {
                config.ServiceURL = settings.ServiceUrl;
                config.ForcePathStyle = true;
            }

            _s3Client = new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string? contentType = null, CancellationToken cancellationToken = default)
        {
            var key = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = key,
                BucketName = _bucketName,
                ContentType = contentType ?? "application/octet-stream",
                CannedACL = S3CannedACL.PublicRead
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest, cancellationToken);

            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }

        public async Task<Stream?> DownloadAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = GetKeyFromUrl(fileUrl);
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await _s3Client.GetObjectAsync(request, cancellationToken);
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (AmazonS3Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = GetKeyFromUrl(fileUrl);
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = GetKeyFromUrl(fileUrl);
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception)
            {
                return false;
            }
        }

        public Task<string> GetFileUrlAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var url = $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
            return Task.FromResult(url);
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

        private string GetKeyFromUrl(string fileUrl)
        {
            var uri = new Uri(fileUrl);
            return uri.AbsolutePath.TrimStart('/');
        }
    }
}
