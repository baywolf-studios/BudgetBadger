using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.FileSyncProvider.Dropbox;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Settings;
using BudgetBadger.Models;
using Dropbox.Api;
using Prism.Commands;
using Prism.Ioc;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Sync
{
    public class FileSyncProvidersPageViewModel : INavigationAware
    {
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;
        readonly IOAuth2Authenticator Authenticator;
        readonly ISettings Settings;
        readonly IContainerRegistry ContainerRegistry;

        public ICommand DropBoxSyncCommand { get; set; }

        public FileSyncProvidersPageViewModel(INavigationService navigationService,
                                              IPageDialogService dialogService,
                                              IOAuth2Authenticator authenticator,
                                              ISettings settings)
        {
            NavigationService = navigationService;
            DialogService = dialogService;
            Authenticator = authenticator;
            Settings = settings;

            DropBoxSyncCommand = new DelegateCommand(ExecuteDropBoxSyncCommand);
        }

        public void ExecuteDropBoxSyncCommand()
        {
            var callbackUrl = new Uri(@"budgetbadger://authorize");
            var authorizeUri = DropboxFileSyncHelper.GetAuthorizationUri(callbackUrl);
            Authenticator.Authenticate(authorizeUri.AbsoluteUri, callbackUrl.AbsoluteUri, HandleOAuth2AthenticationHandler);
        }

        public async void HandleOAuth2AthenticationHandler(Result<Uri> result)
        {
            if (result.Success)
            {
                var accessToken = DropboxFileSyncHelper.GetAccessToken(result.Data);

                await Settings.AddOrUpdateValueAsync(SettingsKeys.SyncMode, SyncMode.DropboxSync);
                await Settings.AddOrUpdateValueAsync(SettingsKeys.DropboxAccessToken, accessToken);

                await NavigationService.GoBackAsync();
            }
            else
            {
                await Settings.AddOrUpdateValueAsync(SettingsKeys.SyncMode, SyncMode.NoSync);
                await DialogService.DisplayAlertAsync("Authentication Failed", "Did not authenticate with Dropbox", "Ok");
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
            DropBoxSyncCommand.Execute(null);
        }
    }
}
