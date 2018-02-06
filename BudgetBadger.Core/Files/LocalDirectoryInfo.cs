using System;
using System.Collections.Generic;
using System.IO;

namespace BudgetBadger.Core.Files
{
    public class LocalDirectoryInfo : IDirectoryInfo
    {
        readonly DirectoryInfo DirectoryInfo;

        public LocalDirectoryInfo(string directoryPath)
        {
            DirectoryInfo = new DirectoryInfo(directoryPath);
        }

        public bool Exists => DirectoryInfo.Exists;

        public string FullName => DirectoryInfo.FullName;

        public string Name => DirectoryInfo.Name;

        public Stream CreateFile(string fileName)
        {
            var filePath = Path.Combine(this.FullName, fileName);
            return File.Create(filePath);
        }

        public IEnumerable<IFileInfo> GetFiles()
        {
            var files = new List<LocalFileInfo>();

            foreach (FileInfo file in DirectoryInfo.GetFiles())
            {
                files.Add(new LocalFileInfo(file.Name, this));
            }

            return files;
        }
    }
}
