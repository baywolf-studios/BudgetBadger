using System.Collections.Generic;

namespace BudgetBadger.Core.CloudSync
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
