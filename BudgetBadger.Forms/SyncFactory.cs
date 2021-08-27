using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.FileSyncProvider.Dropbox;
using BudgetBadger.FileSyncProvider.Dropbox.Authentication;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Prism.Services;

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
        readonly IDropboxAuthentication _dropboxAuthentication;
        readonly IPageDialogService _dialogService;

        public SyncFactory(IResourceContainer resourceContainer,
            ISettings settings,
            IDirectoryInfo syncDirectory,
            IAccountSyncLogic accountSyncLogic,
            IPayeeSyncLogic payeeSyncLogic,
            IEnvelopeSyncLogic envelopeSyncLogic,
            ITransactionSyncLogic transactionSyncLogic,
            KeyValuePair<string, IFileSyncProvider>[] fileSyncProviders,
            IDropboxAuthentication dropboxAuthentication,
            IPageDialogService pageDialogService)
        {
            _resourceContainer = resourceContainer;
            _settings = settings;
            _syncDirectory = syncDirectory;
            _accountSyncLogic = accountSyncLogic;
            _payeeSyncLogic = payeeSyncLogic;
            _envelopeSyncLogic = envelopeSyncLogic;
            _transactionSyncLogic = transactionSyncLogic;
            _fileSyncProviders = fileSyncProviders;
            _dropboxAuthentication = dropboxAuthentication;
            _dialogService = pageDialogService;
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
            var syncMode = _settings.GetValueOrDefault(AppSettings.SyncMode);
            if (String.IsNullOrEmpty(syncMode) || syncMode == SyncMode.NoSync)
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

        public async Task<Result> EnableDropboxCloudSync()
        {
            var result = new Result();

            try
            {
                var dropboxResult = await _dropboxAuthentication.GetRefreshTokenAsync();

                if (dropboxResult.Success)
                {
                    await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.DropboxSync);
                    await _settings.AddOrUpdateValueAsync(DropboxSettings.RefreshToken, dropboxResult.Data);
                    result.Success = true;
                }
                else
                {
                    await DisableDropboxCloudSync();
                    result.Success = false;
                    result.Message = _resourceContainer.GetResourceString("AlertMessageDropboxError");
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertAuthenticationUnsuccessful"),
                                _resourceContainer.GetResourceString("AlertMessageDropboxError"),
                                _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            catch (Exception ex)
            {
                await DisableDropboxCloudSync();
                result.Success = false;
                result.Message = ex.Message;
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertAuthenticationUnsuccessful"),
                            ex.Message,
                            _resourceContainer.GetResourceString("AlertOk"));
            }

            return result;
        }

        public async Task DisableDropboxCloudSync()
        {
            await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
        }
    }
}
