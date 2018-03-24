using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Transactions
{
    public class TransactionSelectionPageViewModel : BindableBase, IPageLifecycleAware
    {
        readonly ITransactionLogic _transactionLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SearchCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        IReadOnlyList<Transaction> _transactions;
        public IReadOnlyList<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        IReadOnlyList<IGrouping<string, Transaction>> _groupedTransactions;
        public IReadOnlyList<IGrouping<string, Transaction>> GroupedTransactions
        {
            get => _groupedTransactions;
            set => SetProperty(ref _groupedTransactions, value);
        }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); ExecuteSearchCommand(); }
        }

        public TransactionSelectionPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ITransactionLogic transactionLogic)
        {
            _transactionLogic = transactionLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Transactions = new List<Transaction>();
            SelectedTransaction = null;
            GroupedTransactions = Transactions.GroupBy(a => "").ToList();

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
        }

        public async void OnAppearing()
        {
            await ExecuteRefreshCommand();
        }

        public void OnDisappearing()
        {
        }

        public async Task ExecuteSelectedCommand()
        {
            if (SelectedTransaction == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Transaction, SelectedTransaction }
            };

            await _navigationService.GoBackAsync(parameters);

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
                Result<IReadOnlyList<Transaction>> result;

                result = await _transactionLogic.GetTransactionsAsync();

                if (result.Success)
                {
                    Transactions = result.Data;
                    GroupedTransactions = _transactionLogic.GroupTransactions(Transactions);
                }
                else
                {
                    await Task.Yield();
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "Okay");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }


        public void ExecuteSearchCommand()
        {
            GroupedTransactions = _transactionLogic.GroupTransactions(_transactionLogic.SearchTransactions(Transactions, SearchText));
        }
    }
}
