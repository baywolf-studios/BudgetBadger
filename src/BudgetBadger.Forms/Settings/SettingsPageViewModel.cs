using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
        readonly IResourceContainer _resourceContainer;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISettings _settings;
        readonly ISyncFactory _syncFactory;
        readonly ILocalize _localize;

        string _detect;

        public ICommand SyncToggleCommand { get; set; }
        public ICommand SyncCommand { get; set; }
        public ICommand HelpCommand { get => new Command(async () => await Browser.OpenAsync(new Uri("https://BudgetBadger.io"))); }
        public ICommand CurrencySelectedCommand { get; set; }
        public ICommand DateSelectedCommand { get; set; }
        public ICommand LanguageSelectedCommand { get; set; }
        public ICommand DimensionSelectedCommand { get; set; }
        public ICommand LicenseCommand { get; set; }
        public ICommand ThirdPartyNoticesCommand { get; set; }

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

        KeyValuePair<string, CultureInfo> _selectedLanguage;
        public KeyValuePair<string, CultureInfo> SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
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

        public IList<string> DimensionList
        {
            get => Enum.GetNames(typeof(DimensionSize)).Select(_resourceContainer.GetResourceString).ToList();
        }

        DimensionSize _selectedDimensionSize;
        public DimensionSize SelectedDimensionSize
        {
            get => _selectedDimensionSize;
            set => SetProperty(ref _selectedDimensionSize, value);
        }

        public SettingsPageViewModel(IResourceContainer resourceContainer,
                                     INavigationService navigationService,
                                     IPageDialogService dialogService,
                                     ISettings settings,
                                     ISyncFactory syncFactory,
                                     ILocalize localize)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _settings = settings;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _localize = localize;

            IsBusy = false;
            _detect = _resourceContainer.GetResourceString("DetectLabel");

            SyncToggleCommand = new Command(async () => await ExecuteSyncToggleCommand());
            SyncCommand = new Command(async () => await ExecuteSyncCommand());
            CurrencySelectedCommand = new Command(async () => await ExecuteCurrencySelectedCommand());
            LanguageSelectedCommand = new Command(async () => await ExecuteLanguageSelectedCommand());
            DimensionSelectedCommand = new Command(async () => await ExecuteDimensionSelectedCommand());
            LicenseCommand = new Command(async () => await ExecuteLicenseCommand());
            ThirdPartyNoticesCommand = new Command(async () => await ExecuteThirdPartyNoticesCommand());

            ResetAppearance();
            ResetLocalization();
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
            DropboxEnabled = (syncMode == SyncMode.DropboxSync);
            ShowSync = (syncMode == SyncMode.DropboxSync);

            LastSynced = await _syncFactory.GetLastSyncDateTimeAsync();
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
                catch (Exception)
                {

                }
            }

            return result.ToList();
        }

        public async Task ExecuteSyncToggleCommand()
        {
            var syncMode = await _settings.GetValueOrDefaultAsync(AppSettings.SyncMode);

            if (syncMode != SyncMode.DropboxSync && DropboxEnabled)
            {
                var enableDropboxResult = await _syncFactory.EnableDropboxCloudSync();

                if (enableDropboxResult.Success)
                {
                    await ExecuteSyncCommand();
                }
                else
                {
                    DropboxEnabled = false;
                }  
            }

            if (!DropboxEnabled)
            {
                await _syncFactory.DisableDropboxCloudSync();
            }

            ShowSync = DropboxEnabled;
            LastSynced = await _syncFactory.GetLastSyncDateTimeAsync();
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
                var syncService = await _syncFactory.GetSyncServiceAsync();
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

                LastSynced = await _syncFactory.GetLastSyncDateTimeAsync();
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
                await _settings.AddOrUpdateValueAsync(AppSettings.AppearanceDimensionSize, Enum.GetName(typeof(DimensionSize), SelectedDimensionSize));

                ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                if (mergedDictionaries != null)
                {
                    var otherDicts = mergedDictionaries.Where(m => !(m is LargeDimensionResources)
                                                                && !(m is MediumDimensionResources)
                                                                && !(m is SmallDimensionResources)).ToList();

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
            Stream stream = assembly.GetManifestResourceStream($"{assemblyName}.LICENSE");
            string text = "";
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

        async void ResetAppearance()
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

        async void ResetLocalization()
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
                SelectedCurrencyFormat = CurrencyFormatList.FirstOrDefault(c => c.Value == CultureInfo.InvariantCulture);
            }
        }
    }
}