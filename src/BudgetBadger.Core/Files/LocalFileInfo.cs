using System;
using System.IO;

namespace BudgetBadger.Core.Files
{
    public class LocalFileInfo : IFileInfo
    {
        readonly FileInfo FileInfo;

        public LocalFileInfo(string fileName, IDirectoryInfo directory)
        {
            var filePath = Path.Combine(directory.FullName, fileName);
            FileInfo = new FileInfo(filePath);
        }

        public bool Exists => FileInfo.Exists;

        public string FullName => FileInfo.FullName;

        public string Name => FileInfo.Name;

        public Stream Open()
        {
            return FileInfo.Open(FileMode.Open);
        }
    }
}
