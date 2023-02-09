using System.IO;

namespace Samhammer.AzureBlobStorage.Contracts
{
    public class BlobContract : BlobInfoContract
    {
        public Stream Content { get; set; }
    }
}
