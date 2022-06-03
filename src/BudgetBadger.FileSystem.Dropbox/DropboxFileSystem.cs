using System.Collections.Generic;
using BudgetBadger.Core.FileSystem;

namespace BudgetBadger.FileSystem.Dropbox
{
    public class DropboxFileSystem : IFileSystem
    {
        public DropboxFileSystem()
        {
            File = new DropboxFile();
            Directory = new DropboxDirectory();
        }

        public void SetAuthentication(IReadOnlyDictionary<string, string> keys)
        {
            File.SetAuthentication(keys);
            Directory.SetAuthentication(keys);
        }

        public IFile File { get; }
        public IDirectory Directory { get; }
    }
}
