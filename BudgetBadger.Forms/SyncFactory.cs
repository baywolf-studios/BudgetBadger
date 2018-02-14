using System;
using System.Collections.Generic;
using BudgetBadger.Core.Files;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;

namespace BudgetBadger.Forms
{
    public static class SyncFactory
    {
        public static ISync CreateSync(ISettings settings,
                                       IFileSyncProvider fileSyncProvider,
                                       IDirectoryInfo syncDirectory,
                                       IAccountSyncLogic accountSyncLogic,
                                       IPayeeSyncLogic payeeSyncLogic,
                                       IEnvelopeSyncLogic envelopeSyncLogic,
                                       ITransactionSyncLogic transactionSyncLogic)
        {
            if (settings.GetValueOrDefault(SettingsKeys.SyncMode) != SyncMode.NoSync)
            {
                return new FileSync(syncDirectory,
                                    fileSyncProvider,
                                    accountSyncLogic,
                                    payeeSyncLogic,
                                    envelopeSyncLogic,
                                    transactionSyncLogic);
            }

            return new NoSync();
        }
    }
}
