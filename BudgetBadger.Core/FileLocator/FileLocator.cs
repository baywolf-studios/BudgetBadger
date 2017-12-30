using System;
using System.IO;

namespace BudgetBadger.Core.FileLocator
{
    public class FileLocator : IFileLocator
    {
        readonly string appFolderName = "BudgetBadger";

        public string GetFilePath(string fileName)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            string applicationPath = Path.Combine(documentsPath, appFolderName);

            Directory.CreateDirectory(applicationPath);

            var result = Path.Combine(applicationPath, fileName);

            return result;
        }
    }
}
