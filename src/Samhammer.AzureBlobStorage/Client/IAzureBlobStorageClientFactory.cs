using Azure.Storage.Blobs;

namespace Samhammer.AzureBlobStorage.Client
{
    public interface IAzureBlobStorageClientFactory
    {
        BlobServiceClient GetClient();
    }
}
