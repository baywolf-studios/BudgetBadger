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
        public ICommand ReconcileCommand { get; set; }
        public ICommand ToggleReconcileModeCommand { get; set; }
        public ICommand TogglePostedTransactionCommand { get; set; }

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

        IReadOnlyList<Transaction> _filteredTransactions;
        public IReadOnlyList<Transaction> FilteredTransactions
        {
            get => _filteredTransactions;
            set
            {
                SetProperty(ref _filteredTransactions, value);
                RaisePropertyChanged(nameof(PostedTotal));
                RaisePropertyChanged(nameof(Difference));
            }
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
            FilteredTransactions = new List<Transaction>();
            GroupedTransactions = Transactions.GroupBy(t => "").ToList();
            SelectedTransaction = null;
            StatementDate = DateTime.Now;
            StatementAmount = 0;

            ReconcileCommand = new DelegateCommand(async () => await ExecuteReconcileCommand());
            ToggleReconcileModeCommand = new DelegateCommand(ExecuteToggleReconcileModeCommand);
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
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
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

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
                        FilteredTransactions = result.Data;
                        GroupedTransactions = _transactionLogic.GroupTransactions(FilteredTransactions);
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
            FilteredTransactions = Transactions.Where(t => !t.Reconciled && t.ServiceDate <= StatementDate).ToList();
            GroupedTransactions = _transactionLogic.GroupTransactions(FilteredTransactions);
            NoTransactions = (FilteredTransactions?.Count ?? 0) == 0;
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
        }
    }
}
