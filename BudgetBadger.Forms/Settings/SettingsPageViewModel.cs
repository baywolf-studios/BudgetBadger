using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Purchase;
using BudgetBadger.Core.Settings;
using BudgetBadger.FileSyncProvider.Dropbox;
using BudgetBadger.Forms.Enums;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SimpleAuth;
using SimpleAuth.Providers;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Settings
{
    public class SettingsPageViewModel : BindableBase, IPageLifecycleAware, INavigatingAware
    {
        const string BudgetBadgerProMacAppLink = "macappstore://itunes.apple.com/app/id402437824?mt=12";

        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISettings _settings;
        readonly DropBoxApi _dropboxApi;
        readonly IPurchaseService _purchaseService;
        readonly ISyncFactory _syncFactory;
        readonly ILocalize _localize;

        public ICommand SyncToggleCommand { get; set; }
        public ICommand ShowDeletedCommand { get; set; }
        public ICommand RestoreProCommand { get; set; }
        public ICommand PurchaseProCommand { get; set; }
        public ICommand SyncCommand { get; set; }
        public ICommand HelpCommand { get => new DelegateCommand(() => Device.OpenUri(new Uri("http://BudgetBadger.io"))); }
        public ICommand EmailCommand { get => new DelegateCommand(() => Device.OpenUri(new Uri("mailto:support@BudgetBadger.io"))); }
        public ICommand CurrencySelectedCommand { get; set; }
        public ICommand DateSelectedCommand { get; set; }

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
            set => SetProperty(ref _hasPro, value);
        }

        bool _showSyncButton;
        public bool ShowSync
        {
            get => _showSyncButton;
            set => SetProperty(ref _showSyncButton, value);
        }

        string _lastSynced;
        public string LastSynced
        {
            get => _lastSynced;
            set => SetProperty(ref _lastSynced, value);
        }

        List<KeyValuePair<string, CultureInfo>> _currencyFormatList;
        public List<KeyValuePair<string, CultureInfo>> CurrencyFormatList
        {
            get => _currencyFormatList;
            set => SetProperty(ref _currencyFormatList, value);
        }

        KeyValuePair<string, CultureInfo> _selectedCurrencyFormat;
        public KeyValuePair<string, CultureInfo> SelectedCurrencyFormat
        {
            get => _selectedCurrencyFormat;
            set => SetProperty(ref _selectedCurrencyFormat, value);
        }

        List<KeyValuePair<string, CultureInfo>> _dateFormatList;
        public List<KeyValuePair<string, CultureInfo>> DateFormatList
        {
            get => _dateFormatList;
            set => SetProperty(ref _dateFormatList, value);
        }

        int _selectedDateFormatIndex;
        public int SelectedDateFormatIndex
        {
            get => _selectedDateFormatIndex;
            set => SetProperty(ref _selectedDateFormatIndex, value);
        }

        KeyValuePair<string, CultureInfo> _selectedDateFormat;
        public KeyValuePair<string, CultureInfo> SelectedDateFormat
        {
            get => _selectedDateFormat;
            set => SetProperty(ref _selectedDateFormat, value);
        }

        public SettingsPageViewModel(INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      ISettings settings,
                                      DropBoxApi dropboxApi,
                                      IPurchaseService purchaseService,
                                      ISyncFactory syncFactory,
                                      ILocalize localize)
        {
            _navigationService = navigationService;
            _settings = settings;
            _dialogService = dialogService;
            _dropboxApi = dropboxApi;
            _purchaseService = purchaseService;
            _syncFactory = syncFactory;
            _localize = localize;

            HasPro = false;
            IsBusy = false;
            CurrencyFormatList = new List<KeyValuePair<string, CultureInfo>>();
            DateFormatList = new List<KeyValuePair<string, CultureInfo>>();

            SyncToggleCommand = new DelegateCommand(async () => await ExecuteSyncToggleCommand());
            ShowDeletedCommand = new DelegateCommand<string>(async (obj) => await ExecuteShowDeletedCommand(obj));
            RestoreProCommand = new DelegateCommand(async () => await ExecuteRestoreProCommand());
            PurchaseProCommand = new DelegateCommand(async () => await ExecutePurchaseProCommand());
            SyncCommand = new DelegateCommand(async () => await ExecuteSyncCommand());
            CurrencySelectedCommand = new DelegateCommand(async () => await ExecuteCurrencySelectedCommand());
            DateSelectedCommand = new DelegateCommand(async () => await ExecuteDateSelectedCommand());
        }

        public void OnNavigatingTo(INavigationParameters parameters)
        {
            if (CurrencyFormatList.Count == 0)
            {
                CurrencyFormatList.AddRange(GetCurrencies());
            }

            if (DateFormatList.Count == 0)
            {
                DateFormatList.AddRange(GetDateFormats());
            }

            var currentCurrencyFormat = _settings.GetValueOrDefault(AppSettings.CurrencyFormat);
            if (CurrencyFormatList.Any(c => c.Value.Name == currentCurrencyFormat))
            {
                SelectedCurrencyFormat = CurrencyFormatList.FirstOrDefault(c => c.Value.Name == currentCurrencyFormat);
            }
            else
            {
                SelectedCurrencyFormat = CurrencyFormatList.FirstOrDefault(c => c.Key == "Automatic");
            }

            var currentDateFormat = _settings.GetValueOrDefault(AppSettings.DateFormat);
            if (DateFormatList.Any(d => d.Value.Name == currentDateFormat))
            {
                SelectedDateFormat = DateFormatList.FirstOrDefault(d => d.Value.Name == currentDateFormat);
            }
            else
            {
                SelectedDateFormat = DateFormatList.FirstOrDefault(c => c.Key == "Automatic");
            }
        }

        public async void OnAppearing()
        {
            var purchasedPro = await _purchaseService.VerifyPurchaseAsync(Purchases.Pro);

            HasPro = purchasedPro.Success;

            var syncMode = _settings.GetValueOrDefault(AppSettings.SyncMode);
            DropboxEnabled = (syncMode == SyncMode.DropboxSync);
            ShowSync = (syncMode == SyncMode.DropboxSync);
            LastSynced = _syncFactory.GetLastSyncDateTime();
        }

        public void OnDisappearing()
        {
        }

        List<KeyValuePair<string, CultureInfo>> GetCurrencies()
        {
            var result = new Dictionary<string, CultureInfo>
            {
                { "Automatic", CultureInfo.InvariantCulture }
            };

            var allCultures = new List<CultureInfo>
            {
                new CultureInfo("en-US"), // USD
                new CultureInfo("fr-FR"), // EUR
                new CultureInfo("ja-JP"), // JPY
                new CultureInfo("en-GB"), // GBP
                new CultureInfo("en-AU"), // AUD
                new CultureInfo("en-CA"), // CAD
                new CultureInfo("de-CH"), // CNH
                new CultureInfo("zh-CN"), // CHF
                new CultureInfo("sv-SE"), // SEK
                new CultureInfo("en-NZ"), // NZD
                new CultureInfo("es-MX") // MXN
            };

            var otherCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            allCultures.AddRange(otherCultures);

            foreach (var culture in allCultures)
            {
                try
                {
                    var region = new RegionInfo(culture.LCID);
                    var numberFormat = String.Join(": ", region.ISOCurrencySymbol, (-1234567.89).ToString("C", culture.NumberFormat));
                    result[numberFormat] = culture;
                }
                catch (Exception ex)
                {

                }
            }

            return result.ToList();
        }

        List<KeyValuePair<string, CultureInfo>> GetDateFormats()
        {
            var result = new Dictionary<string, CultureInfo>
            {
                { "Automatic", CultureInfo.InvariantCulture }
            };

            var allCultures = new List<CultureInfo>
            {
                new CultureInfo("en-US"), // USD
                new CultureInfo("en-GB"), // GBP
                new CultureInfo("fr-FR"), // EUR
                new CultureInfo("ja-JP"), // JPY
                new CultureInfo("en-AU"), // AUD
                new CultureInfo("en-CA"), // CAD
                new CultureInfo("de-CH"), // CNH
                new CultureInfo("zh-CN"), // CHF
                new CultureInfo("sv-SE"), // SEK
                new CultureInfo("en-NZ"), // NZD
                new CultureInfo("es-MX") // MXN
            };

            var otherCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            allCultures.AddRange(otherCultures);

            foreach (var culture in allCultures.Where(c => DateTime.Now >= c.Calendar.MinSupportedDateTime && DateTime.Now <= c.Calendar.MaxSupportedDateTime))
            {
                try
                {
                    var dateFormat = String.Join(" : ", DateTime.Now.ToString("d", culture.DateTimeFormat), DateTime.Now.ToString("Y", culture.DateTimeFormat));
                    result[dateFormat] = culture;
                }
                catch (Exception ex)
                {

                }
            }

            return result.ToList();
        }

        public async Task ExecuteSyncToggleCommand()
        {
            var syncMode = _settings.GetValueOrDefault(AppSettings.SyncMode);

            if (syncMode != SyncMode.DropboxSync && DropboxEnabled)
            {
                var verifyPurchase = await _purchaseService.VerifyPurchaseAsync(Purchases.Pro);
                HasPro = verifyPurchase.Success;

                if (!HasPro)
                {
                    var wantToPurchase = await _dialogService.DisplayAlertAsync("Budget Badger Pro", "You currently do not have access to these features. Would you like to purchase Budget Badger Pro?", "Purchase", "Cancel");
                    if (wantToPurchase)
                    {
                        await ExecutePurchaseProCommand();
                    }
                }

                if (HasPro)
                {
                    try
                    {
                        _dropboxApi.ForceRefresh = true;

                        var account = await _dropboxApi.Authenticate() as OAuthAccount;

                        if (account.IsValid())
                        {
                            await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.DropboxSync);
                            await _settings.AddOrUpdateValueAsync(DropboxSettings.AccessToken, account.Token);
                            await ExecuteSyncCommand();
                            ShowSync = true;
                        }
                        else
                        {
                            DropboxEnabled = false;
                            await _dialogService.DisplayAlertAsync("Authentication Unsuccessful", "Did not authenticate with Dropbox. Sync disabled.", "Ok");
                        }
                    }
                    catch (Exception ex)
                    {
                        DropboxEnabled = false;
                        await _dialogService.DisplayAlertAsync("Authentication Unsuccessful", ex.Message, "Ok");
                    }
                }
                else
                {
                    DropboxEnabled = false;
                }
            }

            if (!DropboxEnabled)
            {
                await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
            }

            ShowSync = DropboxEnabled;
            LastSynced = _syncFactory.GetLastSyncDateTime();
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
                if (Device.RuntimePlatform == Device.macOS)
                {
                    Device.OpenUri(new Uri(BudgetBadgerProMacAppLink));
                }
                else
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

        public async Task ExecuteCurrencySelectedCommand()
        {
            var current = (CultureInfo)_localize.GetLocale().Clone();

            if (SelectedCurrencyFormat.Key == "Automatic")
            {
                var device = _localize.GetDeviceCultureInfo();
                current.NumberFormat = device.NumberFormat;
                await _settings.AddOrUpdateValueAsync(AppSettings.CurrencyFormat, "Automatic");
            }
            else
            {
                current.NumberFormat = SelectedCurrencyFormat.Value.NumberFormat;
                await _settings.AddOrUpdateValueAsync(AppSettings.CurrencyFormat, SelectedCurrencyFormat.Value.Name);
            }

            _localize.SetLocale(current);
        }

        public async Task ExecuteDateSelectedCommand()
        {
            var current = (CultureInfo)_localize.GetLocale().Clone();
            if (SelectedDateFormat.Key == "Automatic")
            {
                var device = _localize.GetDeviceCultureInfo();
                current.DateTimeFormat = device.DateTimeFormat;
                await _settings.AddOrUpdateValueAsync(AppSettings.DateFormat, "Automatic");
            }
            else
            {
                current.DateTimeFormat = SelectedDateFormat.Value.DateTimeFormat;
                await _settings.AddOrUpdateValueAsync(AppSettings.DateFormat, SelectedDateFormat.Value.Name);
            }

            _localize.SetLocale(current);
        }
    }
}