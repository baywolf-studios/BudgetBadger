using System;
using System.IO;

namespace BudgetBadger.Core.Files
{
    public interface IFileInfo : IFileSystemInfo
    {
        Stream Open();
    }
}
