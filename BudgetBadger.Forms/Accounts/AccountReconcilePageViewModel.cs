using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Purchase;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountReconcilePageViewModel : BindableBase, INavigationAware
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<ITransactionLogic> _transactionLogic;
        readonly INavigationService _navigationService;
        readonly Lazy<IAccountLogic> _accountLogic;
        readonly IPageDialogService _dialogService;
        readonly Lazy<ISyncFactory> _syncFactory;
        readonly Lazy<IEnvelopeLogic> _envelopeLogic;
        readonly Lazy<IPayeeLogic> _payeeLogic;
        readonly Lazy<IPurchaseService> _purchaseService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
        public ICommand ReconcileCommand { get; set; }
        public ICommand ToggleReconcileModeCommand { get; set; }
        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand DeleteTransactionCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand SaveTransactionCommand { get; set; }
        public Predicate<object> Filter { get => (t) => _transactionLogic.Value.FilterTransaction((Transaction)t, SearchText); }

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

        IReadOnlyList<Payee> _payees;
        public IReadOnlyList<Payee> Payees
        {
            get => _payees;
            set => SetProperty(ref _payees, value);
        }

        IReadOnlyList<Envelope> _envelopes;
        public IReadOnlyList<Envelope> Envelopes
        {
            get => _envelopes;
            set => SetProperty(ref _envelopes, value);
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

        bool _hasPro;
        public bool HasPro
        {
            get => _hasPro;
            set => SetProperty(ref _hasPro, value);
        }

        public AccountReconcilePageViewModel(Lazy<IResourceContainer> resourceContainer,
                                             INavigationService navigationService,
                                             Lazy<ITransactionLogic> transactionLogic,
                                             Lazy<IAccountLogic> accountLogic, 
                                             IPageDialogService dialogService,
                                             Lazy<ISyncFactory> syncFactory,
                                             Lazy<IEnvelopeLogic> envelopeLogic,
                                             Lazy<IPayeeLogic> payeeLogic,
                                             Lazy<IPurchaseService> purchaseService)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _transactionLogic = transactionLogic;
            _accountLogic = accountLogic;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _envelopeLogic = envelopeLogic;
            _payeeLogic = payeeLogic;
            _purchaseService = purchaseService;

            Account = new Account();
            Transactions = new List<Transaction>();
            StatementTransactions = new List<Transaction>();
            Payees = new List<Payee>();
            Envelopes = new List<Envelope>();
            SelectedTransaction = null;
            StatementDate = DateTime.Now;
            StatementAmount = 0;


            ReconcileCommand = new DelegateCommand(async () => await ExecuteReconcileCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            ToggleReconcileModeCommand = new DelegateCommand(ExecuteToggleReconcileModeCommand);
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
            DeleteTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteDeleteTransactionCommand(t));
            TransactionSelectedCommand = new DelegateCommand<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
            SaveTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteSaveTransactionCommand(t));
        }

        public async void OnNavigatingTo(INavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
            }

            var purchasedPro = await _purchaseService.Value.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;

            await ExecuteRefreshCommand();
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (_needToSync)
            {
                var syncService = _syncFactory.Value.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.Value.SetLastSyncDateTime(DateTime.Now);
                    _needToSync = false;
                }
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
                    if (Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet)
                    {
                        var payeesResult = await _payeeLogic.Value.GetPayeesForSelectionAsync();
                        if (payeesResult.Success
                            && (Payees == null || !Payees.SequenceEqual(payeesResult.Data)))
                        {
                            Payees = payeesResult.Data;
                        }
                        else if (!payeesResult.Success)
                        {
                            await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), payeesResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                        }

                        var envelopesResult = await _envelopeLogic.Value.GetEnvelopesForSelectionAsync();
                        if (envelopesResult.Success
                            && (Envelopes == null || !Envelopes.SequenceEqual(envelopesResult.Data)))
                        {
                            Envelopes = envelopesResult.Data;
                        }
                        else if (!envelopesResult.Success)
                        {
                            await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), envelopesResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                        }
                    }

                    var accountResult = await _accountLogic.Value.GetAccountAsync(Account.Id);
                    if (accountResult.Success)
                    {
                        Account = accountResult.Data;
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), accountResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                    }

                    var result = await _transactionLogic.Value.GetAccountTransactionsAsync(Account);
                    if (result.Success
                        && (Transactions == null || !Transactions.SequenceEqual(result.Data)))
                    {
                        Transactions = result.Data;
                    }
                    else if (!result.Success)
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                    }
                    SelectedTransaction = null;
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
            var temp = Transactions.Where(t => !t.Reconciled && t.ServiceDate <= StatementDate).ToList();
            temp.Sort();
            StatementTransactions = temp;

            NoTransactions = (StatementTransactions?.Count ?? 0) == 0;
        }

        public async Task ExecuteReconcileCommand()
        {
            var reconcileResult = await _accountLogic.Value.ReconcileAccount(Account.Id, StatementDate, StatementAmount);

            if (reconcileResult.Success)
            {
                _needToSync = true;

                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertReconcileUnsuccessful"), reconcileResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
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
                    result = await _transactionLogic.Value.UpdateSplitTransactionPostedAsync(transaction.SplitId.Value, transaction.Posted);
                }
                else
                {
                    result = await _transactionLogic.Value.SaveTransactionAsync(transaction);
                }

                if (result.Success)
                {
                    var transactionFromList = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
                    if (transactionFromList != null)
                    {
                        transactionFromList.Posted = transaction.Posted;
                    }
                    RaisePropertyChanged(nameof(PostedTotal));
                    RaisePropertyChanged(nameof(Difference));

                    _needToSync = true;
                }
                else
                {
                    transaction.Posted = !transaction.Posted;
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }
            }
        }

        public async Task ExecuteDeleteTransactionCommand(Transaction transaction)
        {
            var result = await _transactionLogic.Value.DeleteTransactionAsync(transaction.Id);

            if (result.Success)
            {
                await ExecuteRefreshCommand();

                _needToSync = true;
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
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

        public async Task ExecuteSaveTransactionCommand(Transaction transaction)
        {
            var correctedTransactionResult = await _transactionLogic.Value.GetCorrectedTransaction(transaction);

            if (correctedTransactionResult.Success)
            {
                transaction = correctedTransactionResult.Data;
                var result = await _transactionLogic.Value.SaveTransactionAsync(transaction);

                if (result.Success)
                {
                    _needToSync = true;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), correctedTransactionResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }
    }
}
