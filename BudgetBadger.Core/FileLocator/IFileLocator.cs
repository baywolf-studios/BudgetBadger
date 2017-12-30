using System;
namespace BudgetBadger.Core.FileLocator
{
    public interface IFileLocator
    {
        string GetFilePath(string fileName);
    }
}
