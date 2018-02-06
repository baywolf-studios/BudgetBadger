using System;
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
        readonly IFileSyncProvider FileProvider;
        readonly IDirectoryInfo SyncDirectory;

        public FileSync(IDirectoryInfo syncDirectory,
                        IFileSyncProvider fileProvider,
                        IAccountDataAccess localAccountDataAccess,
                        IAccountDataAccess remoteAccountDataAccess,
                        IPayeeDataAccess localPayeeDataAccess,
                        IPayeeDataAccess remotePayeeDataAccess,
                        IEnvelopeDataAccess localEnvelopeDataAccess,
                        IEnvelopeDataAccess remoteEnvelopeDataAccess,
                        ITransactionDataAccess localTransactionDataAccess,
                        ITransactionDataAccess remoteTransactionDataAccess)
            : base(localAccountDataAccess,
                  remoteAccountDataAccess,
                  localPayeeDataAccess,
                  remotePayeeDataAccess,
                  localEnvelopeDataAccess,
                  remoteEnvelopeDataAccess,
                  localTransactionDataAccess,
                  remoteTransactionDataAccess)
        {
            SyncDirectory = syncDirectory;
            FileProvider = fileProvider;
        }

        public override async Task<Result> FullSync()
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

        public override async Task<Result> Pull()
        {
            var result = await FileProvider.PullFilesTo(SyncDirectory);

            if (result.Success)
            {
                result = await base.Pull();
            }

            return result;
        }

        public override async Task<Result> Push()
        {
            var result = await base.Push();

            if (result.Success)
            {
                result = await FileProvider.PushFilesFrom(SyncDirectory);
            }

            return result;
        }
    }
}
