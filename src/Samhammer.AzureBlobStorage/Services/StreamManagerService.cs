using System.IO;
using System.Threading;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Samhammer.AzureBlobStorage.Options;

namespace Samhammer.AzureBlobStorage.Services
{
    public class StreamManagerService : IStreamManagerService
    {
        private const long MaxSmallPoolFreeBytes = 1000000;

        private const long MaxLargePoolFreeBytes = 10000000;

        private IOptions<StreamManagerOptions> StreamManagerOptions { get; }

        private static RecyclableMemoryStreamManager streamManager;

        private static bool initialized;

        private static object initializeLock = new object();

        public StreamManagerService(IOptions<StreamManagerOptions> streamManagerOptions)
        {
            StreamManagerOptions = streamManagerOptions;
        }

        public RecyclableMemoryStreamManager GetStreamManager()
        {
            return LazyInitializer.EnsureInitialized(ref streamManager, ref initialized, ref initializeLock, InitStreamManager);
        }

        public RecyclableMemoryStreamManager InitStreamManager()
        {
            var streamManager = new RecyclableMemoryStreamManager(
                StreamManagerOptions.Value.MaxSmallPoolFreeBytes ?? MaxSmallPoolFreeBytes,
                StreamManagerOptions.Value.MaxLargePoolFreeBytes ?? MaxLargePoolFreeBytes);

            return streamManager;
        }

        public MemoryStream GetStream()
        {
            var streamManager = GetStreamManager();
            return streamManager.GetStream();
        }
    }

    public interface IStreamManagerService
    {
        public MemoryStream GetStream();
    }
}
