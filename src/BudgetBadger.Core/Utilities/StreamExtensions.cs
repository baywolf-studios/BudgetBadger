using System.IO;

namespace BudgetBadger.Core.Utilities
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream inputStream)
        {
            if (inputStream is MemoryStream stream)
                return stream.ToArray();

            using (var memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
