using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetBadger.Core.FileSystem
{
    public interface IDirectory
    {
        void SetAuthentication(IReadOnlyDictionary<string, string> keys);
        Task CreateDirectoryAsync(string path);
        Task<bool> ExistsAsync(string path);
        Task<IReadOnlyList<string>> GetFilesAsync(string path);
        Task<IReadOnlyList<string>> GetDirectoriesAsync(string path);
        Task DeleteAsync(string path, bool recursive = false);
        Task MoveAsync(string sourceDirName, string destDirName);
    }
}
