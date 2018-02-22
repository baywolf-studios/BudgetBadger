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
using BudgetBadger.Core;
using BudgetBadger.Core.Files;
using BudgetBadger.FileSyncProvider.Dropbox;
using BudgetBadger.Forms.Settings;
using BudgetBadger.Core.Settings;
using BudgetBadger.Forms.Sync;
using Prism.AppModel;
using BudgetBadger.Forms.Enums;
using System.IO;
using SimpleAuth.Providers;
using System.Linq;
using Prism.Services;
using System.Threading.Tasks;
using SimpleAuth;
using BudgetBadger.Models;

namespace BudgetBadger.Forms
{
    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer initializer = null) : base(initializer) { }

        protected async override void OnStart()
        {
            var refreshResult = await RefreshSyncCredentials();

            if (refreshResult.Success)
            {
                var verifyResult = await VerifySyncValidation();

                if (verifyResult.Success)
                {
                    var syncService = Container.Resolve<ISync>();
                    await syncService.FullSync();
                }
                else
                {
                    var dialogService = Container.Resolve<IPageDialogService>();
                    await dialogService.DisplayAlertAsync("Sync Setup Invalid", verifyResult.Message, "OK");
                }
            }
            else
            {
                var dialogService = Container.Resolve<IPageDialogService>();
                await dialogService.DisplayAlertAsync("Sync Setup Invalid", refreshResult.Message, "OK");
            }
            // refreshing the tokens and stuffs.
            //var settings = Container.Resolve<ISettings>();
            //if (settings.GetValueOrDefault(SettingsKeys.SyncMode) == SyncMode.DropboxSync)
            //{
            //    var dropboxApi = Container.Resolve<DropBoxApi>();
            //    try
            //    {
            //        var account = await dropboxApi.Authenticate();
            //    }
            //    catch (Exception ex)
            //    {
            //        var test = ex.Message;
            //    }
            //}
        }

        protected async override void OnResume()
        {
            await VerifySyncValidation();
        }

        protected async override void OnInitialized()
        {
            InitializeComponent();

            SQLitePCL.Batteries_V2.Init();

            await NavigationService.NavigateAsync("MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var timer = Stopwatch.StartNew();

            var container = containerRegistry.GetContainer();


            container.Register<IApplicationStore, ApplicationStore>();
            container.Register<ISettings, AppStoreSettings>();

            var appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BudgetBader");

            var dataDirectory = Path.Combine(appDataDirectory, "data");
            Directory.CreateDirectory(dataDirectory);
            var syncDirectory = Path.Combine(appDataDirectory, "sync");
            Directory.CreateDirectory(syncDirectory);

            var defaultConnectionString = "Data Source=" + Path.Combine(dataDirectory, "default.bb");
            var syncConnectionString = "Data Source=" + Path.Combine(syncDirectory, "default.bb");

            //default dataaccess
            container.RegisterInstance(defaultConnectionString, serviceKey: "defaultConnectionString");
            container.Register<IAccountDataAccess>(made: Made.Of(() => new AccountSqliteDataAccess(Arg.Of<string>("defaultConnectionString"))));
            container.Register<IPayeeDataAccess>(made: Made.Of(() => new PayeeSqliteDataAccess(Arg.Of<string>("defaultConnectionString"))));
            container.Register<IEnvelopeDataAccess>(made: Made.Of(() => new EnvelopeSqliteDataAccess(Arg.Of<string>("defaultConnectionString"))));
            container.Register<ITransactionDataAccess>(made: Made.Of(() => new TransactionSqliteDataAccess(Arg.Of<string>("defaultConnectionString"))));

            //default logic
            container.Register<ITransactionLogic, TransactionLogic>();
            container.Register<IAccountLogic, AccountLogic>();
            container.Register<IPayeeLogic, PayeeLogic>();
            container.Register<IEnvelopeLogic, EnvelopeLogic>();

            //sync dataaccess
            container.RegisterInstance(syncConnectionString, serviceKey: "syncConnectionString");
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
                                                         _ => "***REMOVED***",
                                                         _ => "",
                                                         _ => "budgetbadger://authorize"));

            container.Register(made: Made.Of(() => SyncFactory.CreateSync(Arg.Of<ISettings>(),
                                                                          Arg.Of<IDirectoryInfo>(),
                                                                          Arg.Of<IAccountSyncLogic>(),
                                                                          Arg.Of<IPayeeSyncLogic>(),
                                                                          Arg.Of<IEnvelopeSyncLogic>(),
                                                                          Arg.Of<ITransactionSyncLogic>(),
                                                                          Arg.Of<KeyValuePair<string, IFileSyncProvider>[]>())));

            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>("MainPage");
            containerRegistry.RegisterForNavigation<AccountsPage, AccountsPageViewModel>();
            containerRegistry.RegisterForNavigation<AccountInfoPage, AccountInfoPageViewModel>();
            containerRegistry.RegisterForNavigation<AccountEditPage, AccountEditPageViewModel>();
            containerRegistry.RegisterForNavigation<PayeesPage, PayeesPageViewModel>();
            containerRegistry.RegisterForNavigation<PayeeInfoPage, PayeeInfoPageViewModel>();
            containerRegistry.RegisterForNavigation<PayeeEditPage, PayeeEditPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopesPage, EnvelopesPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeInfoPage, EnvelopeInfoPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeEditPage, EnvelopeEditPageViewModel>();
            containerRegistry.RegisterForNavigation<EnvelopeGroupsPage, EnvelopeGroupsPageViewModel>();
            containerRegistry.RegisterForNavigation<TransactionPage, TransactionPageViewModel>();
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
            containerRegistry.RegisterForNavigation<SyncPage, SyncPageViewModel>();
            containerRegistry.RegisterForNavigation<SyncModesPage, SyncModesPageViewModel>();

            timer.Stop();
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
                var account = await dropboxApi.Authenticate() as OAuthAccount;
                if (account.IsValid())
                {
                    await settings.AddOrUpdateValueAsync(DropboxSettings.AccessToken, account.Token);
                }
                else
                {
                    var dialogs = Container.Resolve<IPageDialogService>();
                    await dialogs.DisplayAlertAsync("Sync Unsucessful", "Could not validate credentials. Please try again.", "OK");
                }
            }
        }

        async Task<Result> VerifySyncValidation()
        {
            //checking current filesyncprovider is valid
            var settings = Container.Resolve<ISettings>();
            var currentSyncMode = settings.GetValueOrDefault(AppSettings.SyncMode);

            if (!string.IsNullOrEmpty(currentSyncMode) && currentSyncMode != SyncMode.NoSync)
            {
                var providers = Container.Resolve<KeyValuePair<string, IFileSyncProvider>[]>();
                var currentProvider = providers.FirstOrDefault(p => p.Key == currentSyncMode).Value;

                if (currentProvider != null)
                {
                    return await currentProvider.IsValid();
                }
                else
                {
                    return new Result { Success = false, Message = "Unkown error" }; 
                }
            }

            return new Result { Success = true };

        }
    }
}
