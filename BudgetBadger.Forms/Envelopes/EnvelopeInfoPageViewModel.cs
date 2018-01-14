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

namespace BudgetBadger.Forms.Envelopes
{
    [AddINotifyPropertyChangedInterface]
    public class EnvelopeInfoPageViewModel : INavigationAware
    {
        readonly ITransactionLogic TransactionLogic;
        readonly INavigationService NavigationService;
        readonly IEnvelopeLogic EnvelopeLogic;

        public ICommand NewTransactionCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public bool IsBusy { get; set; }

        public Budget Budget { get; set; }
        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<GroupedList<Transaction>> GroupedTransactions { get; set; }
        public Transaction SelectedTransaction { get; set; }

        public EnvelopeInfoPageViewModel(INavigationService navigationService, ITransactionLogic transactionLogic, IEnvelopeLogic envelopeLogic)
        {
            TransactionLogic = transactionLogic;
            NavigationService = navigationService;
            EnvelopeLogic = envelopeLogic;

            Budget = new Budget();
            Transactions = new ObservableCollection<Transaction>();
            GroupedTransactions = new ObservableCollection<GroupedList<Transaction>>();
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            TransactionSelectedCommand = new DelegateCommand(async () => await ExecuteTransactionSelectedCommand());
            NewTransactionCommand = new DelegateCommand(async () => await ExecuteNewTransactionCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            var budget = parameters.GetValue<Budget>(NavigationParameterType.Budget);
            if (budget != null)
            {
                Budget = budget.DeepCopy();
            }

            await ExecuteRefreshCommand();
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public async Task ExecuteEditCommand()
        {
            var parameters = new NavigationParameters
            {
                { NavigationParameterType.Budget, Budget }
            };
            await NavigationService.NavigateAsync(NavigationPageName.EnvelopeEditPage, parameters);
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

        public async Task ExecuteNewTransactionCommand()
        {
            var parameters = new NavigationParameters
            {
                { NavigationParameterType.Envelope, Budget.Envelope }
            };
            await NavigationService.NavigateAsync(NavigationPageName.TransactionPage, parameters);
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
                if (Budget.Exists)
                {
                    var budgetResult = await EnvelopeLogic.GetBudgetAsync(Budget.Id);
                    if (budgetResult.Success)
                    {
                        Budget = budgetResult.Data;
                    }

                    var result = await TransactionLogic.GetEnvelopeTransactionsAsync(Budget.Envelope);
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
    }
}
