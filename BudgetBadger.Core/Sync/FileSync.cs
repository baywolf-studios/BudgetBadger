using System;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Core;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Files;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public class FileSync : Sync
    {
        static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        readonly IFileSyncProvider FileProvider;
        readonly IDirectoryInfo SyncDirectory;

        public FileSync(IDirectoryInfo syncDirectory,
                        IFileSyncProvider fileProvider,
                        IAccountSyncLogic accountSyncLogic,
                        IPayeeSyncLogic payeeSyncLogic,
                        IEnvelopeSyncLogic envelopeSyncLogic,
                        ITransactionSyncLogic transactionSyncLogic)
            : base(accountSyncLogic,
                  payeeSyncLogic,
                  envelopeSyncLogic,
                  transactionSyncLogic)
        {
            SyncDirectory = syncDirectory;
            FileProvider = fileProvider;
        }

        public override async Task<Result> FullSync()
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                var result = await FileProvider.PullFilesTo(SyncDirectory);

                if (result.Success)
                {
                    result = await base.FullSync();
                }

                if (result.Success)
                {
                    result = await FileProvider.PushFilesFrom(SyncDirectory);
                }

                return result;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public override async Task<Result> Pull()
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                var result = await FileProvider.PullFilesTo(SyncDirectory);

                if (result.Success)
                {
                    result = await base.Pull();
                }

                return result;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public override async Task<Result> Push()
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                var result = await base.Push();

                if (result.Success)
                {
                    result = await FileProvider.PushFilesFrom(SyncDirectory);
                }

                return result;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
