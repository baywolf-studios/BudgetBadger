using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public class NoFileSyncProvider : IFileSyncProvider
    {
        public NoFileSyncProvider()
        {
        }

        public Task<Result> PullFilesTo(IDirectoryInfo destinationDirectory)
        {
            return Task.FromResult(new Result { Success = true });
        }

        public Task<Result> PushFilesFrom(IDirectoryInfo sourceDirectory)
        {
            return Task.FromResult(new Result { Success = true });
        }

        public Task<Result> IsValid()
        {
            return Task.FromResult(new Result { Success = true });
        }
    }
}
