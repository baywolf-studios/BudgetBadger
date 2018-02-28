using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using System.Collections.Generic;
using Prism.Mvvm;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountInfoPageViewModel : BindableBase, INavigatingAware
    {
        readonly ITransactionLogic TransactionLogic;
        readonly INavigationService NavigationService;
        readonly IAccountLogic AccountLogic;

        public ICommand EditCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NewTransactionCommand { get; set; }
        public ICommand PaymentCommand { get; set; }

        bool _isBusy;
        public bool IsBusy 
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        Account _account;
        public Account Account
        {
            get => _account;
            set => SetProperty(ref _account, value);
        }

        IEnumerable<Transaction> _transactions;
        public IEnumerable<Transaction> Transactions
        {
            get => _transactions;
            set
            {
                SetProperty(ref _transactions, value);
                RaisePropertyChanged("PendingTotal");
                RaisePropertyChanged("PostedTotal");
                RaisePropertyChanged("TransactionsTotal");
            }
        }

        ILookup<string, Transaction> _groupedTransactions;
        public ILookup<string, Transaction> GroupedTransactions
        {
            get => _groupedTransactions;
            set => SetProperty(ref _groupedTransactions, value);
        }

        Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        public decimal PendingTotal { get => Transactions.Where(t => t.Pending).Sum(t2 => t2.Amount); }
        public decimal PostedTotal { get => Transactions.Where(t => t.Posted).Sum(t2 => t2.Amount); }
        public decimal TransactionsTotal { get => Transactions.Sum(t2 => t2.Amount); }

        public AccountInfoPageViewModel(INavigationService navigationService, ITransactionLogic transactionLogic, IAccountLogic accountLogic)
        {
            TransactionLogic = transactionLogic;
            NavigationService = navigationService;
            AccountLogic = accountLogic;

            Account = new Account();
            Transactions = new List<Transaction>();
            GroupedTransactions = Transactions.ToLookup(t => "");
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            TransactionSelectedCommand = new DelegateCommand(async () => await ExecuteTransactionSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            NewTransactionCommand = new DelegateCommand(async () => await ExecuteNewTransactionCommand());
            PaymentCommand = new DelegateCommand(async () => await ExecutePaymentCommand());
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
            }

            await ExecuteRefreshCommand();
        }

        public async Task ExecuteEditCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Account, Account }
            };
            await NavigationService.NavigateAsync(PageName.AccountEditPage, parameters);
        }

        public async Task ExecuteTransactionSelectedCommand()
        {
            if (SelectedTransaction == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Transaction, SelectedTransaction }
            };
            await NavigationService.NavigateAsync(PageName.TransactionPage, parameters);

            SelectedTransaction = null;
        }

        public async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                if (Account.Exists)
                {
                    var accountResult = await AccountLogic.GetAccountAsync(Account.Id);
                    if (accountResult.Success)
                    {
                        Account = accountResult.Data;
                    }
                    else
                    {
                        //show alert that account data may be stale
                    }

                    var result = await TransactionLogic.GetAccountTransactionsAsync(Account);
                    if (result.Success)
                    {
                        Transactions = result.Data;
                        GroupedTransactions = TransactionLogic.GroupTransactions(Transactions);
                        SelectedTransaction = null;
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteNewTransactionCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Account, Account }
            };
            await NavigationService.NavigateAsync(PageName.TransactionPage, parameters);
        }

        public async Task ExecutePaymentCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Account, Account },
                { PageParameter.TransactionAmount, Account.Payment }
            };
            await NavigationService.NavigateAsync(PageName.TransactionPage, parameters);
        }
    }
}
