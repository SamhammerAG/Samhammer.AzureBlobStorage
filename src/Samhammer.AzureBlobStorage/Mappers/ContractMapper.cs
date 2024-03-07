using System.IO;
using Azure.Storage.Blobs.Models;
using Samhammer.AzureBlobStorage.Contracts;

namespace Samhammer.AzureBlobStorage.Mappers
{
    public static class ContractMapper
    {
        public static BlobInfoContract ToBlobInfoContract(BlobItem blob)
        {
            var properties = blob.Properties;

            return new BlobInfoContract()
            {
                Name = blob.Name,
                ContentEncoding = properties.ContentEncoding ?? string.Empty,
                ContentType = properties.ContentType,
                Size = properties.ContentLength,
                DateCreated = properties.CreatedOn,
                AccessTier = properties.AccessTier?.ToString(),
                BlobType = properties.BlobType?.ToString(),
            };
        }

        public static BlobContract ToBlobContract(string name, BlobProperties properties, Stream stream)
        {
            return new BlobContract()
            {
                Name = name,
                ContentEncoding = properties.ContentEncoding ?? string.Empty,
                ContentType = properties.ContentType,
                Size = properties.ContentLength,
                DateCreated = properties.CreatedOn,
                AccessTier = properties.AccessTier,
                BlobType = properties.BlobType.ToString(),
                Content = stream,
            };
        }
    }
}
