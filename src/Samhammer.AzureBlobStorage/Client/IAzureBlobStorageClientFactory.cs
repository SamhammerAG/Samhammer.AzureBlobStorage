using Azure.Storage.Blobs;

namespace Samhammer.AzureBlobStorage.Client
{
    public interface IAzureBlobStorageClientFactory
    {
        string GetDefaultContainerName();

        BlobServiceClient GetClient(BlobClientOptions options = null);
    }
}
