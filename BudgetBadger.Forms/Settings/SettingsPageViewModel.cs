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

        public ICommand SyncToggleCommand { get; set; }
        public ICommand ShowDeletedCommand { get; set; }
        public ICommand RestoreProCommand { get; set; }
        public ICommand PurchaseProCommand { get; set; }

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

        public SettingsPageViewModel(INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      ISettings settings,
                                      DropBoxApi dropboxApi,
                                      IPurchaseService purchaseService)
        {
            _navigationService = navigationService;
            _settings = settings;
            _dialogService = dialogService;
            _dropboxApi = dropboxApi;
            _purchaseService = purchaseService;

            HasPro = false;

            SyncToggleCommand = new DelegateCommand(async () => await ExecuteSyncToggleCommand());
            ShowDeletedCommand = new DelegateCommand<string>(async (obj) => await ExecuteShowDeletedCommand(obj));
            RestoreProCommand = new DelegateCommand(async () => await ExecuteRestoreProCommand());
            PurchaseProCommand = new DelegateCommand(async () => await ExecutePurchaseProCommand());
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

                if (!purchaseResult.Success)
                {
                    await _dialogService.DisplayAlertAsync("Purchase Unsuccessful", purchaseResult.Message, "Ok");
                }
            }
        }
    }
}
