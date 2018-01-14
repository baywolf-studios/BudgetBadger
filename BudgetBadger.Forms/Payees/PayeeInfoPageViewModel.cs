using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using Prism.Commands;
using Prism.Navigation;
using PropertyChanged;

namespace BudgetBadger.Forms.Payees
{
    [AddINotifyPropertyChangedInterface]
    public class PayeeInfoPageViewModel : INavigationAware
    {
        readonly ITransactionLogic TransactionLogic;
        readonly INavigationService NavigationService;
        readonly IPayeeLogic PayeeLogic;

        public ICommand EditCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NewTransactionCommand { get; set; }

        public bool IsBusy { get; set; }

        public Payee Payee { get; set; }
        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<GroupedList<Transaction>> GroupedTransactions { get; set; }
        public Transaction SelectedTransaction { get; set; }

        public decimal LifetimeSpent { get => Transactions.Sum(t => t.Amount); }

        public PayeeInfoPageViewModel(INavigationService navigationService, ITransactionLogic transactionLogic, IPayeeLogic payeeLogic)
        {
            TransactionLogic = transactionLogic;
            NavigationService = navigationService;
            PayeeLogic = payeeLogic;

            Payee = new Payee();
            Transactions = new ObservableCollection<Transaction>();
            GroupedTransactions = new ObservableCollection<GroupedList<Transaction>>();
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            TransactionSelectedCommand = new DelegateCommand(async () => await ExecuteTransactionSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            NewTransactionCommand = new DelegateCommand(async () => await ExecuteNewTransactionCommand());
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            var payee = parameters.GetValue<Payee>(NavigationParameterType.Payee);
            if (payee != null)
            {
                Payee = payee.DeepCopy();
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
                { NavigationParameterType.Payee, Payee }
            };
            await NavigationService.NavigateAsync(NavigationPageName.PayeeEditPage, parameters);
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
                if (Payee.Exists)
                {
                    var payeeResult = await PayeeLogic.GetPayeeAsync(Payee.Id);
                    if (payeeResult.Success)
                    {
                        Payee = payeeResult.Data;
                    }
                    else
                    {
                        //show alert that account data may be stale
                    }

                    var result = await TransactionLogic.GetPayeeTransactionsAsync(Payee);
                    if (result.Success)
                    {
                        Transactions = new ObservableCollection<Transaction>(result.Data);
                        GroupedTransactions = new ObservableCollection<GroupedList<Transaction>>(TransactionLogic.GroupTransactions(Transactions));
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
                { NavigationParameterType.Payee, Payee }
            };
            await NavigationService.NavigateAsync(NavigationPageName.TransactionPage, parameters);
        }
    }
}
