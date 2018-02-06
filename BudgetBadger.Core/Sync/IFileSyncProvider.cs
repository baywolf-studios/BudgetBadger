using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public interface IFileSyncProvider
    {
        Task<Result> PullFiles(IDirectoryInfo destinationDirectory);
        Task<Result> PushFiles(IDirectoryInfo sourceDirectory);
    }
}
