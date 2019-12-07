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
using Prism;
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
    public class SettingsPageViewModel : BaseViewModel, IInitialize
    {
        readonly IResourceContainer _resourceContainer;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISettings _settings;
        readonly DropBoxApi _dropboxApi;
        readonly IPurchaseService _purchaseService;
        readonly ISyncFactory _syncFactory;
        readonly ILocalize _localize;

        string _detect;

        public ICommand SyncToggleCommand { get; set; }
        public ICommand RestoreProCommand { get; set; }
        public ICommand PurchaseProCommand { get; set; }
        public ICommand SyncCommand { get; set; }
        public ICommand HelpCommand { get => new DelegateCommand(() => Device.OpenUri(new Uri("http://BudgetBadger.io"))); }
        public ICommand EmailCommand { get => new DelegateCommand(() => Device.OpenUri(new Uri("mailto:support@BudgetBadger.io"))); }
        public ICommand CurrencySelectedCommand { get; set; }
        public ICommand DateSelectedCommand { get; set; }
        public ICommand LanguageSelectedCommand { get; set; }

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

        List<KeyValuePair<string, CultureInfo>> _languageList;
        public List<KeyValuePair<string, CultureInfo>> LanguageList
        {
            get => _languageList;
            set => SetProperty(ref _languageList, value);
        }

        int _selectedLanguageIndex;
        public int SelectedLanguageIndex
        {
            get => _selectedLanguageIndex;
            set => SetProperty(ref _selectedLanguageIndex, value);
        }

        List<KeyValuePair<string, CultureInfo>> _currencyFormatList;
        public List<KeyValuePair<string, CultureInfo>> CurrencyFormatList
        {
            get => _currencyFormatList;
            set => SetProperty(ref _currencyFormatList, value);
        }

        int _selectedCurrencyFormatIndex;
        public int SelectedCurrencyFormatIndex
        {
            get => _selectedCurrencyFormatIndex;
            set => SetProperty(ref _selectedCurrencyFormatIndex, value);
        }

        public SettingsPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      ISettings settings,
                                      DropBoxApi dropboxApi,
                                      IPurchaseService purchaseService,
                                      ISyncFactory syncFactory,
                                      ILocalize localize)
        {
            _resourceContainer = resourceContainer;
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
            LanguageList = new List<KeyValuePair<string, CultureInfo>>();
            _detect = _resourceContainer.GetResourceString("DetectLabel");

            SyncToggleCommand = new DelegateCommand(async () => await ExecuteSyncToggleCommand());
            RestoreProCommand = new DelegateCommand(async () => await ExecuteRestoreProCommand());
            PurchaseProCommand = new DelegateCommand(async () => await ExecutePurchaseProCommand());
            SyncCommand = new DelegateCommand(async () => await ExecuteSyncCommand());
            CurrencySelectedCommand = new DelegateCommand(async () => await ExecuteCurrencySelectedCommand());
            LanguageSelectedCommand = new DelegateCommand(async () => await ExecuteLanguageSelectedCommand());
        }

        public void Initialize(INavigationParameters parameters)
        {
            ResetLocalization();
        }

        public override async void OnActivated()
        {
            ResetLocalization();

            _detect = _resourceContainer.GetResourceString("DetectLabel");

            var purchasedPro = await _purchaseService.VerifyPurchaseAsync(Purchases.Pro);

            HasPro = purchasedPro.Success;

            var syncMode = _settings.GetValueOrDefault(AppSettings.SyncMode);
            DropboxEnabled = (syncMode == SyncMode.DropboxSync);
            ShowSync = (syncMode == SyncMode.DropboxSync);

            LastSynced = _syncFactory.GetLastSyncDateTime();
        }

        List<KeyValuePair<string, CultureInfo>> GetLanguages()
        {
            var result = new Dictionary<string, CultureInfo>
            {
                { _detect, CultureInfo.InvariantCulture }
            };

            var english = new CultureInfo("en-US");
            result.Add(english.DisplayName, english);

            var englishUK = new CultureInfo("en-GB");
            result.Add(englishUK.DisplayName, englishUK);

            var german = new CultureInfo("de");
            result.Add(german.DisplayName, german);

            //var spanish = new CultureInfo("es");
            //result.Add(spanish.DisplayName, spanish);

            return result.ToList();
        }

        List<KeyValuePair<string, CultureInfo>> GetCurrencies()
        {
            var result = new Dictionary<string, CultureInfo>
            {
                { _detect, CultureInfo.InvariantCulture }
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
                    var numberFormat = String.Join(" ", region.ISOCurrencySymbol, (-1234567.89).ToString("C", culture.NumberFormat));
                    result[numberFormat] = culture;
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
                    var wantToPurchase = await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertBudgetBadgerPro"),
                    _resourceContainer.GetResourceString("AlertMessageBudgetBadgerPro"),
                    _resourceContainer.GetResourceString("AlertPurchase"),
                    _resourceContainer.GetResourceString("AlertCancel"));
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
                            await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertAuthenticationUnsuccessful"),
                                _resourceContainer.GetResourceString("AlertMessageDropboxError"),
                                _resourceContainer.GetResourceString("AlertOk"));
                        }
                    }
                    catch (Exception ex)
                    {
                        DropboxEnabled = false;
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertAuthenticationUnsuccessful"),
                            ex.Message,
                            _resourceContainer.GetResourceString("AlertOk"));
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

        public async Task ExecuteRestoreProCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = _resourceContainer.GetResourceString("BusyTextLoading");

            try
            {
                var result = await _purchaseService.RestorePurchaseAsync(Purchases.Pro);

                HasPro = result.Success;

                if (!HasPro)
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRestorePurchaseUnsuccessful"),
                        result.Message,
                        _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecutePurchaseProCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = _resourceContainer.GetResourceString("BusyTextLoading");

            try
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
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertPurchaseUnsuccessful"),
                            purchaseResult.Message,
                            _resourceContainer.GetResourceString("AlertOk"));
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSyncCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = _resourceContainer.GetResourceString("BusyTextSyncing");

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
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSyncUnsuccessful"),
                        syncResult.Message,
                        _resourceContainer.GetResourceString("AlertOk"));
                }

                LastSynced = _syncFactory.GetLastSyncDateTime();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteLanguageSelectedCommand()
        {
            if (SelectedLanguageIndex > -1)
            {
                var current = (CultureInfo)_localize.GetLocale().Clone();

            
                var selectedLanguage = LanguageList.ElementAtOrDefault(SelectedLanguageIndex);

                if (selectedLanguage.Value == CultureInfo.InvariantCulture) // set to device
                {
                    var device = _localize.GetDeviceCultureInfo();
                    current = device;
                    await _settings.AddOrUpdateValueAsync(AppSettings.Language, string.Empty);
                }
                else // user choice
                {
                    current = selectedLanguage.Value;
                    await _settings.AddOrUpdateValueAsync(AppSettings.Language, selectedLanguage.Value.Name);
                }

                _localize.SetLocale(current);
            }
            
        }

        public async Task ExecuteCurrencySelectedCommand()
        {
            if (SelectedCurrencyFormatIndex > -1)
            {
                var current = (CultureInfo)_localize.GetLocale().Clone();

                var selectedCurrencyFormat = CurrencyFormatList.ElementAtOrDefault(SelectedCurrencyFormatIndex);

                if (selectedCurrencyFormat.Value == CultureInfo.InvariantCulture)
                {
                    var device = _localize.GetDeviceCultureInfo();
                    current.NumberFormat = device.NumberFormat;
                    await _settings.AddOrUpdateValueAsync(AppSettings.CurrencyFormat, string.Empty);
                }
                else
                {
                    current.NumberFormat = selectedCurrencyFormat.Value.NumberFormat;
                    await _settings.AddOrUpdateValueAsync(AppSettings.CurrencyFormat, selectedCurrencyFormat.Value.Name);
                }

                _localize.SetLocale(current);
            }
        }

        void ResetLocalization()
        {
            _detect = _resourceContainer.GetResourceString("DetectLabel");

            LanguageList = GetLanguages();

            var currentLanguage = _settings.GetValueOrDefault(AppSettings.Language);
            if (LanguageList.Any(d => d.Value.Name == currentLanguage))
            {
                SelectedLanguageIndex = LanguageList.IndexOf(LanguageList.FirstOrDefault(d => d.Value.Name == currentLanguage));
            }
            else
            {
                SelectedLanguageIndex = LanguageList.IndexOf(LanguageList.FirstOrDefault(c => c.Value == CultureInfo.InvariantCulture));
            }

            CurrencyFormatList = GetCurrencies();

            var currentCurrencyFormat = _settings.GetValueOrDefault(AppSettings.CurrencyFormat);
            if (CurrencyFormatList.Any(c => c.Value.Name == currentCurrencyFormat))
            {
                SelectedCurrencyFormatIndex = CurrencyFormatList.IndexOf(CurrencyFormatList.FirstOrDefault(c => c.Value.Name == currentCurrencyFormat));
            }
            else
            {
                SelectedCurrencyFormatIndex = CurrencyFormatList.IndexOf(CurrencyFormatList.FirstOrDefault(c => c.Value == CultureInfo.InvariantCulture));
            }
        }
    }
}