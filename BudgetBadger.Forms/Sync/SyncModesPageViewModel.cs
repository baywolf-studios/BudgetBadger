using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.FileSyncProvider.Dropbox;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Settings;
using BudgetBadger.Models;
using Dropbox.Api;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SimpleAuth;
using SimpleAuth.Providers;

namespace BudgetBadger.Forms.Sync
{
    public class SyncModesPageViewModel : BindableBase, INavigationAware
    {
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISettings _settings;
        readonly DropBoxApi _dropboxApi;

        public ICommand DropboxSelectedCommand { get; set; }
        public ICommand DisableSelectedCommand { get; set; }

        public SyncModesPageViewModel(INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      ISettings settings,
                                      DropBoxApi dropboxApi)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _settings = settings;
            _dropboxApi = dropboxApi;

            DropboxSelectedCommand = new DelegateCommand(async () => ExecuteDropboxSelectedCommand());
            DisableSelectedCommand = new DelegateCommand(async () => ExecuteDisableSelectedCommand());
        }

        public async Task ExecuteDisableSelectedCommand()
        {
            await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
            await _navigationService.GoBackAsync();
        }

        public async Task ExecuteDropboxSelectedCommand()
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
                }
            }
            catch (Exception ex)
            {
                await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
                await _dialogService.DisplayAlertAsync("Authentication Unsuccessful", ex.Message, "Ok");
            }
            finally
            {
                await _navigationService.GoBackAsync();
            }
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
        }
    }
}
