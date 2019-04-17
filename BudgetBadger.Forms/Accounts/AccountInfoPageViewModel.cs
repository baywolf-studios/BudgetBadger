using System;
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
using BudgetBadger.Core.Sync;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountInfoPageViewModel : BindableBase, INavigationAware
    {
        readonly ITransactionLogic _transactionLogic;
        readonly INavigationService _navigationService;
        readonly IAccountLogic _accountLogic;
        readonly IPageDialogService _dialogService;
        readonly ISyncFactory _syncFactory;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteTransactionCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public ICommand PaymentCommand { get; set; }
        public ICommand ReconcileCommand { get; set; }
        public Predicate<object> Filter { get => (t) => _transactionLogic.FilterTransaction((Transaction)t, SearchText); }

        bool _needToSync;

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

        ObservableList<Transaction> _transactions;
        public ObservableList<Transaction> Transactions
        {
            get => _transactions;
            set
            {
                SetProperty(ref _transactions, value);
                RaisePropertyChanged(nameof(PendingTotal));
                RaisePropertyChanged(nameof(PostedTotal));
            }
        }

        Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        public decimal PendingTotal { get => Transactions?.Where(t => t.Pending).Sum(t2 => t2.Amount ?? 0) ?? 0; }
        public decimal PostedTotal { get => Transactions?.Where(t => t.Posted).Sum(t2 => t2.Amount ?? 0) ?? 0; }

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

        public AccountInfoPageViewModel(INavigationService navigationService,
                                        ITransactionLogic transactionLogic,
                                        IAccountLogic accountLogic,
                                        IPageDialogService dialogService,
                                        ISyncFactory syncFactory)
        {
            _transactionLogic = transactionLogic;
            _navigationService = navigationService;
            _accountLogic = accountLogic;
            _dialogService = dialogService;
            _syncFactory = syncFactory;

            Account = new Account();
            Transactions = new ObservableList<Transaction>();
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            DeleteTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteDeleteTransactionCommand(t));
            TransactionSelectedCommand = new DelegateCommand<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
            PaymentCommand = new DelegateCommand(async () => await ExecutePaymentCommand());
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
            ReconcileCommand = new DelegateCommand(async () => await ExecuteReconcileCommand());
        }

        public async void OnNavigatingTo(INavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
            }

            await ExecuteRefreshCommand();
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (_needToSync)
            {
                var syncService = _syncFactory.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.SetLastSyncDateTime(DateTime.Now);
                }
            }
        }

        public async Task ExecuteEditCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Account, Account }
            };
            await _navigationService.NavigateAsync(PageName.AccountEditPage, parameters);
        }

        public async Task ExecuteDeleteTransactionCommand(Transaction transaction)
        {
            var result = await _transactionLogic.DeleteTransactionAsync(transaction.Id);

            if (result.Success)
            {
                await ExecuteRefreshCommand();

                _needToSync = true;
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
                        Transactions.MergeRange(result.Data);
                        Transactions.Sort();
                        SelectedTransaction = null;
                    }
                }

                NoTransactions = (Transactions?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteAddTransactionCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Account, Account }
            };

            await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
        }

        public async Task ExecutePaymentCommand()
        {
            if (Account.PaymentRequired)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Account, Account },
                    { PageParameter.TransactionAmount, Account.Payment }
                };
                await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
            }
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
                    var transactionFromList = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
                    if (transactionFromList != null)
                    {
                        transactionFromList.Posted = transaction.Posted;
                    }
                    RaisePropertyChanged(nameof(PendingTotal));
                    RaisePropertyChanged(nameof(PostedTotal));

                    _needToSync = true;
                }
                else
                {
                    transaction.Posted = !transaction.Posted;
                    await _dialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
                }
            }
        }

        public async Task ExecuteReconcileCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Account, Account }
            };

            await _navigationService.NavigateAsync(PageName.AccountReconcilePage, parameters);
        }
    }
}
