using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.CloudSync;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Settings;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Style;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Settings
{
    public class SettingsPageViewModel : BaseViewModel, INavigatedAware
    {
        private readonly ICloudSync _cloudSync;
        private readonly IPageDialogService _dialogService;
        private readonly ILocalize _localize;
        private readonly INavigationService _navigationService;
        private readonly IResourceContainer _resourceContainer;
        private readonly ISettings _settings;

        private string _busyText;

        private List<KeyValuePair<string, CultureInfo>> _currencyFormatList;

        private string _detect;

        private bool _dropboxEnabled;

        private bool _isBusy;

        private List<KeyValuePair<string, CultureInfo>> _languageList;

        private string _lastSynced;

        private KeyValuePair<string, CultureInfo> _selectedCurrencyFormat;

        private DimensionSize _selectedDimensionSize;

        private KeyValuePair<string, CultureInfo> _selectedLanguage;

        private bool _showSyncButton;
        private bool _webDavEnabled;

        public SettingsPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
            IPageDialogService dialogService,
            ISettings settings,
            ICloudSync cloudSync,
            ILocalize localize)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _settings = settings;
            _dialogService = dialogService;
            _cloudSync = cloudSync;
            _localize = localize;

            IsBusy = false;
            _detect = _resourceContainer.GetResourceString("DetectLabel");

            DropboxToggleCommand = new Command(async () => await ExecuteDropboxToggleCommand());
            WebDavToggleCommand = new Command(async () => ExecuteWebDavToggleCommand());
            SyncCommand = new Command(async () => await ExecuteSyncCommand());
            CurrencySelectedCommand = new Command(async () => await ExecuteCurrencySelectedCommand());
            LanguageSelectedCommand = new Command(async () => await ExecuteLanguageSelectedCommand());
            DimensionSelectedCommand = new Command(async () => await ExecuteDimensionSelectedCommand());
            LicenseCommand = new Command(async () => await ExecuteLicenseCommand());
            ThirdPartyNoticesCommand = new Command(async () => await ExecuteThirdPartyNoticesCommand());

            ResetAppearance();
            ResetLocalization();
        }

        public ICommand DropboxToggleCommand { get; set; }
        public ICommand SyncCommand { get; set; }

        public ICommand HelpCommand =>
            new Command(async () => await Browser.OpenAsync(new Uri("https://BudgetBadger.io")));

        public ICommand CurrencySelectedCommand { get; set; }
        public ICommand LanguageSelectedCommand { get; set; }
        public ICommand DimensionSelectedCommand { get; set; }
        public ICommand LicenseCommand { get; set; }
        public ICommand ThirdPartyNoticesCommand { get; set; }
        public ICommand WebDavToggleCommand { get; set; }

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

        public bool DropboxEnabled
        {
            get => _dropboxEnabled;
            set => SetProperty(ref _dropboxEnabled, value);
        }

        public bool WebDavEnabled
        {
            get => _webDavEnabled;
            set => SetProperty(ref _webDavEnabled, value);
        }

        public bool ShowSync
        {
            get => _showSyncButton;
            set => SetProperty(ref _showSyncButton, value);
        }

        public string LastSynced
        {
            get => _lastSynced;
            set => SetProperty(ref _lastSynced, value);
        }

        public List<KeyValuePair<string, CultureInfo>> LanguageList
        {
            get => _languageList;
            set => SetProperty(ref _languageList, value);
        }

        public KeyValuePair<string, CultureInfo> SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public List<KeyValuePair<string, CultureInfo>> CurrencyFormatList
        {
            get => _currencyFormatList;
            set => SetProperty(ref _currencyFormatList, value);
        }

        public KeyValuePair<string, CultureInfo> SelectedCurrencyFormat
        {
            get => _selectedCurrencyFormat;
            set => SetProperty(ref _selectedCurrencyFormat, value);
        }

        public IList<string> DimensionList =>
            Enum.GetNames(typeof(DimensionSize)).Select(_resourceContainer.GetResourceString).ToList();

        public DimensionSize SelectedDimensionSize
        {
            get => _selectedDimensionSize;
            set => SetProperty(ref _selectedDimensionSize, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public override async void OnActivated()
        {
            _detect = _resourceContainer.GetResourceString("DetectLabel");

            var syncMode = await _settings.GetValueOrDefaultAsync(AppSettings.SyncMode);
            DropboxEnabled = syncMode == SyncMode.Dropbox;
            WebDavEnabled = syncMode == SyncMode.WebDav;
            ShowSync = DropboxEnabled || WebDavEnabled;

            var lastSyncedDateTime = await _cloudSync.GetLastSyncDateTimeAsync();
            LastSynced = _resourceContainer.GetFormattedString("{0:g}", lastSyncedDateTime);
        }

        private List<KeyValuePair<string, CultureInfo>> GetLanguages()
        {
            var result = new Dictionary<string, CultureInfo> { { _detect, CultureInfo.InvariantCulture } };

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

        private List<KeyValuePair<string, CultureInfo>> GetCurrencies()
        {
            var result = new Dictionary<string, CultureInfo> { { _detect, CultureInfo.InvariantCulture } };

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
                    var numberFormat = string.Join(" ",
                        region.ISOCurrencySymbol,
                        (-1234567.89).ToString("C", culture.NumberFormat));
                    result[numberFormat] = culture;
                }
                catch (Exception)
                {
                }
            }

            return result.ToList();
        }

        public async Task ExecuteDropboxToggleCommand()
        {
            var syncMode = await _settings.GetValueOrDefaultAsync(AppSettings.SyncMode);

            if (syncMode != SyncMode.Dropbox && DropboxEnabled)
            {
                await _navigationService.NavigateAsync(PageName.DropboxSetupPage);
            }
            else if (syncMode == SyncMode.Dropbox && !DropboxEnabled)
            {
                await _cloudSync.DisableCloudSync();
                ShowSync = false;
                LastSynced = string.Empty;
            }
        }

        public async Task ExecuteWebDavToggleCommand()
        {
            var syncMode = await _settings.GetValueOrDefaultAsync(AppSettings.SyncMode);

            if (syncMode != SyncMode.WebDav && WebDavEnabled)
            {
                await _navigationService.NavigateAsync(PageName.WebDavSetupPage);
            }
            else if (syncMode == SyncMode.WebDav && !WebDavEnabled)
            {
                await _cloudSync.DisableCloudSync();
                ShowSync = false;
                LastSynced = string.Empty;
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
                var syncResult = await _cloudSync.Sync();

                if (syncResult.Failure)
                {
                    await _dialogService.DisplayAlertAsync(
                        _resourceContainer.GetResourceString("AlertSyncUnsuccessful"),
                        syncResult.Message,
                        _resourceContainer.GetResourceString("AlertOk"));
                }

                var lastSyncedDateTime = await _cloudSync.GetLastSyncDateTimeAsync();

                LastSynced = _resourceContainer.GetFormattedString("{0:g}", lastSyncedDateTime);
                ;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteLanguageSelectedCommand()
        {
            var current = (CultureInfo)_localize.GetLocale().Clone();

            if (SelectedLanguage.Value == CultureInfo.InvariantCulture) // set to device
            {
                var device = _localize.GetDeviceCultureInfo();
                current = device;
                await _settings.AddOrUpdateValueAsync(AppSettings.Language, string.Empty);
            }
            else // user choice
            {
                current = SelectedLanguage.Value;
                await _settings.AddOrUpdateValueAsync(AppSettings.Language, SelectedLanguage.Value.Name);
            }

            _localize.SetLocale(current);

            TranslationProvider.Instance.Invalidate();
        }

        public async Task ExecuteCurrencySelectedCommand()
        {
            var current = (CultureInfo)_localize.GetLocale().Clone();

            if (SelectedCurrencyFormat.Value == CultureInfo.InvariantCulture)
            {
                var device = _localize.GetDeviceCultureInfo();
                current.NumberFormat = device.NumberFormat;
                await _settings.AddOrUpdateValueAsync(AppSettings.CurrencyFormat, string.Empty);
            }
            else
            {
                current.NumberFormat = SelectedCurrencyFormat.Value.NumberFormat;
                await _settings.AddOrUpdateValueAsync(AppSettings.CurrencyFormat, SelectedCurrencyFormat.Value.Name);
            }

            _localize.SetLocale(current);

            TranslationProvider.Instance.Invalidate();
        }

        public async Task ExecuteDimensionSelectedCommand()
        {
            if ((int)SelectedDimensionSize > -1)
            {
                await _settings.AddOrUpdateValueAsync(AppSettings.AppearanceDimensionSize,
                    Enum.GetName(typeof(DimensionSize), SelectedDimensionSize));

                var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                if (mergedDictionaries != null)
                {
                    var otherDicts = mergedDictionaries.Where(m =>
                            !(m is LargeDimensionResources) &&
                            !(m is MediumDimensionResources) &&
                            !(m is SmallDimensionResources))
                        .ToList();

                    mergedDictionaries.Clear();
                    foreach (var dict in otherDicts)
                    {
                        mergedDictionaries.Add(dict);
                    }

                    switch (SelectedDimensionSize)
                    {
                        case DimensionSize.DimensionSizeLarge:
                            mergedDictionaries.Add(new LargeDimensionResources());
                            break;
                        case DimensionSize.DimensionSizeSmall:
                            mergedDictionaries.Add(new SmallDimensionResources());
                            break;
                        case DimensionSize.DimensionSizeMedium:
                        default:
                            mergedDictionaries.Add(new MediumDimensionResources());
                            break;
                    }
                }

                DynamicResourceProvider.Instance.Invalidate();
            }
        }

        public async Task ExecuteLicenseCommand()
        {
            var assembly = typeof(ThirdPartyNoticesPageViewModel).Assembly;
            var assemblyName = assembly.GetName().Name;
            var stream = assembly.GetManifestResourceStream($"{assemblyName}.LICENSE");
            var text = "";
            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.LicenseName, _resourceContainer.GetResourceString("BudgetBadger") },
                { PageParameter.LicenseText, text }
            };

            await _navigationService.NavigateAsync(PageName.LicensePage, parameters);
        }

        public async Task ExecuteThirdPartyNoticesCommand()
        {
            await _navigationService.NavigateAsync(PageName.ThirdPartyNoticesPage);
        }

        private async void ResetAppearance()
        {
            var currentDimension = await _settings.GetValueOrDefaultAsync(AppSettings.AppearanceDimensionSize);

            if (Enum.TryParse(currentDimension, out DimensionSize selectedDimensionSize))
            {
                SelectedDimensionSize = selectedDimensionSize;
            }
            else
            {
                SelectedDimensionSize = DimensionSize.DimensionSizeMedium;
            }
        }

        private async void ResetLocalization()
        {
            _detect = _resourceContainer.GetResourceString("DetectLabel");

            LanguageList = GetLanguages();

            var currentLanguage = await _settings.GetValueOrDefaultAsync(AppSettings.Language);
            if (LanguageList.Any(d => d.Value.Name == currentLanguage))
            {
                SelectedLanguage = LanguageList.FirstOrDefault(d => d.Value.Name == currentLanguage);
            }
            else
            {
                SelectedLanguage = LanguageList.FirstOrDefault(c => c.Value == CultureInfo.InvariantCulture);
            }

            CurrencyFormatList = GetCurrencies();

            var currentCurrencyFormat = await _settings.GetValueOrDefaultAsync(AppSettings.CurrencyFormat);
            if (CurrencyFormatList.Any(c => c.Value.Name == currentCurrencyFormat))
            {
                SelectedCurrencyFormat = CurrencyFormatList.FirstOrDefault(c => c.Value.Name == currentCurrencyFormat);
            }
            else
            {
                SelectedCurrencyFormat =
                    CurrencyFormatList.FirstOrDefault(c => c.Value == CultureInfo.InvariantCulture);
            }
        }
    }
}
