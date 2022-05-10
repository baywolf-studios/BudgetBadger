using System;
using System.IO;
using BudgetBadger.Core.Files;

namespace BudgetBadger.Forms
{
    public static class FileLocator
    {
        static readonly string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        static readonly string applicationPath = Path.Combine(documentsPath, "BudgetBadger");

        public static IDirectoryInfo GetBudgetsPath()
        {
            var localPath = Path.Combine(applicationPath, "budgets");

            Directory.CreateDirectory(localPath);

            return new LocalDirectoryInfo(localPath);
        }

        public static IFileInfo GetBudgetFilePath(string budgetName)
        {
            var localPath = GetBudgetsPath();

            Directory.CreateDirectory(localPath.FullName);

            return new LocalFileInfo(budgetName, localPath);
        }

        public static IDirectoryInfo GetSyncPath()
        {
            var syncPath = Path.Combine(applicationPath, "sync");

            Directory.CreateDirectory(syncPath);

            return new LocalDirectoryInfo(syncPath);
        }

        public static IFileInfo GetSyncFilePath(string fileName)
        {
            var syncPath = GetSyncPath();

            Directory.CreateDirectory(syncPath.FullName);

            return new LocalFileInfo(fileName, syncPath);
        }
    }
}
