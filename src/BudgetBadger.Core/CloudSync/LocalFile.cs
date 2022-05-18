using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BudgetBadger.Core.CloudSync
{
    public class LocalFile : IFile
    {
        public void SetAuthentication(IReadOnlyDictionary<string, string> keys)
        {
        }
        
        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            return File.ReadAllBytes(path);
        }

        public async Task WriteAllBytesAsync(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }

        public async Task DeleteAsync(string path)
        {
            File.Delete(path);
        }

        public async Task CopyAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        public async Task MoveAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            if (overwrite)
            {
                File.Delete(destFileName);
            }

            File.Move(sourceFileName, destFileName);
        }

        public async Task<bool> ExistsAsync(string path)
        {
            return File.Exists(path);
        }
    }
}
