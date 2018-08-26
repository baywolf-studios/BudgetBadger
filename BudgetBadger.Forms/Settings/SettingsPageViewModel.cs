using System;
using System.Threading.Tasks;
using System.Windows.Input;
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

        public ICommand SyncToggleCommand { get; set; }
        public ICommand ShowDeletedCommand { get; set; }

        bool _dropboxEnabled;
        public bool DropboxEnabled
        {
            get => _dropboxEnabled;
            set => SetProperty(ref _dropboxEnabled, value);
        }

        public SettingsPageViewModel(INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      ISettings settings,
                                      DropBoxApi dropboxApi)
        {
            _navigationService = navigationService;
            _settings = settings;
            _dialogService = dialogService;
            _dropboxApi = dropboxApi;

            SyncToggleCommand = new DelegateCommand(async () => await ExecuteSyncToggleCommand());
            ShowDeletedCommand = new DelegateCommand<string>(async (obj) => await ExecuteShowDeletedCommand(obj));
        }

        public async Task ExecuteSyncToggleCommand()
        {
			var syncMode = _settings.GetValueOrDefault(AppSettings.SyncMode);

			if (syncMode != SyncMode.DropboxSync && DropboxEnabled)
            {
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

        public void OnAppearing()
        {
            var syncMode = _settings.GetValueOrDefault(AppSettings.SyncMode);

            if (syncMode == SyncMode.DropboxSync)
            {
                DropboxEnabled = true;
            }
            else
            {
                DropboxEnabled = false;
            }
        }

        public void OnDisappearing()
        {
        }
    }
}
