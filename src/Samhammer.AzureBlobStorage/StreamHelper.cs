using System;
using Microsoft.IO;

namespace Samhammer.AzureBlobStorage
{
    public class StreamHelper
    {
        private static string maxSmallPoolFreeBytes = Environment.GetEnvironmentVariable("MAX_SMALLPOOL_FREEBYTES");
        private static string maxLargePoolFreeBytes = Environment.GetEnvironmentVariable("MAX_LARGEPOOL_FREEBYTES");
        public static readonly RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager(
            string.IsNullOrEmpty(maxSmallPoolFreeBytes) ? long.Parse(maxSmallPoolFreeBytes) : 1000000,
            string.IsNullOrEmpty(maxLargePoolFreeBytes) ? long.Parse(maxLargePoolFreeBytes) : 10000000);
    }
}
