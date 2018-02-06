using System;
using System.Collections.Generic;
using System.IO;

namespace BudgetBadger.Core.Files
{
    public interface IDirectoryInfo : IFileSystemInfo
    {
        bool Exists { get; }

        IEnumerable<IFileInfo> GetFiles();

        // creates or overwrites the file in the directory with the name passed in
        Stream CreateFile(string fileName);
    }
}
