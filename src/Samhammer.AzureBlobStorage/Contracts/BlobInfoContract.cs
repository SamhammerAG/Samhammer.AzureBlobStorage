using System;

namespace Samhammer.AzureBlobStorage.Contracts
{
    public class BlobInfoContract
    {
        public string Name { get; set; }

        public string BlobType { get; set; }

        public string ContentEncoding { get; set; }

        public string ContentType { get; set; }

        public long? Size { get; set; }

        public DateTimeOffset? DateCreated { get; set; }

        public string AccessTier { get; set; }
    }
}
