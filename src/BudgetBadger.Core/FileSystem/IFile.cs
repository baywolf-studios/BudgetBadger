using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetBadger.Core.FileSystem
{
    public interface IFile
    {
        void SetAuthentication(IReadOnlyDictionary<string, string> keys);
        Task<byte[]> ReadAllBytesAsync(string path);
        Task WriteAllBytesAsync(string path, byte[] data);
        Task DeleteAsync(string path);
        Task CopyAsync(string sourceFileName, string destFileName, bool overwrite = false);
        Task MoveAsync(string sourceFileName, string destFileName, bool overwrite = false);
        Task<bool> ExistsAsync(string path);
    }
}
