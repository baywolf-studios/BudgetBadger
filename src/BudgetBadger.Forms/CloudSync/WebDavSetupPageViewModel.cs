using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.CloudSync;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Settings;
using BudgetBadger.FileSystem.WebDav;
using BudgetBadger.Forms.Enums;
using ImTools;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.CloudSync
{
    public class WebDavSetupPageViewModel : BaseViewModel
    {
        private readonly ICloudSync _cloudSync;
        private readonly IPageDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IResourceContainer _resourceContainer;
        private readonly ISettings _settings;

        private string _busyText;

        private bool _isBusy;
        private string _webDavDirectory;
        private string _webDavPassword;

        private string _webDavServer;
        private bool _webDavAcceptInvalidCertificate;
        private string _webDavServerError;
        private string _webDavUsername;

        public WebDavSetupPageViewModel(INavigationService navigationService,
            IPageDialogService dialogService,
            ISettings settings,
            ICloudSync cloudSync,
            IResourceContainer resourceContainer)
        {
            _navigationService = navigationService;
            _resourceContainer = resourceContainer;
            _dialogService = dialogService;
            _settings = settings;
            _cloudSync = cloudSync;

            SaveCommand = new Command(async () => await ExecuteSaveCommand());
        }

        public ICommand SaveCommand { get; set; }

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

        public string WebDavServer
        {
            get => _webDavServer;
            set
            {
                SetProperty(ref _webDavServer, value);
                WebDavServerError = string.IsNullOrEmpty(value)
                    ? _resourceContainer.GetResourceString("RequiredError")
                    : string.Empty;
            }
        }

        public string WebDavServerError
        {
            get => _webDavServerError;
            set => SetProperty(ref _webDavServerError, value);
        }

        public bool WebDavAcceptInvalidCertificate
        {
            get => _webDavAcceptInvalidCertificate;
            set => SetProperty(ref _webDavAcceptInvalidCertificate, value);
        }
        
        public string WebDavDirectory
        {
            get => _webDavDirectory;
            set => SetProperty(ref _webDavDirectory, value);
        }

        public string WebDavUsername
        {
            get => _webDavUsername;
            set => SetProperty(ref _webDavUsername, value);
        }

        public string WebDavPassword
        {
            get => _webDavPassword;
            set => SetProperty(ref _webDavPassword, value);
        }

        private async Task ExecuteSaveCommand()
        {
            WebDavServerError = string.IsNullOrEmpty(WebDavServer)
                ? _resourceContainer.GetResourceString("RequiredError")
                : string.Empty;
            
            if (IsBusy || string.IsNullOrEmpty(WebDavServer))
            {
                return;
            }

            IsBusy = true;
            BusyText = _resourceContainer.GetResourceString("BusyTextSaving");

            try
            {
                await _settings.AddOrUpdateValueAsync(WebDavSettings.Server, WebDavServer);
                await _settings.AddOrUpdateValueAsync(WebDavSettings.AcceptInvalidCertificate,
                    WebDavAcceptInvalidCertificate.ToString());
                await _settings.AddOrUpdateValueAsync(WebDavSettings.Directory, WebDavDirectory);
                await _settings.AddOrUpdateValueAsync(WebDavSettings.Username, WebDavUsername);
                await _settings.AddOrUpdateValueAsync(WebDavSettings.Password, WebDavPassword);
                await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.WebDav);

                BusyText = _resourceContainer.GetResourceString("BusyTextSyncing");

                var syncResult = await _cloudSync.Sync();

                if (syncResult.Failure)
                {
                    await _cloudSync.DisableCloudSync();
                    await _dialogService.DisplayAlertAsync(
                        _resourceContainer.GetResourceString("AlertSyncUnsuccessful"),
                        syncResult.Message,
                        _resourceContainer.GetResourceString("AlertOk"));
                }
                else
                {
                    await _navigationService.GoBackAsync();
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
                IsBusy = false;
            }
        }
    }
}
