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
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using BudgetBadger.Forms.Accounts;
using BudgetBadger.Forms.Envelopes;
using BudgetBadger.Forms.Transactions;
using BudgetBadger.Core.Sync;
using BudgetBadger.Core;
using BudgetBadger.Core.Files;

namespace BudgetBadger.Forms
{
    public partial class App : PrismApplication
    {
        protected override void OnInitialized()
        {
            InitializeComponent();

            SQLitePCL.Batteries_V2.Init();

            NavigationService.NavigateAsync("MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var timer = Stopwatch.StartNew();

            var localAppDirectory = new LocalDirectoryInfo(FileLocator.GetLocalPath());
            var localDatabase = new LocalFileInfo("databse.bb", localAppDirectory);
            var localAccountDataAccess = new AccountSqliteDataAccess(localDatabase);
            var localPayeeDataAccess = new PayeeSqliteDataAccess(localDatabase);
            var localEnvelopeDatAccess = new EnvelopeSqliteDataAccess(localDatabase);
            var localTransactionDataAccess = new TransactionSqliteDataAccess(localDatabase);

            var syncAppDirectory = new LocalDirectoryInfo(FileLocator.GetSyncPath());
            var syncDatabase = new LocalFileInfo("databse.bb", syncAppDirectory);
            var syncAccountDataAccess = new AccountSqliteDataAccess(syncDatabase);
            var syncPayeeDataAccess = new PayeeSqliteDataAccess(syncDatabase);
            var syncEnvelopeDatAccess = new EnvelopeSqliteDataAccess(syncDatabase);
            var syncTransactionDataAccess = new TransactionSqliteDataAccess(syncDatabase);

            containerRegistry.RegisterInstance<IAccountDataAccess>(localAccountDataAccess);
            containerRegistry.RegisterInstance<IPayeeDataAccess>(localPayeeDataAccess);
            containerRegistry.RegisterInstance<IEnvelopeDataAccess>(localEnvelopeDatAccess);
            containerRegistry.RegisterInstance<ITransactionDataAccess>(localTransactionDataAccess);

            containerRegistry.Register<ITransactionLogic, TransactionLogic>();
            containerRegistry.Register<IAccountLogic, AccountLogic>();
            containerRegistry.Register<IPayeeLogic, PayeeLogic>();
            containerRegistry.Register<IEnvelopeLogic, EnvelopeLogic>();

            var fileSyncProvider = new LocalFileSyncProvider(@"/Users/matthewpritchett/BudgetBadger");
            containerRegistry.RegisterInstance<IFileSyncProvider>(fileSyncProvider);

            var fileSync = new FileSync(syncAppDirectory,
                                       fileSyncProvider,
                                       localAccountDataAccess,
                                       syncAccountDataAccess,
                                       localPayeeDataAccess,
                                       syncPayeeDataAccess,
                                       localEnvelopeDatAccess,
                                       syncEnvelopeDatAccess,
                                       localTransactionDataAccess,
                                        syncTransactionDataAccess);
            containerRegistry.RegisterInstance<ISync>(fileSync);

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
            timer.Stop();
        }
    }
}
