using System;
namespace BudgetBadger.Core.Files
{
    public interface IFileSystemInfo
    {
        string FullName { get; }
        string Name { get; }
    }
}
