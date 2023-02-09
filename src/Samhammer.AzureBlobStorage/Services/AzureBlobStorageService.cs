﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Samhammer.AzureBlobStorage.Client;
using Samhammer.AzureBlobStorage.Contracts;
using Samhammer.AzureBlobStorage.Mappers;

namespace Samhammer.AzureBlobStorage.Services
{
    public class AzureBlobStorageService<T> : IAzureBlobStorageService<T> where T : IAzureBlobStorageClientFactory
    {
        private readonly BlobServiceClient _client;

        public AzureBlobStorageService(T blobStorageClientFactory)
        {
            _client = blobStorageClientFactory.GetClient();
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

        public async Task CreateContainerIfNotExistsAsync(string containerName)
        {
            var containerClient = await GetContainerClient(containerName, true);

            await containerClient.CreateIfNotExistsAsync();
        }

        public async Task DeleteContainerAsync(string containerName)
        {
            var containerClient = await GetContainerClient(containerName);

            await containerClient.DeleteAsync();
        }

        public async IAsyncEnumerable<BlobInfoContract> ListBlobsInContainerAsync(string containerName)
        {
            var containerClient = await GetContainerClient(containerName);
            var blobs = containerClient.GetBlobsAsync();

            await foreach (var blob in blobs)
            {
                var model = ContractMapper.ToBlobInfoContract(blob);
                yield return model;
            }
        }

        public async Task<BlobContract> GetBlobContentsAsync(string containerName, string blobName)
        {
            var containerClient = await GetContainerClient(containerName);
            var blobClient = await GetBlobClient(containerClient, blobName);

            var properties = (await blobClient.GetPropertiesAsync()).Value;
            var stream = await blobClient.OpenReadAsync();

            return ContractMapper.ToBlobContract(blobClient.Name, properties, stream);
        }

        public async Task UploadBlobAsync(string containerName, string blobName, string contentType, Stream content)
        {
            var containerClient = await GetContainerClient(containerName);
            var blobClient = await GetBlobClient(containerClient, blobName, true);

            var options = new BlobUploadOptions() { HttpHeaders = new BlobHttpHeaders() { ContentType = contentType } };
            await blobClient.UploadAsync(content, options);
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var containerClient = await GetContainerClient(containerName);
            var blobClient = await GetBlobClient(containerClient, blobName);

            await blobClient.DeleteAsync();
        }

        public async Task<BlobContainerClient> GetContainerClient(string containerName, bool ignoreNonExistentContainer = false)
        {
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

    public interface IAzureBlobStorageService<T> where T : IAzureBlobStorageClientFactory
    {
        public string GetStorageAccountName();

        public IAsyncEnumerable<StorageContainerContract> GetContainersAsync();

        public Task CreateContainerIfNotExistsAsync(string containerName);

        public Task DeleteContainerAsync(string containerName);

        public IAsyncEnumerable<BlobInfoContract> ListBlobsInContainerAsync(string containerName);

        public Task<BlobContract> GetBlobContentsAsync(string containerName, string blobName);

        public Task UploadBlobAsync(string containerName, string blobName, string contentType, Stream content);

        public Task DeleteBlobAsync(string containerName, string blobName);
    }
}
