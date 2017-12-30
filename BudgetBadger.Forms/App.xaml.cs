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
            containerRegistry.Register<IFileLocator, FileLocator>();
            containerRegistry.RegisterInstance<ITransactionDataAccess>(new TransactionSqliteDataAccess(@"Filename=/Users/matthewpritchett/BudgetBadger/database.bb"));
            containerRegistry.RegisterInstance<IAccountDataAccess>(new AccountSqliteDataAccess(@"Filename=/Users/matthewpritchett/BudgetBadger/database.bb"));
            containerRegistry.RegisterInstance<IPayeeDataAccess>(new PayeeSqliteDataAccess(@"Filename=/Users/matthewpritchett/BudgetBadger/database.bb"));
            containerRegistry.RegisterInstance<IEnvelopeDataAccess>(new EnvelopeSqliteDataAccess(@"Filename=/Users/matthewpritchett/BudgetBadger/database.bb"));
            containerRegistry.Register<ITransactionLogic, TransactionLogic>();
            containerRegistry.Register<IAccountLogic, AccountLogic>();
            containerRegistry.Register<IPayeeLogic, PayeeLogic>();
            containerRegistry.Register<IEnvelopeLogic, EnvelopeLogic>(); 
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
