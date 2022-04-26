using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public interface IFileSyncProvider
    {
        Task<Result> PullFilesTo(IDirectoryInfo destinationDirectory);
        Task<Result> PushFilesFrom(IDirectoryInfo sourceDirectory);
    }
}
