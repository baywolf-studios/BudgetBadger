using System.Collections.Generic;
using BudgetBadger.Core.FileSystem;

namespace BudgetBadger.FileSystem.WebDav
{
    public class WebDavFileSystem : IFileSystem
    {
        public WebDavFileSystem()
        {
            File = new WebDavFile();
            Directory = new WebDavDirectory();
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
