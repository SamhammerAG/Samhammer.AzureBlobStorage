using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Samhammer.AzureBlobStorage.Options;

namespace Samhammer.AzureBlobStorage.Services
{
    public class StreamManagerService : IStreamManagerService
    {
        private const long MaxSmallPoolFreeBytes = 1000000;

        private const long MaxLargePoolFreeBytes = 10000000;

        private readonly RecyclableMemoryStreamManager _streamManager;

        public StreamManagerService(IOptions<StreamManagerOptions> streamManagerOptions)
        {
            StreamManagerOptions options = streamManagerOptions.Value;
            _streamManager = new RecyclableMemoryStreamManager(
                options.MaxSmallPoolFreeBytes.HasValue ? options.MaxSmallPoolFreeBytes.Value : MaxSmallPoolFreeBytes,
                options.MaxLargePoolFreeBytes.HasValue ? options.MaxLargePoolFreeBytes.Value : MaxLargePoolFreeBytes);
        }

        public MemoryStream GetStream()
        {
            return _streamManager.GetStream();
        }
    }

    public interface IStreamManagerService
    {
        public MemoryStream GetStream();
    }
}
