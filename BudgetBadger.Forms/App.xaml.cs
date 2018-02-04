using System;
using System.Collections.Generic;
using System.Diagnostics;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.FileLocator;
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
using System.IO;
using BudgetBadger.Core.Sync;

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

            var localApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BudgetBadger");
            if (!Directory.Exists(localApplicationDataPath))
            {
                Directory.CreateDirectory(localApplicationDataPath);
            }

            var localPath = Path.Combine(localApplicationDataPath, "local");
            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
            var localDatabase = Path.Combine(localPath, "database.bb");

            var syncPath = Path.Combine(localApplicationDataPath, "sync");
            if (!Directory.Exists(syncPath))
            {
                Directory.CreateDirectory(syncPath);
            }
            var syncDatabase = Path.Combine(syncPath, "database.bb");

            var localAccountDataAccess = new AccountSqliteDataAccess(localDatabase);
            var localPayeeDataAccess = new PayeeSqliteDataAccess(localDatabase);
            var localEnvelopeDatAccess = new EnvelopeSqliteDataAccess(localDatabase);
            var localTransactionDataAccess = new TransactionSqliteDataAccess(localDatabase);

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
            var syncLogic = new SyncLogic(
                localAccountDataAccess,
                syncAccountDataAccess,
                localPayeeDataAccess,
                syncPayeeDataAccess,
                localEnvelopeDatAccess,
                syncEnvelopeDatAccess,
                localTransactionDataAccess,
                syncTransactionDataAccess);
            containerRegistry.RegisterInstance<ISyncLogic>(syncLogic);

            var fileSyncProvider = new LocalFileSyncProvider(@"/Users/matthewpritchett/BudgetBadger", syncPath);
            containerRegistry.RegisterInstance<IFileSyncProvider>(fileSyncProvider);
            containerRegistry.Register<ISync, FileSync>();

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
