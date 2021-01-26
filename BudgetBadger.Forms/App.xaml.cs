using System;
using System.Collections.Generic;
using System.Diagnostics;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.Logic;
using BudgetBadger.Forms.Payees;
using BudgetBadger.Forms.Views;
using BudgetBadger.Forms.ViewModels;
using DryIoc;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using BudgetBadger.Forms.Accounts;
using BudgetBadger.Forms.Envelopes;
using BudgetBadger.Forms.Transactions;
using BudgetBadger.Core.Sync;
using BudgetBadger.Core.Files;
using BudgetBadger.FileSyncProvider.Dropbox;
using BudgetBadger.Forms.Settings;
using BudgetBadger.Core.Settings;
using Prism.AppModel;
using BudgetBadger.Forms.Enums;
using System.IO;
using Prism.Services;
using BudgetBadger.Forms.Reports;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;
using BudgetBadger.Models;
using BudgetBadger.Core.Purchase;
using BudgetBadger.Forms.Purchase;
using Plugin.InAppBilling.Abstractions;
using Plugin.InAppBilling;
using Microsoft.Data.Sqlite;
using BudgetBadger.Core.LocalizedResources;
using System.Globalization;
using BudgetBadger.Core.Utilities;
using Prism.Logging;
using Prism.Navigation;
using Prism.Events;
using BudgetBadger.Forms.Events;
using System.Threading;
using BudgetBadger.Forms.UserControls;
using System.Linq;
using BudgetBadger.Forms.Style;
using BudgetBadger.Forms.Authentication;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace BudgetBadger.Forms
{
    public partial class App : PrismApplication
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        private Timer _syncTimer;

        protected override async void OnInitialized()
        {
            InitializeComponent();

            SQLitePCL.Batteries_V2.Init();

            var settings = Container.Resolve<ISettings>();
            int.TryParse(settings.GetValueOrDefault(AppSettings.AppOpenedCount), out int appOpenedCount);
            if (appOpenedCount > 0)
            {
                if (Device.Idiom == TargetIdiom.Desktop)
                {
                    var parameters = new NavigationParameters
                    {
                        { PageParameter.PageName, "EnvelopesPage" }
                    };
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
                    var parameters = new NavigationParameters
                    {
                        { PageParameter.PageName, "AccountsPage" }
                    };
                    await NavigationService.NavigateAsync("/MainPage/NavigationPage/AccountsPage", parameters);
                }
                else
                {
                    await NavigationService.NavigateAsync("MainPage?selectedTab=AccountsPage");
                }
            }

            _syncTimer = new Timer(_ => Task.Run(async () => await Sync()).FireAndForget());

            SetAppTheme(Application.Current.RequestedTheme);
            SetAppearanceSize();
            SetLocale();

            Application.Current.RequestedThemeChanged += (s, a) => 
            {
                SetAppTheme(a.RequestedTheme);
            };

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

            eventAggregator.GetEvent<SplitTransactionSavedEvent>().Subscribe(ResetSyncTimer);
            eventAggregator.GetEvent<SplitTransactionStatusUpdatedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(x => ResetSyncTimer());
            eventAggregator.GetEvent<TransactionStatusUpdatedEvent>().Subscribe(x => ResetSyncTimer());
        }

        protected async override void OnStart()
        {
            // tracking number of times app opened
            var settings = Container.Resolve<ISettings>();
            int.TryParse(settings.GetValueOrDefault(AppSettings.AppOpenedCount), out int appOpenedCount);
            appOpenedCount++;
            await settings.AddOrUpdateValueAsync(AppSettings.AppOpenedCount, appOpenedCount.ToString());

            await VerifyPurchases();
            ResetSyncTimerAtStartOrResume();
        }

        protected override void OnResume()
        {
            ResetSyncTimerAtStartOrResume();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var timer = Stopwatch.StartNew();

            var container = containerRegistry.GetContainer();

#if DEBUG
            container.Register<ILoggerFacade, ConsoleLogger>();
#endif
            container.Register<IApplicationStore, ApplicationStore>();
            container.Register<ISettings, AppStoreSettings>();
            container.Register<IPurchaseService, CachedInAppBillingPurchaseService>();
            container.Register<IResourceContainer, ResourceContainer>();

            var appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BudgetBadger");
            Directory.CreateDirectory(appDataDirectory);

            var dataDirectory = Path.Combine(appDataDirectory, "data");
            Directory.CreateDirectory(dataDirectory);
            var syncDirectory = Path.Combine(appDataDirectory, "sync");
            if (Directory.Exists(syncDirectory))
            {
                Directory.Delete(syncDirectory, true);
            }
            Directory.CreateDirectory(syncDirectory);

            //default dataaccess
            var defaultConnectionString = "Data Source=" + Path.Combine(dataDirectory, "default.bb");
            container.UseInstance(defaultConnectionString, serviceKey: "defaultConnectionString");
            container.Register<IAccountDataAccess>(made: Made.Of(() => new AccountSqliteDataAccess(Arg.Of<string>("defaultConnectionString"))));
            container.Register<IPayeeDataAccess>(made: Made.Of(() => new PayeeSqliteDataAccess(Arg.Of<string>("defaultConnectionString"))));
            container.Register<IEnvelopeDataAccess>(made: Made.Of(() => new EnvelopeSqliteDataAccess(Arg.Of<string>("defaultConnectionString"))));
            container.Register<ITransactionDataAccess>(made: Made.Of(() => new TransactionSqliteDataAccess(Arg.Of<string>("defaultConnectionString"))));

            //default logic
            container.Register<ITransactionLogic, TransactionLogic>();
            container.Register<IAccountLogic, AccountLogic>();
            container.Register<IPayeeLogic, PayeeLogic>();
            container.Register<IEnvelopeLogic, EnvelopeLogic>();
            container.Register<IReportLogic, ReportLogic>();

            //sync dataaccess
            var syncConnectionString = "Data Source=" + Path.Combine(syncDirectory, "default.bb");
            container.UseInstance(syncConnectionString, serviceKey: "syncConnectionString");
            container.Register<IAccountDataAccess>(made: Made.Of(() => new AccountSqliteDataAccess(Arg.Of<string>("syncConnectionString"))), serviceKey: "syncAccountDataAccess");
            container.Register<IPayeeDataAccess>(made: Made.Of(() => new PayeeSqliteDataAccess(Arg.Of<string>("syncConnectionString"))), serviceKey: "syncPayeeDataAccess");
            container.Register<IEnvelopeDataAccess>(made: Made.Of(() => new EnvelopeSqliteDataAccess(Arg.Of<string>("syncConnectionString"))), serviceKey: "syncEnvelopeDataAccess");
            container.Register<ITransactionDataAccess>(made: Made.Of(() => new TransactionSqliteDataAccess(Arg.Of<string>("syncConnectionString"))), serviceKey: "syncTransactionDataAccess");

            //sync directory for filesyncproviders
            container.Register<IDirectoryInfo>(made: Made.Of(() => new LocalDirectoryInfo(syncDirectory)));

            //sync logics
            container.Register<IAccountSyncLogic>(made: Made.Of(() => new AccountSyncLogic(Arg.Of<IAccountDataAccess>(),
                                                                                           Arg.Of<IAccountDataAccess>("syncAccountDataAccess"))));
            container.Register<IPayeeSyncLogic>(made: Made.Of(() => new PayeeSyncLogic(Arg.Of<IPayeeDataAccess>(),
                                                                                       Arg.Of<IPayeeDataAccess>("syncPayeeDataAccess"))));
            container.Register<IEnvelopeSyncLogic>(made: Made.Of(() => new EnvelopeSyncLogic(Arg.Of<IEnvelopeDataAccess>(),
                                                                                             Arg.Of<IEnvelopeDataAccess>("syncEnvelopeDataAccess"))));
            container.Register<ITransactionSyncLogic>(made: Made.Of(() => new TransactionSyncLogic(Arg.Of<ITransactionDataAccess>(),
                                                                                                   Arg.Of<ITransactionDataAccess>("syncTransactionDataAccess"))));

            container.Register<IFileSyncProvider, DropboxFileSyncProvider>(serviceKey: SyncMode.DropboxSync);



            container.Register<ISyncFactory>(made: Made.Of(() => new SyncFactory(Arg.Of<IResourceContainer>(),
                                                                          Arg.Of<ISettings>(),
                                                                          Arg.Of<IDirectoryInfo>(),
                                                                          Arg.Of<IAccountSyncLogic>(),
                                                                          Arg.Of<IPayeeSyncLogic>(),
                                                                          Arg.Of<IEnvelopeSyncLogic>(),
                                                                          Arg.Of<ITransactionSyncLogic>(),
                                                                          Arg.Of<KeyValuePair<string, IFileSyncProvider>[]>())));

            container.Register(made: Made.Of(() => StaticSyncFactory.CreateSync(Arg.Of<ISettings>(),
                                                                          Arg.Of<IDirectoryInfo>(),
                                                                          Arg.Of<IAccountSyncLogic>(),
                                                                          Arg.Of<IPayeeSyncLogic>(),
                                                                          Arg.Of<IEnvelopeSyncLogic>(),
                                                                          Arg.Of<ITransactionSyncLogic>(),
                                                                          Arg.Of<KeyValuePair<string, IFileSyncProvider>[]>())));


#if DEBUG
            container.Register<IPurchaseService, TrueInAppBillingPurchaseService>();
#else
            if (Device.RuntimePlatform == Device.macOS)
            {
                container.Register<IPurchaseService, CachedInAppBillingPurchaseService>();
            }
            else
            {
                container.UseInstance<IInAppBilling>(CrossInAppBilling.Current);
                container.Register<IPurchaseService, CachedInAppBillingPurchaseService>();
            }
#endif

            containerRegistry.RegisterForNavigationOnIdiom<MainPage, MainPageViewModel>(desktopView: typeof(MainDesktopPage), tabletView: typeof(MainTabletPage));

            containerRegistry.RegisterForNavigation<MyPage>("NavigationPage");
            containerRegistry.RegisterForNavigationOnIdiom<AccountsPage, AccountsPageViewModel>(desktopView: typeof(AccountsDetailedPage), tabletView: typeof(AccountsDetailedPage));
            containerRegistry.RegisterForNavigation<AccountSelectionPage, AccountSelectionPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<AccountInfoPage, AccountInfoPageViewModel>(desktopView: typeof(AccountInfoDetailedPage), tabletView: typeof(AccountInfoDetailedPage));
            containerRegistry.RegisterForNavigation<AccountEditPage, AccountEditPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<AccountReconcilePage, AccountReconcilePageViewModel>(desktopView: typeof(AccountReconcileDetailedPage), tabletView: typeof(AccountReconcileDetailedPage));
            containerRegistry.RegisterForNavigation<HiddenAccountsPage, HiddenAccountsPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<PayeesPage, PayeesPageViewModel>(desktopView: typeof(PayeesDetailedPage), tabletView: typeof(PayeesDetailedPage));
            containerRegistry.RegisterForNavigation<PayeeSelectionPage, PayeeSelectionPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<PayeeInfoPage, PayeeInfoPageViewModel>(desktopView: typeof(PayeeInfoDetailedPage), tabletView: typeof(PayeeInfoDetailedPage));
            containerRegistry.RegisterForNavigation<PayeeEditPage, PayeeEditPageViewModel>();
            containerRegistry.RegisterForNavigation<HiddenPayeesPage, HiddenPayeesPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<EnvelopesPage, EnvelopesPageViewModel>(desktopView: typeof(EnvelopesDetailedPage), tabletView: typeof(EnvelopesDetailedPage));
            containerRegistry.RegisterForNavigation<EnvelopeSelectionPage, EnvelopeSelectionPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<EnvelopeInfoPage, EnvelopeInfoPageViewModel>(desktopView: typeof(EnvelopeInfoDetailedPage), tabletView: typeof(EnvelopeInfoDetailedPage));
            containerRegistry.RegisterForNavigation<EnvelopeEditPage, EnvelopeEditPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeTransferPage, EnvelopeTransferPageViewModel>();
            containerRegistry.RegisterForNavigation<HiddenEnvelopesPage, HiddenEnvelopesPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<EnvelopeGroupsPage, EnvelopeGroupsPageViewModel>(desktopView: typeof(EnvelopeGroupsDetailedPage), tabletView: typeof(EnvelopeGroupsDetailedPage));
            containerRegistry.RegisterForNavigation<EnvelopeGroupSelectionPage, EnvelopeGroupSelectionPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeGroupEditPage, EnvelopeGroupEditPageViewModel>();
            containerRegistry.RegisterForNavigation<HiddenEnvelopeGroupsPage, HiddenEnvelopeGroupsPageViewModel>();
            containerRegistry.RegisterForNavigation<TransactionEditPage, TransactionEditPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<SplitTransactionPage, SplitTransactionPageViewModel>(desktopView: typeof(SplitTransactionDetailedPage), tabletView: typeof(SplitTransactionDetailedPage));
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
            containerRegistry.RegisterForNavigation<ReportsPage, ReportsPageViewModel>();
            containerRegistry.RegisterForNavigation<NetWorthReportPage, NetWorthReportPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopesSpendingReportPage, EnvelopesSpendingReportPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeTrendsReportPage, EnvelopeTrendsReportsPageViewModel>();
            containerRegistry.RegisterForNavigation<PayeesSpendingReportPage, PayeesSpendingReportPageViewModel>();
            containerRegistry.RegisterForNavigation<PayeeTrendsReportPage, PayeeTrendsReportPageViewModel>();

            timer.Stop();
        }

        void SetAppTheme(OSAppTheme oSAppTheme)
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                var dictsToRemove = mergedDictionaries.Where(m => (m is LightThemeResources)
                                                               || (m is DarkThemeResources)).ToList();

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

        void SetAppearanceSize()
        {
            var settings = Container.Resolve<ISettings>();

            var currentDimension = settings.GetValueOrDefault(AppSettings.AppearanceDimensionSize);

            if (Enum.TryParse(currentDimension, out DimensionSize selectedDimensionSize))
            {
                ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                if (mergedDictionaries != null)
                {
                    var dictsToRemove = mergedDictionaries.Where(m => (m is LargeDimensionResources)
                                                                   || (m is MediumDimensionResources)
                                                                   || (m is SmallDimensionResources)).ToList();

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


        void SetLocale()
        {
            var localize = Container.Resolve<ILocalize>();
            var settings = Container.Resolve<ISettings>();

            var currentLanguage = settings.GetValueOrDefault(AppSettings.Language);
            CultureInfo currentCulture = null;

            if (!String.IsNullOrEmpty(currentLanguage))
            {
                try
                {

                    currentCulture = new CultureInfo(currentLanguage);
                }
                catch (Exception ex)
                {
                    // culture doesn't exist
                }
            }

            if (currentCulture == null)
            {
                currentCulture = (CultureInfo)localize.GetDeviceCultureInfo().Clone();
            }
            
            
            var currentCurrencyFormat = settings.GetValueOrDefault(AppSettings.CurrencyFormat);
            if (!String.IsNullOrEmpty(currentCurrencyFormat))
            {
                try
                {
                    var currencyCulture = new CultureInfo(currentCurrencyFormat);
                    currentCulture.NumberFormat = currencyCulture.NumberFormat;
                }
                catch(Exception ex)
                {
                    // culture doesn't exist
                }
            }

            localize.SetLocale(currentCulture);
        }

        async Task VerifyPurchases()
        {
            var purchaseService = Container.Resolve<IPurchaseService>();

            var proResult = await purchaseService.VerifyPurchaseAsync(Purchases.Pro);

            if (!proResult.Success)
            {
                var settings = Container.Resolve<ISettings>();
                await settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.NoSync);
            }
        }

        private void ResetSyncTimerAtStartOrResume()
        {
            _syncTimer.Change(TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(-1));
        }

        private void ResetSyncTimer()
        {
            _syncTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(-1));
        }

        async Task Sync()
        {
            var syncFactory = Container.Resolve<ISyncFactory>();
            var syncService = syncFactory.GetSyncService();
            var syncResult = await syncService.FullSync();
            if (syncResult.Success)
            {
                await syncFactory.SetLastSyncDateTime(DateTime.Now);
            }
        }
    }
}
