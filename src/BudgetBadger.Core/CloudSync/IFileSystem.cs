using System.Collections.Generic;

namespace BudgetBadger.Core.CloudSync
{
    public interface IFileSystem
    {
        IFile File { get; }

        IDirectory Directory { get; }

        void SetAuthentication(IReadOnlyDictionary<string, string> keys);
    }
}