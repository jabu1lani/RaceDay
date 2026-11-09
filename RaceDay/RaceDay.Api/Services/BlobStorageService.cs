using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration configuration)
        {
            string connString = configuration["AzureBlobStorage:ConnectionString"]!;
            string containerName = configuration["AzureBlobStorage:ContainerName"] ?? "raceday-images";
            _container = new BlobContainerClient(connString, containerName);
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            // Create container if it doesn't exist
            await _container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string blobName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            BlobClient blob = _container.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, true);

            return blob.Uri.ToString();
        }
    }
}