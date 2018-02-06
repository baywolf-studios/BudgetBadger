using System;
using System.IO;

namespace BudgetBadger.Core
{
    public static class FileLocator
    {
        static readonly string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        static readonly string applicationPath = Path.Combine(documentsPath, "BudgetBadger");

        public static string GetLocalPath()
        {
            var localPath = Path.Combine(applicationPath, "local");

            Directory.CreateDirectory(localPath);

            return localPath;
        }

        public static string GetLocalFilePath(string fileName)
        {
            var localPath = GetLocalPath();

            Directory.CreateDirectory(localPath);

            var result = Path.Combine(localPath, fileName);

            return result;
        }

        public static string GetSyncPath()
        {
            var syncPath = Path.Combine(applicationPath, "sync");

            Directory.CreateDirectory(syncPath);

            return syncPath;
        }

        public static string GetSyncFilePath(string fileName)
        {
            var syncPath = GetSyncPath();

            Directory.CreateDirectory(syncPath);

            var result = Path.Combine(syncPath, fileName);

            return result;
        }
    }
}
