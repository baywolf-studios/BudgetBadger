using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace BudgetBadger.Core.Utilities
{
	public static class CompressionExtensions
	{
        public static byte[] Compress(this byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (var compressed = new MemoryStream())
            {
                using (var zip = new GZipStream(compressed, compressionLevel))
                {
                    zip.Write(data, 0, data.Length);
                }

                return compressed.ToArray();
            }
        }
        
        public static Stream Compress(this Stream data)
        {
            var compressed = new MemoryStream();
            using (var zip = new GZipStream(compressed, CompressionLevel.Fastest, true))
            {
                data.CopyTo(zip);
            }

            compressed.Seek(0, SeekOrigin.Begin);
            return compressed;
        }
        
        public static byte[] Decompress(this byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            using (var compressed = new MemoryStream(data))
            using (var decompressed = new MemoryStream())
            using (var zip = new GZipStream(compressed, CompressionMode.Decompress))
            {
                zip.CopyTo(decompressed);
                return decompressed.ToArray();
            }
        }

        public static Stream Decompress(this Stream data)
        {
            var decompressed = new MemoryStream();
            using (var zip = new GZipStream(data, CompressionMode.Decompress, true))
            {
                zip.CopyTo(decompressed);
            }

            decompressed.Seek(0, SeekOrigin.Begin);
            return decompressed;
        }
    }
}

