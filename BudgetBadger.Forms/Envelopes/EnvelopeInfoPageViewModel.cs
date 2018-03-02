using System;
using System.Collections.ObjectModel;
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

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeInfoPageViewModel : BindableBase, INavigatingAware
    {
        readonly ITransactionLogic TransactionLogic;
        readonly INavigationService NavigationService;
        readonly IEnvelopeLogic EnvelopeLogic;

        public ICommand NewTransactionCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        Budget _budget;
        public Budget Budget
        {
            get => _budget;
            set => SetProperty(ref _budget, value);
        }

        IEnumerable<Transaction> _transactions;
        public IEnumerable<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
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

        public EnvelopeInfoPageViewModel(INavigationService navigationService, ITransactionLogic transactionLogic, IEnvelopeLogic envelopeLogic)
        {
            TransactionLogic = transactionLogic;
            NavigationService = navigationService;
            EnvelopeLogic = envelopeLogic;

            Budget = new Budget();
            Transactions = new List<Transaction>();
            GroupedTransactions = Transactions.ToLookup(t => "");
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            TransactionSelectedCommand = new DelegateCommand(async () => await ExecuteTransactionSelectedCommand());
            NewTransactionCommand = new DelegateCommand(async () => await ExecuteNewTransactionCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            var budget = parameters.GetValue<Budget>(PageParameter.Budget);
            if (budget != null)
            {
                Budget = budget.DeepCopy();
            }

            await ExecuteRefreshCommand();
        }

        public async Task ExecuteEditCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Budget, Budget }
            };
            await NavigationService.NavigateAsync(PageName.EnvelopeEditPage, parameters);
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

        public async Task ExecuteNewTransactionCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Envelope, Budget.Envelope }
            };
            await NavigationService.NavigateAsync(PageName.TransactionPage, parameters);
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
                if (Budget.IsActive)
                {
                    var budgetResult = await EnvelopeLogic.GetBudgetAsync(Budget.Id);
                    if (budgetResult.Success)
                    {
                        Budget = budgetResult.Data;
                    }
                }

                var result = await TransactionLogic.GetEnvelopeTransactionsAsync(Budget.Envelope);
                if (result.Success)
                {
                    Transactions = result.Data;
                    GroupedTransactions = TransactionLogic.GroupTransactions(Transactions);
                    SelectedTransaction = null;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
