using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Samhammer.AzureBlobStorage.Options;

namespace Samhammer.AzureBlobStorage.Client
{
    public class DefaultAzureBlobStorageClientFactory : IDefaultAzureBlobStorageClientFactory
    {
        private IOptions<AzureBlobStorageOptions> Options { get; }

        public DefaultAzureBlobStorageClientFactory(IOptions<AzureBlobStorageOptions> options)
        {
            Options = options;
        }

        public BlobServiceClient GetClient(BlobClientOptions options = null)
        {
            return new BlobServiceClient(Options.Value.ConnectionString, options);
        }

        public string GetDefaultContainerName()
        {
            return Options.Value.ContainerName;
        }
    }

    public interface IDefaultAzureBlobStorageClientFactory : IAzureBlobStorageClientFactory
    {
    }
}
