using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.FileSyncProvider.Dropbox;
using BudgetBadger.Forms.Enums;

namespace BudgetBadger.Forms
{
    public static class SyncFactory
    {
        public static ISync CreateSync(ISettings settings,
                                       IDirectoryInfo syncDirectory,
                                       IAccountSyncLogic accountSyncLogic,
                                       IPayeeSyncLogic payeeSyncLogic,
                                       IEnvelopeSyncLogic envelopeSyncLogic,
                                       ITransactionSyncLogic transactionSyncLogic,
                                       KeyValuePair<string, IFileSyncProvider>[] fileSyncProviders)
        {
            if (settings.GetValueOrDefault(AppSettings.SyncMode) == SyncMode.DropboxSync)
            {
                var dropboxFileSyncProvider = fileSyncProviders.FirstOrDefault(f => f.Key == SyncMode.DropboxSync).Value;

                return new FileSync(syncDirectory,
                                    dropboxFileSyncProvider,
                                    accountSyncLogic,
                                    payeeSyncLogic,
                                    envelopeSyncLogic,
                                    transactionSyncLogic);
            }

            return new NoSync();
        }
    }
}
