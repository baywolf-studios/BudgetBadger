using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Core.CloudSync;
using BudgetBadger.DataAccess;
using BudgetBadger.Core.FileSystem;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Utilities;
using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.FileSystem.Dropbox;
using BudgetBadger.FileSystem.WebDav;
using BudgetBadger.Forms.Accounts;
using BudgetBadger.Forms.Authentication;
using BudgetBadger.Forms.CloudSync;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Envelopes;
using BudgetBadger.Forms.Events;
using BudgetBadger.Forms.Payees;
using BudgetBadger.Forms.Reports;
using BudgetBadger.Forms.Settings;
using BudgetBadger.Forms.Style;
using BudgetBadger.Forms.Transactions;
using BudgetBadger.Forms.ViewModels;
using BudgetBadger.Forms.Views;
using DryIoc;
using Prism;
using Prism.AppModel;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BudgetBadger.Logic;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace BudgetBadger.Forms
{
    public partial class App : PrismApplication
    {
        private Timer _syncTimer;
        private static bool _isSyncing;

        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App() : this(null)
        {
        }

        public App(IPlatformInitializer initializer) : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            var settings = Container.Resolve<ISettings>();
            var appOCount = await settings.GetValueOrDefaultAsync(AppSettings.AppOpenedCount);
            int.TryParse(appOCount, out var appOpenedCount);
            if (appOpenedCount > 0)
            {
                if (Device.Idiom == TargetIdiom.Desktop)
                {
                    var parameters = new NavigationParameters { { PageParameter.PageName, "EnvelopesPage" } };
                    await NavigationService.NavigateAsync("/MainPage/NavigationPage/EnvelopesPage", parameters);
                }
                else
                {
                    await NavigationService.NavigateAsync("MainPage");
                }
            }
            else
            {
                if (Device.Idiom == TargetIdiom.Desktop)
                {
                    var parameters = new NavigationParameters { { PageParameter.PageName, "AccountsPage" } };
                    await NavigationService.NavigateAsync("/MainPage/NavigationPage/AccountsPage", parameters);
                }
                else
                {
                    await NavigationService.NavigateAsync("MainPage?selectedTab=AccountsPage");
                }
            }

            _syncTimer = new Timer(_ => Sync().FireAndForget(), null, Timeout.Infinite, Timeout.Infinite);

            SetAppTheme(Application.Current.RequestedTheme);
            await SetAppearanceSize();
            await SetLocale();

            Application.Current.RequestedThemeChanged += (s, a) => { SetAppTheme(a.RequestedTheme); };

            var eventAggregator = Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<AccountDeletedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<AccountHiddenEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<AccountSavedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<AccountUnhiddenEvent>().Subscribe(x => ResetSyncTimer());

            eventAggregator.GetEvent<BudgetSavedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<EnvelopeDeletedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<EnvelopeHiddenEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<EnvelopeUnhiddenEvent>().Subscribe(x => ResetSyncTimer());

            eventAggregator.GetEvent<EnvelopeGroupDeletedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<EnvelopeGroupHiddenEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<EnvelopeGroupUnhiddenEvent>().Subscribe(x => ResetSyncTimer());

            eventAggregator.GetEvent<PayeeDeletedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<PayeeHiddenEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<PayeeSavedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<PayeeUnhiddenEvent>().Subscribe(x => ResetSyncTimer());

            eventAggregator.GetEvent<SplitTransactionSavedEvent>().Subscribe(() => ResetSyncTimer());
            eventAggregator.GetEvent<SplitTransactionStatusUpdatedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<TransactionStatusUpdatedEvent>().Subscribe(x => ResetSyncTimer());
        }

        protected override async void OnStart()
        {
            await CleanupTempDirectory();
            await UpgradeApp();

            // tracking number of times app opened
            var settings = Container.Resolve<ISettings>();
            var appOCount = await settings.GetValueOrDefaultAsync(AppSettings.AppOpenedCount);
            int.TryParse(appOCount, out var appOpenedCount);
            appOpenedCount++;
            await settings.AddOrUpdateValueAsync(AppSettings.AppOpenedCount, appOpenedCount.ToString());

            ResetSyncTimer(1);
        }

        protected override void OnResume()
        {
            ResetSyncTimer(10);
        }

        protected override Rules CreateContainerRules()
        {
            return base.CreateContainerRules().WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var timer = Stopwatch.StartNew();

            var container = containerRegistry.GetContainer();

            container.Register<ISettings, SecureStorageSettings>();
            container.Register<IResourceContainer, ResourceContainer>();

            var appDataDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BudgetBadger");
            Directory.CreateDirectory(appDataDirectory);

            var dataDirectory = Path.Combine(appDataDirectory, "data");
            Directory.CreateDirectory(dataDirectory);

            //default data access
            var defaultConnectionString = SqliteConnectionStringBuilder.Get(Path.Combine(dataDirectory, "default.bb"));
            container.UseInstance(defaultConnectionString, serviceKey: "defaultConnectionString");
            container.Register<IDataAccess>(made: Made.Of(() =>
                new SqliteDataAccess(Arg.Of<string>("defaultConnectionString"))));

            //default logic
            container.Register<ITransactionLogic, TransactionLogic>();
            container.Register<IAccountLogic, AccountLogic>();
            container.Register<IPayeeLogic, PayeeLogic>();
            container.Register<IEnvelopeLogic, EnvelopeLogic>();
            container.Register<IReportLogic, ReportLogic>();

            if (Device.RuntimePlatform != Device.UWP)
            {
                container.Register<IWebAuthenticator, WebAuthenticator>();
            }

            container.Register<ITempSqliteDataAccessFactory, TempSqliteDataAccessFactory>();
            container.Register<IFileSystem, LocalFileSystem>(serviceKey: Enums.FileSystem.Local);
            container.Register<IFileSystem, DropboxFileSystem>(serviceKey: Enums.FileSystem.Dropbox);
            container.Register<IFileSystem, WebDavFileSystem>(serviceKey: Enums.FileSystem.WebDav);
            container.Register<IDropboxAuthentication, DropboxAuthentication>();
            container.Register<ISyncEngine, SyncEngine>();
            container.Register<ICloudSync, FileCloudSync>();

            containerRegistry.RegisterForNavigationOnIdiom<MainPage, MainPageViewModel>(
                desktopView: typeof(MainDesktopPage),
                tabletView: typeof(MainTabletPage));

            containerRegistry.RegisterForNavigation<MyPage>("NavigationPage");
            containerRegistry.RegisterForNavigationOnIdiom<AccountsPage, AccountsPageViewModel>(
                desktopView: typeof(AccountsDetailedPage),
                tabletView: typeof(AccountsDetailedPage));
            containerRegistry.RegisterForNavigation<AccountSelectionPage, AccountSelectionPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<AccountInfoPage, AccountInfoPageViewModel>(
                desktopView: typeof(AccountInfoDetailedPage),
                tabletView: typeof(AccountInfoDetailedPage));
            containerRegistry.RegisterForNavigation<AccountEditPage, AccountEditPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<AccountReconcilePage, AccountReconcilePageViewModel>(
                desktopView: typeof(AccountReconcileDetailedPage),
                tabletView: typeof(AccountReconcileDetailedPage));
            containerRegistry.RegisterForNavigation<HiddenAccountsPage, HiddenAccountsPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<PayeesPage, PayeesPageViewModel>(
                desktopView: typeof(PayeesDetailedPage),
                tabletView: typeof(PayeesDetailedPage));
            containerRegistry.RegisterForNavigation<PayeeSelectionPage, PayeeSelectionPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<PayeeInfoPage, PayeeInfoPageViewModel>(
                desktopView: typeof(PayeeInfoDetailedPage),
                tabletView: typeof(PayeeInfoDetailedPage));
            containerRegistry.RegisterForNavigation<PayeeEditPage, PayeeEditPageViewModel>();
            containerRegistry.RegisterForNavigation<HiddenPayeesPage, HiddenPayeesPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<EnvelopesPage, EnvelopesPageViewModel>(
                desktopView: typeof(EnvelopesDetailedPage),
                tabletView: typeof(EnvelopesDetailedPage));
            containerRegistry.RegisterForNavigation<EnvelopeSelectionPage, EnvelopeSelectionPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<EnvelopeInfoPage, EnvelopeInfoPageViewModel>(
                desktopView: typeof(EnvelopeInfoDetailedPage),
                tabletView: typeof(EnvelopeInfoDetailedPage));
            containerRegistry.RegisterForNavigation<EnvelopeEditPage, EnvelopeEditPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeTransferPage, EnvelopeTransferPageViewModel>();
            containerRegistry.RegisterForNavigation<HiddenEnvelopesPage, HiddenEnvelopesPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<EnvelopeGroupsPage, EnvelopeGroupsPageViewModel>(
                desktopView: typeof(EnvelopeGroupsDetailedPage),
                tabletView: typeof(EnvelopeGroupsDetailedPage));
            containerRegistry.RegisterForNavigation<EnvelopeGroupSelectionPage, EnvelopeGroupSelectionPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeGroupEditPage, EnvelopeGroupEditPageViewModel>();
            containerRegistry.RegisterForNavigation<HiddenEnvelopeGroupsPage, HiddenEnvelopeGroupsPageViewModel>();
            containerRegistry.RegisterForNavigation<TransactionEditPage, TransactionEditPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<SplitTransactionPage, SplitTransactionPageViewModel>(
                desktopView: typeof(SplitTransactionDetailedPage),
                tabletView: typeof(SplitTransactionDetailedPage));
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
            containerRegistry.RegisterForNavigation<DropboxSetupPage, DropboxSetupPageViewModel>();
            containerRegistry.RegisterForNavigation<WebDavSetupPage, WebDavSetupPageViewModel>();
            containerRegistry.RegisterForNavigation<ThirdPartyNoticesPage, ThirdPartyNoticesPageViewModel>();
            containerRegistry.RegisterForNavigation<LicensePage, LicensePageViewModel>();
            containerRegistry.RegisterForNavigation<ReportsPage, ReportsPageViewModel>();
            containerRegistry.RegisterForNavigation<NetWorthReportPage, NetWorthReportPageViewModel>();
            containerRegistry
                .RegisterForNavigation<EnvelopesSpendingReportPage, EnvelopesSpendingReportPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeTrendsReportPage, EnvelopeTrendsReportsPageViewModel>();
            containerRegistry.RegisterForNavigation<PayeesSpendingReportPage, PayeesSpendingReportPageViewModel>();
            containerRegistry.RegisterForNavigation<PayeeTrendsReportPage, PayeeTrendsReportPageViewModel>();

            timer.Stop();
        }

        private void SetAppTheme(OSAppTheme oSAppTheme)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                var dictsToRemove = mergedDictionaries.Where(m => m is LightThemeResources || m is DarkThemeResources)
                    .ToList();

                foreach (var dict in dictsToRemove)
                {
                    mergedDictionaries.Remove(dict);
                }

                switch (oSAppTheme)
                {
                    case OSAppTheme.Dark:
                        mergedDictionaries.Add(new DarkThemeResources());
                        break;
                    case OSAppTheme.Light:
                    default:
                        mergedDictionaries.Add(new LightThemeResources());
                        break;
                }
            }

            DynamicResourceProvider.Instance.Invalidate();
        }

        private async Task SetAppearanceSize()
        {
            var settings = Container.Resolve<ISettings>();

            var currentDimension = await settings.GetValueOrDefaultAsync(AppSettings.AppearanceDimensionSize);

            if (Enum.TryParse(currentDimension, out DimensionSize selectedDimensionSize))
            {
                var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                if (mergedDictionaries != null)
                {
                    var dictsToRemove = mergedDictionaries.Where(m =>
                            m is LargeDimensionResources ||
                            m is MediumDimensionResources ||
                            m is SmallDimensionResources)
                        .ToList();

                    foreach (var dict in dictsToRemove)
                    {
                        mergedDictionaries.Remove(dict);
                    }

                    switch (selectedDimensionSize)
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


        private async Task SetLocale()
        {
            var localize = Container.Resolve<ILocalize>();
            var settings = Container.Resolve<ISettings>();

            var currentLanguage = await settings.GetValueOrDefaultAsync(AppSettings.Language);
            CultureInfo currentCulture = null;

            if (!string.IsNullOrEmpty(currentLanguage))
            {
                try
                {
                    currentCulture = new CultureInfo(currentLanguage);
                }
                catch (Exception)
                {
                    // culture doesn't exist
                }
            }

            if (currentCulture == null)
            {
                currentCulture = (CultureInfo)localize.GetDeviceCultureInfo().Clone();
            }


            var currentCurrencyFormat = await settings.GetValueOrDefaultAsync(AppSettings.CurrencyFormat);
            if (!string.IsNullOrEmpty(currentCurrencyFormat))
            {
                try
                {
                    var currencyCulture = new CultureInfo(currentCurrencyFormat);
                    currentCulture.NumberFormat = currencyCulture.NumberFormat;
                }
                catch (Exception)
                {
                    // culture doesn't exist
                }
            }

            localize.SetLocale(currentCulture);
        }

        private void ResetSyncTimer(int seconds = 20)
        {
            var interval = (long)TimeSpan.FromSeconds(seconds).TotalMilliseconds;
            _syncTimer.Change(interval, Timeout.Infinite);
        }

        private async Task Sync()
        {
            if (_isSyncing)
            {
                return;
            }

            try
            {
                _isSyncing = true;
                var syncService = Container.Resolve<ICloudSync>();
                await syncService.Sync();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private async Task UpgradeApp()
        {
            await MigrateFromApplicationStoreToSecureStorageSettings();
            var settings = Container.Resolve<ISettings>();
            var appMigrationVersionString = await settings.GetValueOrDefaultAsync(AppSettings.AppMigrationVersion);

            int.TryParse(appMigrationVersionString, out var appMigrationVersion);

            switch (appMigrationVersion)
            {
                case 0:
                    await settings.AddOrUpdateValueAsync(AppSettings.AppMigrationVersion, "4");
                    break;
                case 1:
                    await UpgradeAppFromV1ToV2();
                    break;
                case 2:
                    await UpgradeAppFromV2ToV3();
                    break;
                case 3:
                    await UpgradeAppFromV3ToV4();
                    break;
            }
        }

        private async Task MigrateFromApplicationStoreToSecureStorageSettings()
        {
            var appStore = new ApplicationStore();

            if (appStore.Properties.Any())
            {
                var settings = Container.Resolve<ISettings>();
                foreach (var setting in appStore.Properties)
                {
                    if (setting.Key == "CurrentAppVersion")
                    {
                        int.TryParse(setting.Value.ToString(), out var currentAppVersion);
                        var appMigrationVersion = currentAppVersion + 1;
                        await settings.AddOrUpdateValueAsync(AppSettings.AppMigrationVersion,
                            appMigrationVersion.ToString());
                    }
                    else
                    {
                        await settings.AddOrUpdateValueAsync(setting.Key, setting.Value.ToString());
                    }
                }

                appStore.Properties.Clear();
                await appStore.SavePropertiesAsync();
            }
        }

        private async Task UpgradeAppFromV1ToV2()
        {
            var settings = Container.Resolve<ISettings>();
            var syncService = Container.Resolve<ICloudSync>();
            await syncService.DisableCloudSync();

            await settings.AddOrUpdateValueAsync(AppSettings.AppMigrationVersion, "2");
        }

        private async Task UpgradeAppFromV2ToV3()
        {
            var settings = Container.Resolve<ISettings>();
            var syncService = Container.Resolve<ICloudSync>();
            await syncService.DisableCloudSync();

            await settings.AddOrUpdateValueAsync(AppSettings.AppMigrationVersion, "3");
        }

        private async Task UpgradeAppFromV3ToV4()
        {
            var settings = Container.Resolve<ISettings>();

            // Migrate from refresh token to dropbox settings refresh token
            var syncMode = await settings.GetValueOrDefaultAsync(AppSettings.SyncMode);
            if (syncMode == SyncMode.Dropbox)
            {
                var dropBoxRefreshToken = await settings.GetValueOrDefaultAsync("RefreshToken");
                if (!string.IsNullOrEmpty(dropBoxRefreshToken))
                {
                    await settings.AddOrUpdateValueAsync(DropboxSettings.RefreshToken, dropBoxRefreshToken);
                    await settings.RemoveAsync("RefreshToken");
                }
            }

            try
            {
                // Cleanup old sync directory
                var appDataDirectory =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "BudgetBadger");
                var syncDirectory = Path.Combine(appDataDirectory, "sync");
                if (Directory.Exists(syncDirectory))
                {
                    Directory.Delete(syncDirectory, true);
                }
            }
            catch (Exception)
            {
                // Don't care if we can't cleanup the old directory
            }

            await settings.AddOrUpdateValueAsync(AppSettings.AppMigrationVersion, "4");
        }

        private static async Task CleanupTempDirectory()
        {
            try
            {
                var localFileSystem = new LocalFileSystem();
                var tempPath = Path.GetTempPath();
                foreach (var file in await localFileSystem.Directory.GetFilesAsync(tempPath))
                {
                    try
                    {
                        await localFileSystem.File.DeleteAsync(file);
                    }
                    catch (Exception)
                    {
                        // temp files, ignore if we can't delete.
                    }
                }

                foreach (var directory in await localFileSystem.Directory.GetDirectoriesAsync(tempPath))
                {
                    try
                    {
                        await localFileSystem.Directory.DeleteAsync(directory, true);
                    }
                    catch (Exception)
                    {
                        // temp files, ignore if we can't delete.
                    }
                }
            }
            catch (Exception)
            {
                // temp files, ignore if we can't delete.
            }
        }
    }
}
