using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BudgetBadger.Core.CloudSync
{
    public class LocalDirectory : IDirectory
    {
        public void SetAuthentication(IReadOnlyDictionary<string, string> keys)
        {
        }
        
        public async Task CreateDirectoryAsync(string path)
        {
            Directory.CreateDirectory(path);
        }

        public async Task<bool> ExistsAsync(string path)
        {
            return Directory.Exists(path);
        }

        public async Task<IReadOnlyList<string>> GetFilesAsync(string path)
        {
            return Directory.GetFiles(path);
        }

        public async Task<IReadOnlyList<string>> GetDirectoriesAsync(string path)
        {
            return Directory.GetDirectories(path);
        }

        public async Task DeleteAsync(string path, bool recursive = false)
        {
            Directory.Delete(path, recursive);
        }

        public async Task MoveAsync(string sourceDirName, string destDirName)
        {
            Directory.Move(sourceDirName, destDirName);
        }
    }
}
