using System;
using Microsoft.IO;

namespace Samhammer.AzureBlobStorage
{
    public class StreamHelper
    {
        private const int BlockSize = 1024;
        private const int LargeBufferMultiple = 1024 * 1024;
        private const int MaxBufferSize = 16 * LargeBufferMultiple;
        private const long DefaultMaxSmallPoolFreeBytes = MaxBufferSize * 4;
        private const long DefaultMaxLargePoolFreeBytes = 100 * BlockSize;
        private static string maxSmallPoolFreeBytes = Environment.GetEnvironmentVariable("MAX_SMALLPOOL_FREEBYTES");
        private static string maxLargePoolFreeBytes = Environment.GetEnvironmentVariable("MAX_LARGEPOOL_FREEBYTES");
        public static readonly RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager(
            BlockSize,
            LargeBufferMultiple,
            MaxBufferSize,
            string.IsNullOrEmpty(maxSmallPoolFreeBytes) ? DefaultMaxSmallPoolFreeBytes : long.Parse(maxSmallPoolFreeBytes),
            string.IsNullOrEmpty(maxLargePoolFreeBytes) ? DefaultMaxLargePoolFreeBytes : long.Parse(maxLargePoolFreeBytes));
    }
}
