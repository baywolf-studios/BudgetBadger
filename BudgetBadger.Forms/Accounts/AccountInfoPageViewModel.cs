using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using Prism.Commands;
using Prism.Navigation;
using PropertyChanged;
using System.Collections.Generic;

namespace BudgetBadger.Forms.Accounts
{
    [AddINotifyPropertyChangedInterface]
    public class AccountInfoPageViewModel : INavigationAware
    {
        readonly ITransactionLogic TransactionLogic;
        readonly INavigationService NavigationService;
        readonly IAccountLogic AccountLogic;

        public ICommand EditCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NewTransactionCommand { get; set; }

        public bool IsBusy { get; set; }

        public Account Account { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
        public ILookup<string, Transaction> GroupedTransactions { get; set; }
        public Transaction SelectedTransaction { get; set; }

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
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(NavigationParameterType.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
            }

            await ExecuteRefreshCommand();
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public async Task ExecuteEditCommand()
        {
            var parameters = new NavigationParameters
            {
                { NavigationParameterType.Account, Account }
            };
            await NavigationService.NavigateAsync(NavigationPageName.AccountEditPage, parameters);
        }

        public async Task ExecuteTransactionSelectedCommand()
        {
            if (SelectedTransaction == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { NavigationParameterType.Transaction, SelectedTransaction }
            };
            await NavigationService.NavigateAsync(NavigationPageName.TransactionPage, parameters);

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
                { NavigationParameterType.Account, Account }
            };
            await NavigationService.NavigateAsync(NavigationPageName.TransactionPage, parameters);
        }
    }
}
