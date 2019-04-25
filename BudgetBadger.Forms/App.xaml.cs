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
using SimpleAuth.Providers;
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
using Newtonsoft.Json;

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

        protected override async void OnInitialized()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("ODcxNzdAMzEzNzJlMzEyZTMwZGczRU9RMGhIQmlNbGU1WG1aUzc3L1duaHNORTA0TFprZ2xVZE10TXJvQT0=");
            InitializeComponent();

            SQLitePCL.Batteries_V2.Init();

            var localize = Container.Resolve<ILocalize>();
            var test = localize.GetDeviceCultureInfo();
            localize.SetLocale(test);

            if (Device.Idiom == TargetIdiom.Desktop)
            {
                await NavigationService.NavigateAsync("/MainPage/NavigationPage/EnvelopesPage");
            }
            else
            {
                await NavigationService.NavigateAsync("MainPage");
            }
        }

        protected async override void OnStart()
        {
            SetLocale();
            await VerifyPurchases();
            await SyncOnStartOrResume();
        }

        protected async override void OnResume()
        {
            SetLocale();
            //await VerifyPurchases();
            await SyncOnStartOrResume();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var timer = Stopwatch.StartNew();

            var container = containerRegistry.GetContainer();

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
            container.UseInstance(new SqliteConnection(defaultConnectionString), serviceKey: "defaultConnection");
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
            container.UseInstance(new SqliteConnection(syncConnectionString), serviceKey: "syncConnection");
            container.Register<IAccountDataAccess>(made: Made.Of(() => new AccountSqliteDataAccess(Arg.Of<string>("syncConnectionString"))),
                                                   serviceKey: "syncAccountDataAccess");
            container.Register<IPayeeDataAccess>(made: Made.Of(() => new PayeeSqliteDataAccess(Arg.Of<string>("syncConnectionString"))),
                                                 serviceKey: "syncPayeeDataAccess");
            container.Register<IEnvelopeDataAccess>(made: Made.Of(() => new EnvelopeSqliteDataAccess(Arg.Of<string>("syncConnectionString"))),
                                                    serviceKey: "syncEnvelopeDataAccess");
            container.Register<ITransactionDataAccess>(made: Made.Of(() => new TransactionSqliteDataAccess(Arg.Of<string>("syncConnectionString"))),
                                                       serviceKey: "syncTransactionDataAccess");

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

            container.Register(made: Made.Of(() => new DropBoxApi(
                Arg.Index<string>(0),
                Arg.Index<string>(1),
                Arg.Index<string>(2),
                Arg.Index<string>(3),
                null),
                _ => SyncMode.DropboxSync,
                _ => "pcg02fuyui9dru9",
                _ => "",
                _ => "budgetbadger://authorize"));

            container.Register<ISyncFactory>(made: Made.Of(() => new SyncFactory(Arg.Of<ISettings>(),
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


#if PRORELEASE || DEBUG
            container.Register<IPurchaseService, TrueInAppBillingPurchaseService>();
#else
            if (Device.RuntimePlatform == Device.macOS)
            {
                container.Register<IPurchaseService, FalseInAppBillingPurchaseService>();
            }
            else
            {
                container.UseInstance<IInAppBilling>(CrossInAppBilling.Current);
                container.Register<IPurchaseService, CachedInAppBillingPurchaseService>();
            }
#endif

            containerRegistry.RegisterForNavigationOnIdiom<MainPage, MainPageViewModel>(desktopView: typeof(MainDesktopPage), tabletView: typeof(MainTabletPage));

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigationOnIdiom<AccountsPage, AccountsPageViewModel>(desktopView: typeof(AccountsDetailedPage), tabletView: typeof(AccountsDetailedPage));
            containerRegistry.RegisterForNavigation<AccountSelectionPage, AccountSelectionPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<AccountInfoPage, AccountInfoPageViewModel>(desktopView: typeof(AccountInfoDetailedPage), tabletView: typeof(AccountInfoDetailedPage));
            containerRegistry.RegisterForNavigation<AccountEditPage, AccountEditPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<AccountReconcilePage, AccountReconcilePageViewModel>(desktopView: typeof(AccountReconcileDetailedPage), tabletView: typeof(AccountReconcileDetailedPage));
            containerRegistry.RegisterForNavigation<DeletedAccountsPage, DeletedAccountsPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<PayeesPage, PayeesPageViewModel>(desktopView: typeof(PayeesDetailedPage), tabletView: typeof(PayeesDetailedPage));
            containerRegistry.RegisterForNavigation<PayeeSelectionPage, PayeeSelectionPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<PayeeInfoPage, PayeeInfoPageViewModel>(desktopView: typeof(PayeeInfoDetailedPage), tabletView: typeof(PayeeInfoDetailedPage));
            containerRegistry.RegisterForNavigation<PayeeEditPage, PayeeEditPageViewModel>();
            containerRegistry.RegisterForNavigation<DeletedPayeesPage, DeletedPayeesPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<EnvelopesPage, EnvelopesPageViewModel>(desktopView: typeof(EnvelopesDetailedPage), tabletView: typeof(EnvelopesDetailedPage));
            containerRegistry.RegisterForNavigation<EnvelopeSelectionPage, EnvelopeSelectionPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<EnvelopeInfoPage, EnvelopeInfoPageViewModel>(desktopView: typeof(EnvelopeInfoDetailedPage), tabletView: typeof(EnvelopeInfoDetailedPage));
            containerRegistry.RegisterForNavigation<EnvelopeEditPage, EnvelopeEditPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeTransferPage, EnvelopeTransferPageViewModel>();
            containerRegistry.RegisterForNavigation<DeletedEnvelopesPage, DeletedEnvelopesPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeGroupSelectionPage, EnvelopeGroupSelectionPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeGroupEditPage, EnvelopeGroupEditPageViewModel>();
            containerRegistry.RegisterForNavigation<DeletedEnvelopeGroupsPage, DeletedEnvelopeGroupsPageViewModel>();
            containerRegistry.RegisterForNavigation<TransactionEditPage, TransactionEditPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<SplitTransactionPage, SplitTransactionPageViewModel>(desktopView: typeof(SplitTransactionDetailedPage), tabletView: typeof(SplitTransactionDetailedPage));
            containerRegistry.RegisterForNavigation<TransactionSelectionPage, TransactionSelectionPageViewModel>();
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
            containerRegistry.RegisterForNavigation<ReportsPage, ReportsPageViewModel>();
            containerRegistry.RegisterForNavigation<NetWorthReportPage, NetWorthReportPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopesSpendingReportPage, EnvelopesSpendingReportPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeTrendsReportPage, EnvelopeTrendsReportsPageViewModel>();
            containerRegistry.RegisterForNavigation<PayeesSpendingReportPage, PayeesSpendingReportPageViewModel>();
            containerRegistry.RegisterForNavigation<PayeeTrendsReportPage, PayeeTrendsReportPageViewModel>();

            timer.Stop();
        }

        void SetLocale()
        {
            var localize = Container.Resolve<ILocalize>();
            var settings = Container.Resolve<ISettings>();

            var currentCulture = (CultureInfo)localize.GetDeviceCultureInfo().Clone();

            var currentCurrencyFormat = settings.GetValueOrDefault(AppSettings.CurrencyFormat);
            if (!String.IsNullOrEmpty(currentCurrencyFormat) && currentCurrencyFormat != "Automatic")
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

            var currentDateFormat = settings.GetValueOrDefault(AppSettings.DateFormat);
            if (!String.IsNullOrEmpty(currentDateFormat) && currentDateFormat != "Automatic")
            {
                try
                {
                    var dateCulture = new CultureInfo(currentDateFormat);
                    currentCulture.DateTimeFormat = dateCulture.DateTimeFormat;
                }
                catch (Exception ex)
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

        async Task SyncOnStartOrResume()
        {
            var refreshResult = await RefreshSyncCredentials();

            if (refreshResult.Success)
            {
                var syncFactory = Container.Resolve<ISyncFactory>();
                var syncService = syncFactory.GetSyncService();
                await syncService.FullSync();
                await syncFactory.SetLastSyncDateTime(DateTime.Now);
            }
            else
            {
                var dialogService = Container.Resolve<IPageDialogService>();
                await dialogService.DisplayAlertAsync("Sync Setup Invalid", refreshResult.Message, "OK");
            }
        }

        async Task<Result> RefreshSyncCredentials()
        {
            //works on device, not on simulator right now
            return new Result { Success = true };

            //checking current filesyncprovider is valid
            var settings = Container.Resolve<ISettings>();
            var currentSyncMode = settings.GetValueOrDefault(AppSettings.SyncMode);

            if (currentSyncMode == SyncMode.DropboxSync)
            {
                var dropboxApi = Container.Resolve<DropBoxApi>();
                var account = await dropboxApi.Authenticate() as SimpleAuth.OAuthAccount;
                if (account.IsValid())
                {
                    await settings.AddOrUpdateValueAsync(DropboxSettings.AccessToken, account.Token);
                    return new Result { Success = true };
                }

                return new Result { Success = false, Message = "Could not validate sync credentials. Please try again." };
            }

            return new Result { Success = true };
        }
    }
}
