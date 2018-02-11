using System;
namespace BudgetBadger.Core.Files
{
    public interface IFileSystemInfo
    {
        bool Exists { get; }
        string FullName { get; }
        string Name { get; }
    }
}
