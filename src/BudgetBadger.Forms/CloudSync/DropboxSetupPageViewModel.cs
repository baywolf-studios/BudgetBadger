using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.CloudSync;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Utilities;
using BudgetBadger.FileSystem.Dropbox;
using BudgetBadger.Forms.Enums;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.CloudSync
{
    public class DropboxSetupPageViewModel : BaseViewModel, INavigatedAware
    {
        private readonly ICloudSync _cloudSync;
        private readonly IPageDialogService _dialogService;
        private readonly IDropboxAuthentication _dropboxAuthentication;
        private readonly INavigationService _navigationService;
        private readonly IResourceContainer _resourceContainer;
        private readonly ISettings _settings;

        private string _busyText;

        private bool _isBusy;

        public DropboxSetupPageViewModel(INavigationService navigationService,
            IPageDialogService dialogService,
            ISettings settings,
            ICloudSync cloudSync,
            IResourceContainer resourceContainer,
            IDropboxAuthentication dropboxAuthentication)
        {
            _navigationService = navigationService;
            _resourceContainer = resourceContainer;
            _dialogService = dialogService;
            _settings = settings;
            _cloudSync = cloudSync;
            _dropboxAuthentication = dropboxAuthentication;
        }

        public ICommand BackCommand => new Command(async () => await _navigationService.GoBackAsync());

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string BusyText
        {
            get => _busyText;
            set => SetProperty(ref _busyText, value);
        }


        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            EnableDropbox().FireAndForget();
        }

        private async Task EnableDropbox()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = _resourceContainer.GetResourceString("BusyTextLoading");

            try
            {
                var dropboxResult = await _dropboxAuthentication.GetRefreshTokenAsync(AppSecrets.DropBoxAppKey);

                if (dropboxResult.Success)
                {
                    BusyText = _resourceContainer.GetResourceString("BusyTextSyncing");

                    await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.Dropbox);
                    await _settings.AddOrUpdateValueAsync(DropboxSettings.RefreshToken, dropboxResult.Data);

                    var syncResult = await _cloudSync.Sync();

                    if (syncResult.Failure)
                    {
                        await _cloudSync.DisableCloudSync();
                        await _dialogService.DisplayAlertAsync(
                            _resourceContainer.GetResourceString("AlertSyncUnsuccessful"),
                            syncResult.Message,
                            _resourceContainer.GetResourceString("AlertOk"));
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(
                        _resourceContainer.GetResourceString("AlertAuthenticationUnsuccessful"),
                        _resourceContainer.GetResourceString("AlertMessageCloudSyncAuthenticationError"),
                        _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync(
                    _resourceContainer.GetResourceString("AlertAuthenticationUnsuccessful"),
                    ex.Message,
                    _resourceContainer.GetResourceString("AlertOk"));
            }
            finally
            {
                await _navigationService.GoBackAsync();
                IsBusy = false;
            }
        }
    }
}
