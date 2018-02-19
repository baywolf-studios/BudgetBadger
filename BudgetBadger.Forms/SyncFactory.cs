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
                                       DropboxFileSyncProvider dropboxFileSyncProvider)
        {
            if (settings.GetValueOrDefault(SettingsKeys.SyncMode) == SyncMode.DropboxSync)
            {
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
