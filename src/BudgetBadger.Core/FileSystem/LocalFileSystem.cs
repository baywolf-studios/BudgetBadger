using System.Collections.Generic;

namespace BudgetBadger.Core.FileSystem
{
    public class LocalFileSystem : IFileSystem
    {
        public void SetAuthentication(IReadOnlyDictionary<string, string> keys)
        {
        }

        public IFile File => new LocalFile();
        public IDirectory Directory => new LocalDirectory();
    }
}
