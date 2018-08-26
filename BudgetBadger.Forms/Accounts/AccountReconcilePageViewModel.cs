using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
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

        public ICommand ReconcileCommand { get; set; }
        public ICommand ToggleReconcileModeCommand { get; set; }

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
                RaisePropertyChanged(nameof(PendingTotal));
                RaisePropertyChanged(nameof(PostedTotal));
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

        DateTime _reconcileDate;
        public DateTime ReconcileDate
        {
            get => _reconcileDate;
            set => SetProperty(ref _reconcileDate, value);
        }

        decimal _reconcileAmount;
        public decimal ReconcileAmount
        {
            get => _reconcileAmount;
            set => SetProperty(ref _reconcileAmount, value);
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
                                             IPageDialogService dialogService)
        {
            _navigationService = navigationService;
            _transactionLogic = transactionLogic;
            _accountLogic = accountLogic;
            _dialogService = dialogService;

            Account = new Account();
            Transactions = new List<Transaction>();
            GroupedTransactions = Transactions.GroupBy(t => "").ToList();
            SelectedTransaction = null;
            ReconcileDate = DateTime.Now;
            ReconcileAmount = 0;

            ReconcileCommand = new DelegateCommand(async () => await ExecuteReconcileCommand());
            ToggleReconcileModeCommand = new DelegateCommand(ExecuteToggleReconcileModeCommand);
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
                        GroupedTransactions = _transactionLogic.GroupTransactions(Transactions);
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

        public async Task ExecuteReconcileCommand()
        {
            var reconcileResult = await _accountLogic.ReconcileAccount(Account.Id, ReconcileDate, PostedTotal);

            if (reconcileResult.Success)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.ReconcileCompleted , true }
                };

                await _navigationService.GoBackAsync(parameters);
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
    }
}
