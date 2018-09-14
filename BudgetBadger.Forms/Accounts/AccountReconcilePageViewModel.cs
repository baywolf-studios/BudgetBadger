using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountReconcilePageViewModel : BindableBase, INavigatingAware
    {
        readonly ITransactionLogic _transactionLogic;
        readonly INavigationService _navigationService;
        readonly IAccountLogic _accountLogic;
        readonly IPageDialogService _dialogService;
        readonly ISync _syncService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
        public ICommand ReconcileCommand { get; set; }
        public ICommand ToggleReconcileModeCommand { get; set; }
        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand DeleteTransactionCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public Predicate<object> Filter { get => (t) => _transactionLogic.FilterTransaction((Transaction)t, SearchText); }

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

        IReadOnlyList<Transaction> _transactions;
        public IReadOnlyList<Transaction> Transactions
        {
            get => _transactions;
            set
            {
                SetProperty(ref _transactions, value);
                RaisePropertyChanged(nameof(PostedTotal));
                RaisePropertyChanged(nameof(Difference));
            }
        }

        IReadOnlyList<Transaction> _statementTransactions;
        public IReadOnlyList<Transaction> StatementTransactions
        {
            get => _statementTransactions;
            set
            {
                SetProperty(ref _statementTransactions, value);
                RaisePropertyChanged(nameof(PostedTotal));
                RaisePropertyChanged(nameof(Difference));
            }
        }

        Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        public decimal PostedTotal { get => Transactions?.Where(t => t.Posted && t.ServiceDate <= StatementDate).Sum(t2 => t2.Amount ?? 0) ?? 0; }
        public decimal Difference { get => StatementAmount - PostedTotal; }

        bool _noTransactions;
        public bool NoTransactions
        {
            get => _noTransactions;
            set => SetProperty(ref _noTransactions, value);
        }

        DateTime _statementDate;
        public DateTime StatementDate
        {
            get => _statementDate;
            set
            {
                SetProperty(ref _statementDate, value);
                UpdateStatementTransactions();
            }
        }

        decimal _statementAmount;
        public decimal StatementAmount
        {
            get => _statementAmount;
            set
            {
                SetProperty(ref _statementAmount, value);
                RaisePropertyChanged(nameof(Difference));
            }
        }

        bool _reconcileMode;
        public bool ReconcileMode
        {
            get => _reconcileMode;
            set
            {
                SetProperty(ref _reconcileMode, value);
                RaisePropertyChanged(nameof(StatementMode));
            }
        }

        public bool StatementMode
        {
            get => !ReconcileMode;
        }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public AccountReconcilePageViewModel(INavigationService navigationService,
                                             ITransactionLogic transactionLogic, 
                                             IAccountLogic accountLogic, 
                                             IPageDialogService dialogService,
                                             ISync syncService)
        {
            _navigationService = navigationService;
            _transactionLogic = transactionLogic;
            _accountLogic = accountLogic;
            _dialogService = dialogService;
            _syncService = syncService;

            Account = new Account();
            Transactions = new List<Transaction>();
            StatementTransactions = new List<Transaction>();
            SelectedTransaction = null;
            StatementDate = DateTime.Now;
            StatementAmount = 0;


            ReconcileCommand = new DelegateCommand(async () => await ExecuteReconcileCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            ToggleReconcileModeCommand = new DelegateCommand(ExecuteToggleReconcileModeCommand);
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
            DeleteTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteDeleteTransactionCommand(t));
            TransactionSelectedCommand = new DelegateCommand<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
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

        public async Task ExecuteRefreshCommand()
        {
            if (!IsBusy)
            {
                IsBusy = true;
            }

            try
            {
                if (Account.IsActive)
                {
                    var accountResult = await _accountLogic.GetAccountAsync(Account.Id);
                    if (accountResult.Success)
                    {
                        Account = accountResult.Data;
                    }
                    else
                    {
                        //show alert that account data may be stale
                    }

                    var result = await _transactionLogic.GetAccountTransactionsAsync(Account);
                    if (result.Success)
                    {
                        Transactions = result.Data;
                        StatementTransactions = result.Data;
                        SelectedTransaction = null;
                    }
                }

                UpdateStatementTransactions();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void UpdateStatementTransactions()
        {
            StatementTransactions = Transactions.Where(t => !t.Reconciled && t.ServiceDate <= StatementDate).ToList();
            NoTransactions = (StatementTransactions?.Count ?? 0) == 0;
        }

        public async Task ExecuteReconcileCommand()
        {
            var reconcileResult = await _accountLogic.ReconcileAccount(Account.Id, StatementDate, StatementAmount);

            if (reconcileResult.Success)
            {
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Could not reconcile", reconcileResult.Message, "OK");
            }
        }

        public void ExecuteToggleReconcileModeCommand()
        {
            ReconcileMode = !ReconcileMode;
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
                    var syncTask = _syncService.FullSync();

                    var syncResult = await syncTask;
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }
                }
                else
                {
                    transaction.Posted = !transaction.Posted;
                    await _dialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
                }
            }

            await ExecuteRefreshCommand();
        }

        public async Task ExecuteDeleteTransactionCommand(Transaction transaction)
        {
            var result = await _transactionLogic.DeleteTransactionAsync(transaction.Id);

            if (result.Success)
            {
                var syncResult = await _syncService.FullSync();

                if (!syncResult.Success)
                {
                    await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                }

                await ExecuteRefreshCommand();
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
            }
        }

        public async Task ExecuteTransactionSelectedCommand(Transaction transaction)
        {
            if (transaction == null)
            {
                return;
            }


            if (transaction.IsSplit)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.SplitTransactionId, transaction.SplitId }
                };
                await _navigationService.NavigateAsync(PageName.SplitTransactionPage, parameters);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Transaction, transaction }
                };
                await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
            }
        }
    }
}
