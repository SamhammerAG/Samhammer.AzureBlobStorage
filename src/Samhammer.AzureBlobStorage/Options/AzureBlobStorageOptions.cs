namespace Samhammer.AzureBlobStorage.Options
{
    public class AzureBlobStorageOptions
    {
        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }

        public int FileUrlExpiresByDays { get; set; } = 1;
    }
}
