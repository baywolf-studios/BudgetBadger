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
using Prism.Services;

namespace BudgetBadger.Forms.Payees
{
    public class PayeeInfoPageViewModel : BindableBase, INavigatingAware
    {
        readonly ITransactionLogic _transactionLogic;
        readonly INavigationService _navigationService;
        readonly IPayeeLogic _payeeLogic;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public ICommand SearchCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        Payee _payee;
        public Payee Payee
        {
            get => _payee;
            set => SetProperty(ref _payee, value);
        }

        IReadOnlyList<Transaction> _transactions;
        public IReadOnlyList<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        IReadOnlyList<IGrouping<string, Transaction>> _groupedTransactions;
        public IReadOnlyList<IGrouping<string, Transaction>> GroupedTransactions
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

        public decimal LifetimeSpent { get => Transactions.Sum(t => t.Amount ?? 0); }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        bool _noTransactions;
        public bool NoTransactions
        {
            get => _noTransactions;
            set => SetProperty(ref _noTransactions, value);
        }

        public PayeeInfoPageViewModel(INavigationService navigationService, ITransactionLogic transactionLogic, IPayeeLogic payeeLogic, IPageDialogService dialogService)
        {
            _transactionLogic = transactionLogic;
            _navigationService = navigationService;
            _payeeLogic = payeeLogic;
            _dialogService = dialogService;

            Payee = new Payee();
            Transactions = new List<Transaction>();
            GroupedTransactions = Transactions.GroupBy(t => "").ToList();
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            TransactionSelectedCommand = new DelegateCommand(async () => await ExecuteTransactionSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                Payee = payee.DeepCopy();
            }

            await ExecuteRefreshCommand();
        }

        public async Task ExecuteEditCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Payee, Payee }
            };
            await _navigationService.NavigateAsync(PageName.PayeeEditPage, parameters);
        }

        public async Task ExecuteTransactionSelectedCommand()
        {
            if (SelectedTransaction == null)
            {
                return;
            }

            if (SelectedTransaction.IsSplit)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.SplitTransactionId, SelectedTransaction.SplitId }
                };
                await _navigationService.NavigateAsync(PageName.SplitTransactionPage, parameters);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Transaction, SelectedTransaction }
                };
                await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
            }

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
                if (Payee.IsActive)
                {
                    var payeeResult = await _payeeLogic.GetPayeeAsync(Payee.Id);
                    if (payeeResult.Success)
                    {
                        Payee = payeeResult.Data;
                    }
                    else
                    {
                        //show alert that account data may be stale
                    }

                    var result = await _transactionLogic.GetPayeeTransactionsAsync(Payee);
                    if (result.Success)
                    {
                        Transactions = result.Data;
                        GroupedTransactions = _transactionLogic.GroupTransactions(Transactions);
                        SelectedTransaction = null;
                    }

                    NoTransactions = (Transactions?.Count ?? 0) == 0;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteAddTransactionCommand()
        {
            await _navigationService.NavigateAsync(PageName.TransactionEditPage);
        }

        public async Task ExecuteTogglePostedTransaction(Transaction transaction)
        {
            if (transaction != null && !transaction.Reconciled)
            {
                transaction.Posted = !transaction.Posted;

                Result result = new Result();

                if (transaction.IsCombined)
                {
                    result = await _transactionLogic.UpdateSplitTransactionPostedAsync(transaction.SplitId.Value, transaction.Posted);
                }
                else
                {
                    result = await _transactionLogic.SaveTransactionAsync(transaction);
                }

                if (result.Success)
                {
                    //var syncTask = _syncService.FullSync();

                    //var syncResult = await syncTask;
                    //if (!syncResult.Success)
                    //{
                    //    await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    //}
                }
                else
                {
                    transaction.Posted = !transaction.Posted;
                    await _dialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
                }
            }
        }

        public void ExecuteSearchCommand()
        {
            GroupedTransactions = _transactionLogic.GroupTransactions(_transactionLogic.SearchTransactions(Transactions, SearchText));
        }
    }
}
