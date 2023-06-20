using System;
using Microsoft.IO;

namespace Samhammer.AzureBlobStorage
{
    public class StreamHelper
    {
        private const long DefaultMaxSmallPoolFreeBytes = 1000000;
        private const long DefaultMaxLargePoolFreeBytes = 10000000;
        private static string maxSmallPoolFreeBytes = Environment.GetEnvironmentVariable("MAX_SMALLPOOL_FREEBYTES");
        private static string maxLargePoolFreeBytes = Environment.GetEnvironmentVariable("MAX_LARGEPOOL_FREEBYTES");
        public static readonly RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager(
            string.IsNullOrEmpty(maxSmallPoolFreeBytes) ? DefaultMaxSmallPoolFreeBytes : long.Parse(maxSmallPoolFreeBytes),
            string.IsNullOrEmpty(maxLargePoolFreeBytes) ? DefaultMaxLargePoolFreeBytes : long.Parse(maxLargePoolFreeBytes));
    }
}
