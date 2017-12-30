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

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeInfoPageViewModel : BaseViewModel, INavigationAware
    {
        readonly ITransactionLogic TransactionLogic;
        readonly INavigationService NavigationService;

        public ICommand NewTransactionCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public Budget Budget { get; set; }
        public ObservableCollection<Transaction> Transactions { get; set; }
        public ObservableCollection<GroupedList<Transaction>> GroupedTransactions { get; set; }
        public Transaction SelectedTransaction { get; set; }

        public EnvelopeInfoPageViewModel(INavigationService navigationService, ITransactionLogic transactionLogic)
        {
            Title = "Envelope Overview";

            TransactionLogic = transactionLogic;
            NavigationService = navigationService;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            TransactionSelectedCommand = new DelegateCommand(async () => await ExecuteTransactionSelectedCommand());
            NewTransactionCommand = new DelegateCommand(async () => await ExecuteNewTransactionCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
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
                var result = await TransactionLogic.GetEnvelopeTransactionsAsync(Budget.Envelope);
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

        public override async void OnNavigatingTo(NavigationParameters parameters)
        {
            var budget = parameters.GetValue<Budget>(NavigationParameterType.Budget);
            if (budget != null)
            {
                Budget = budget.DeepCopy();
            }

            await ExecuteRefreshCommand();
        }
    }
}
