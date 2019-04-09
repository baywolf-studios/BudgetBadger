using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Purchase;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.FileSyncProvider.Dropbox;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Dropbox.Api;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SimpleAuth;
using SimpleAuth.Providers;

namespace BudgetBadger.Forms.Settings
{
    public class SettingsPageViewModel : BindableBase, IPageLifecycleAware
    {
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISettings _settings;
        readonly DropBoxApi _dropboxApi;
        readonly IPurchaseService _purchaseService;
        readonly ISyncFactory _syncFactory;

        public ICommand SyncToggleCommand { get; set; }
        public ICommand ShowDeletedCommand { get; set; }
        public ICommand RestoreProCommand { get; set; }
        public ICommand PurchaseProCommand { get; set; }
        public ICommand SyncCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        string _busyText;
        public string BusyText
        {
            get => _busyText;
            set => SetProperty(ref _busyText, value);
        }

        bool _dropboxEnabled;
        public bool DropboxEnabled
        {
            get => _dropboxEnabled;
            set => SetProperty(ref _dropboxEnabled, value);
        }

        bool _hasPro;
        public bool HasPro
        {
            get => _hasPro;
            set
            {
                SetProperty(ref _hasPro, value);
                RaisePropertyChanged(nameof(DoesNotHavePro));
            }
        }

        public bool DoesNotHavePro
        {
            get => !HasPro;
        }

        string _lastSynced;
        public string LastSynced
        {
            get => _lastSynced;
            set => SetProperty(ref _lastSynced, value);
        }

        public SettingsPageViewModel(INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      ISettings settings,
                                      DropBoxApi dropboxApi,
                                      IPurchaseService purchaseService,
                                      ISyncFactory syncFactory)
        {
            _navigationService = navigationService;
            _settings = settings;
            _dialogService = dialogService;
            _dropboxApi = dropboxApi;
            _purchaseService = purchaseService;
            _syncFactory = syncFactory;

            HasPro = false;
            IsBusy = false;

            SyncToggleCommand = new DelegateCommand(async () => await ExecuteSyncToggleCommand());
            ShowDeletedCommand = new DelegateCommand<string>(async (obj) => await ExecuteShowDeletedCommand(obj));
            RestoreProCommand = new DelegateCommand(async () => await ExecuteRestoreProCommand());
            PurchaseProCommand = new DelegateCommand(async () => await ExecutePurchaseProCommand());
            SyncCommand = new DelegateCommand(async () => await ExecuteSyncCommand());

            LastSynced = _syncFactory.GetLastSyncDateTime();
        }

        public async void OnAppearing()
        {
            var purchasedPro = await _purchaseService.VerifyPurchaseAsync(Purchases.Pro);

            HasPro = purchasedPro.Success;

            var syncMode = _settings.GetValueOrDefault(AppSettings.SyncMode);

            DropboxEnabled = (syncMode == SyncMode.DropboxSync);
        }

        public void OnDisappearing()
        {
        }

        public async Task ExecuteSyncToggleCommand()
        {
            var syncMode = _settings.GetValueOrDefault(AppSettings.SyncMode);

            if (syncMode != SyncMode.DropboxSync && DropboxEnabled)
            {
                var verifyPurchase = await _purchaseService.VerifyPurchaseAsync(Purchases.Pro);
                if (!verifyPurchase.Success)
                {
                    // ask if they'd like to purchase
                    var wantToPurchase = await _dialogService.DisplayAlertAsync("Budget Badger Pro", "You currently do not have access to these features. Would you like to purchase Budget Badger Pro?", "Purchase", "Cancel");

                    if (wantToPurchase)
                    {
                        var purchaseResult = await _purchaseService.PurchaseAsync(Purchases.Pro);
                        if (!purchaseResult.Success)
                        {
                            await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
                            await _dialogService.DisplayAlertAsync("Not Purchased", purchaseResult.Message, "Ok");
                            DropboxEnabled = false;
                            return;
                        }
                    }
                    else
                    {
                        await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
                        DropboxEnabled = false;
                        return;
                    }
                }

                try
                {
                    _dropboxApi.ForceRefresh = true;

                    var account = await _dropboxApi.Authenticate() as OAuthAccount;

                    if (account.IsValid())
                    {
                        await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.DropboxSync);
                        await _settings.AddOrUpdateValueAsync(DropboxSettings.AccessToken, account.Token);
                        await ExecuteSyncCommand();
                    }
                    else
                    {
                        await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
                        await _dialogService.DisplayAlertAsync("Authentication Unsuccessful", "Did not authenticate with Dropbox. Sync disabled.", "Ok");
                        DropboxEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
                    await _dialogService.DisplayAlertAsync("Authentication Unsuccessful", ex.Message, "Ok");
                    DropboxEnabled = false;
                }
            }
            else if (!DropboxEnabled)
            {
                await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
            }
        }

        public async Task ExecuteShowDeletedCommand(string pageName)
        {
            await _navigationService.NavigateAsync(pageName);
        }

        public async Task ExecuteRestoreProCommand()
        {
            var result = await _purchaseService.RestorePurchaseAsync(Purchases.Pro);

            HasPro = result.Success;

            if (!HasPro)
            {
                await _dialogService.DisplayAlertAsync("Restore Purchase Unsuccessful", result.Message, "Ok");
            }
        }

        public async Task ExecutePurchaseProCommand()
        {
            if (!HasPro)
            {
                var purchaseResult = await _purchaseService.PurchaseAsync(Purchases.Pro);

                if (purchaseResult.Success)
                {
                    HasPro = true;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Purchase Unsuccessful", purchaseResult.Message, "Ok");
                }
            }
        }

        public async Task ExecuteSyncCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = "Syncing...";

            try
            {
                var syncService = _syncFactory.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.SetLastSyncDateTime(DateTime.Now);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "Ok");
                }

                LastSynced = _syncFactory.GetLastSyncDateTime();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}