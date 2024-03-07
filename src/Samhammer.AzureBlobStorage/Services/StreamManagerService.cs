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

        private static RecyclableMemoryStreamManager _streamManager;

        private static bool _initialized;

        private static object _initializeLock = new object();

        public StreamManagerService(IOptions<StreamManagerOptions> streamManagerOptions)
        {
            StreamManagerOptions = streamManagerOptions;
        }

        public RecyclableMemoryStreamManager GetStreamManager()
        {
            return LazyInitializer.EnsureInitialized(ref _streamManager, ref _initialized, ref _initializeLock, InitStreamManager);
        }

        public RecyclableMemoryStreamManager InitStreamManager()
        {
            var options = new RecyclableMemoryStreamManager.Options
            {
                MaximumLargePoolFreeBytes = StreamManagerOptions.Value.MaxLargePoolFreeBytes ?? MaxLargePoolFreeBytes,
                MaximumSmallPoolFreeBytes = StreamManagerOptions.Value.MaxSmallPoolFreeBytes ?? MaxSmallPoolFreeBytes,
            };
            return new RecyclableMemoryStreamManager(options);
        }

        public MemoryStream GetStream()
        {
            var sm = GetStreamManager();
            return sm.GetStream();
        }
    }

    public interface IStreamManagerService
    {
        public MemoryStream GetStream();
    }
}
