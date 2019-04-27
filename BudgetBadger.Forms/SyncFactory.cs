using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;

namespace BudgetBadger.Forms
{
    public class SyncFactory : ISyncFactory
    {
        readonly IResourceContainer _resourceContainer;
        readonly ISettings _settings;
        readonly IDirectoryInfo _syncDirectory;
        readonly IAccountSyncLogic _accountSyncLogic;
        readonly IPayeeSyncLogic _payeeSyncLogic;
        readonly IEnvelopeSyncLogic _envelopeSyncLogic;
        readonly ITransactionSyncLogic _transactionSyncLogic;
        readonly KeyValuePair<string, IFileSyncProvider>[] _fileSyncProviders;

        public SyncFactory(IResourceContainer resourceContainer,
            ISettings settings,
            IDirectoryInfo syncDirectory,
            IAccountSyncLogic accountSyncLogic,
            IPayeeSyncLogic payeeSyncLogic,
            IEnvelopeSyncLogic envelopeSyncLogic,
            ITransactionSyncLogic transactionSyncLogic,
            KeyValuePair<string, IFileSyncProvider>[] fileSyncProviders)
        {
            _resourceContainer = resourceContainer;
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
                return _resourceContainer.GetResourceString("SyncDateTimeNever");
            }

            if (DateTime.TryParse(_settings.GetValueOrDefault(AppSettings.LastSyncDateTime), out DateTime dateTime))
            {
                return _resourceContainer.GetFormattedString("{0:g}", dateTime);
            }
            else
            {
                return _resourceContainer.GetResourceString("SyncDateTimeNever");
            }
        }
    }
}
