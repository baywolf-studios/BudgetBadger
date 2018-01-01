using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using BudgetBadger.Forms.ViewModels;
using Prism.Commands;
using Prism.Navigation;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountInfoPageViewModel : BaseViewModel, INavigationAware
    {
        readonly ITransactionLogic TransactionLogic;
        readonly INavigationService NavigationService;

        public ICommand EditCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NewTransactionCommand { get; set; }

        public Account Account { get; set; }
        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<GroupedList<Transaction>> GroupedTransactions { get; set; }
        public Transaction SelectedTransaction { get; set; }

        public decimal PendingTotal { get => Transactions.Where(t => t.Pending).Sum(t2 => t2.Amount); }
        public decimal PostedTotal { get => Transactions.Where(t => t.Posted).Sum(t2 => t2.Amount); }
        public decimal TransactionsTotal { get => Transactions.Sum(t2 => t2.Amount); }

        public AccountInfoPageViewModel(INavigationService navigationService, ITransactionLogic transactionLogic)
        {
            Title = "Account Overview";
            TransactionLogic = transactionLogic;
            NavigationService = navigationService;

            Account = new Account();
            Transactions = new ObservableCollection<Transaction>();
            GroupedTransactions = new ObservableCollection<GroupedList<Transaction>>();
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            TransactionSelectedCommand = new DelegateCommand(async () => await ExecuteTransactionSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            NewTransactionCommand = new DelegateCommand(async () => await ExecuteNewTransactionCommand());
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
                var result = await TransactionLogic.GetAccountTransactionsAsync(Account);
                if (result.Success)
                {
                    Transactions = new ObservableCollection<Transaction>(result.Data);
                    GroupedTransactions = new ObservableCollection<GroupedList<Transaction>>(TransactionLogic.GroupTransactions(Transactions));
                    SelectedTransaction = null;
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

        public override async void OnNavigatingTo(NavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(NavigationParameterType.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
            }

            await ExecuteRefreshCommand();
        }
    }
}
