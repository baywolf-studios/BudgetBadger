using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;

namespace BudgetBadger.Forms
{
    public class SyncFactory : ISyncFactory
    {
        readonly ISettings _settings;
        readonly IDirectoryInfo _syncDirectory;
        readonly IAccountSyncLogic _accountSyncLogic;
        readonly IPayeeSyncLogic _payeeSyncLogic;
        readonly IEnvelopeSyncLogic _envelopeSyncLogic;
        readonly ITransactionSyncLogic _transactionSyncLogic;
        readonly KeyValuePair<string, IFileSyncProvider>[] _fileSyncProviders;

        public SyncFactory(ISettings settings,
            IDirectoryInfo syncDirectory,
            IAccountSyncLogic accountSyncLogic,
            IPayeeSyncLogic payeeSyncLogic,
            IEnvelopeSyncLogic envelopeSyncLogic,
            ITransactionSyncLogic transactionSyncLogic,
            KeyValuePair<string, IFileSyncProvider>[] fileSyncProviders)
        {
            _settings = settings;
            _syncDirectory = syncDirectory;
            _accountSyncLogic = accountSyncLogic;
            _payeeSyncLogic = payeeSyncLogic;
            _envelopeSyncLogic = envelopeSyncLogic;
            _transactionSyncLogic = transactionSyncLogic;
            _fileSyncProviders = fileSyncProviders;
        }

        public ISync GetSyncService()
        {
            return StaticSyncFactory.CreateSync(_settings, _syncDirectory, _accountSyncLogic, _payeeSyncLogic, _envelopeSyncLogic, _transactionSyncLogic, _fileSyncProviders);
        }

        public async Task SetLastSyncDateTime(DateTime dateTime)
        {
            await _settings.AddOrUpdateValueAsync(AppSettings.LastSyncDateTime, dateTime.ToString());
        }

        public string GetLastSyncDateTime()
        {
            if(_settings.GetValueOrDefault(AppSettings.SyncMode) == SyncMode.NoSync)
            {
                return "Never";
            }

            if (DateTime.TryParse(_settings.GetValueOrDefault(AppSettings.LastSyncDateTime), out DateTime dateTime))
            {
                return dateTime.ToString("g");
            }
            else
            {
                return "Never";
            }
        }
    }
}
