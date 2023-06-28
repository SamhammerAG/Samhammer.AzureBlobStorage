using System;

namespace Samhammer.AzureBlobStorage.Options
{
    public class AzureBlobStorageOptions
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }

        public TimeSpan? FileUrlExpires { get; set; }
    }
}
