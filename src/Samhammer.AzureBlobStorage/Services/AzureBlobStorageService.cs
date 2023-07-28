using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using Samhammer.AzureBlobStorage.Client;
using Samhammer.AzureBlobStorage.Contracts;
using Samhammer.AzureBlobStorage.Mappers;
using Samhammer.AzureBlobStorage.Options;

namespace Samhammer.AzureBlobStorage.Services
{
    public class AzureBlobStorageService<T> : IAzureBlobStorageService<T> where T : IAzureBlobStorageClientFactory
    {
        private readonly string _defaultContainerName;

        private readonly BlobServiceClient _client;

        private readonly IOptions<AzureBlobStorageOptions> _blobStorageOptions;

        private readonly IStreamManagerService _streamManagerService;

        public AzureBlobStorageService(T blobStorageClientFactory, IStreamManagerService streamManagerService, IOptions<AzureBlobStorageOptions> blobStorageOptions)
        {
            _defaultContainerName = blobStorageClientFactory.GetDefaultContainerName();
            _client = blobStorageClientFactory.GetClient();
            _blobStorageOptions = blobStorageOptions;
            _streamManagerService = streamManagerService;
        }

        public string GetStorageAccountName()
        {
            return _client.AccountName;
        }

        public async IAsyncEnumerable<StorageContainerContract> GetContainersAsync()
        {
            var containers = _client.GetBlobContainersAsync(BlobContainerTraits.Metadata);

            await foreach (var item in containers)
            {
                yield return new StorageContainerContract() { Name = item.Name };
            }
        }

        public async Task CreateContainerIfNotExistsAsync(string containerName = null)
        {
            var containerClient = await GetContainerClient(containerName, true);

            await containerClient.CreateIfNotExistsAsync();
        }

        public async Task DeleteContainerAsync(string containerName = null)
        {
            var containerClient = await GetContainerClient(containerName);

            await containerClient.DeleteAsync();
        }

        public async IAsyncEnumerable<BlobInfoContract> ListBlobsInContainerAsync(string containerName = null, string folderName = null)
        {
            var containerClient = await GetContainerClient(containerName);
            var blobs = containerClient.GetBlobsAsync(prefix: folderName);

            await foreach (var blob in blobs)
            {
                var model = ContractMapper.ToBlobInfoContract(blob);
                yield return model;
            }
        }

        public async Task<BlobContract> GetBlobContentsAsync(string blobName, string containerName = null)
        {
            var containerClient = await GetContainerClient(containerName);
            var blobClient = await GetBlobClient(containerClient, blobName);

            var properties = (await blobClient.GetPropertiesAsync()).Value;
            var stream = _streamManagerService.GetStream();
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            return ContractMapper.ToBlobContract(blobClient.Name, properties, stream);
        }

        public async Task<string> GetBlobUrlAsync(string blobName, string containerName = null)
        {
            var containerClient = await GetContainerClient(containerName);
            var blobClient = await GetBlobClient(containerClient, blobName);

            var uri = CreateServiceSASBlob(blobClient);

            return uri.AbsoluteUri;
        }

        public async Task UploadBlobAsync(string blobName, string contentType, Stream content, string containerName = null, string folderName = null)
        {
            var containerClient = await GetContainerClient(containerName);
            var blobClient = await GetBlobClient(containerClient, GetBlobPath(folderName, blobName), true);

            var options = new BlobUploadOptions() { HttpHeaders = new BlobHttpHeaders() { ContentType = contentType } };
            await blobClient.UploadAsync(content, options);
        }

        private Uri CreateServiceSASBlob(BlobClient blobClient)
        {
            if (!blobClient.CanGenerateSasUri)
            {
                return null;
            }

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b",
            };

            sasBuilder.ExpiresOn = _blobStorageOptions.Value.FileUrlExpires.HasValue
                ? DateTimeOffset.UtcNow.Add(_blobStorageOptions.Value.FileUrlExpires.Value)
                : DateTimeOffset.UtcNow.AddDays(1);

            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            Uri sasURI = blobClient.GenerateSasUri(sasBuilder);

            return sasURI;
        }

        private string GetBlobPath(string folderName, string blobName)
        {
            var folder = string.IsNullOrWhiteSpace(folderName) ? string.Empty : $"{folderName}/";
            return $"{folder}{blobName}";
        }

        public async Task DeleteBlobAsync(string blobName, string containerName = null)
        {
            var containerClient = await GetContainerClient(containerName);
            var blobClient = await GetBlobClient(containerClient, blobName);

            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public async Task DeleteFolderAsync(string folderName, string containerName = null)
        {
            var containerClient = await GetContainerClient(containerName);
            folderName = folderName.TrimEnd('/');
            var blobs = containerClient.GetBlobsAsync(prefix: $"{folderName}/");

            await foreach (var blob in blobs)
            {
                await DeleteBlobAsync(blob.Name, containerName);
            }
        }

        public async Task<BlobContainerClient> GetContainerClient(string containerName = null, bool ignoreNonExistentContainer = false)
        {
            containerName ??= _defaultContainerName;

            var containerClient = _client.GetBlobContainerClient(containerName);

            if (!ignoreNonExistentContainer && !await containerClient.ExistsAsync())
            {
                throw new ApplicationException($"The container '{containerName}' does not exist");
            }

            return containerClient;
        }

        private async Task<BlobClient> GetBlobClient(BlobContainerClient containerClient, string blobName, bool ignoreNonExistentBlob = false)
        {
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!ignoreNonExistentBlob && !await blobClient.ExistsAsync())
            {
                throw new ApplicationException($"Unable to get blobClient for '{blobName}' in container '{containerClient.Name}' as no blob with this name exists in this container");
            }

            return blobClient;
        }
    }

    public interface IAzureBlobStorageService
    {
        public string GetStorageAccountName();

        public IAsyncEnumerable<StorageContainerContract> GetContainersAsync();

        public Task CreateContainerIfNotExistsAsync(string containerName = null);

        public Task DeleteContainerAsync(string containerName = null);

        public IAsyncEnumerable<BlobInfoContract> ListBlobsInContainerAsync(string containerName = null, string folderName = null);

        public Task<BlobContract> GetBlobContentsAsync(string blobName, string containerName = null);

        public Task<string> GetBlobUrlAsync(string blobName, string containerName = null);

        public Task UploadBlobAsync(string blobName, string contentType, Stream content, string containerName = null, string folderName = null);

        public Task DeleteBlobAsync(string blobName, string containerName = null);

        public Task DeleteFolderAsync(string folderName, string containerName = null);
    }

    public interface IAzureBlobStorageService<T> : IAzureBlobStorageService where T : IAzureBlobStorageClientFactory
    {
    }
}
