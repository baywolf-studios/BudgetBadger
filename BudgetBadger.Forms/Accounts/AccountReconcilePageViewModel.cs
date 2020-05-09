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
using BudgetBadger.Forms.Events;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountReconcilePageViewModel : ObservableBase, INavigationAware, IInitializeAsync
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
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
        public ICommand RefreshSummaryCommand { get; set; }
        public ICommand ReconcileCommand { get; set; }
        public ICommand ToggleReconcileModeCommand { get; set; }
        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand DeleteTransactionCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand SaveTransactionCommand { get; set; }
        public ICommand RefreshTransactionCommand { get; set; }
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

        ObservableList<Payee> _payees;
        public ObservableList<Payee> Payees
        {
            get => _payees;
            set => SetProperty(ref _payees, value);
        }

        ObservableList<Envelope> _envelopes;
        public ObservableList<Envelope> Envelopes
        {
            get => _envelopes;
            set => SetProperty(ref _envelopes, value);
        }

        ObservableList<Transaction> _transactions;
        public ObservableList<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        ObservableList<Transaction> _statementTransactions;
        public ObservableList<Transaction> StatementTransactions
        {
            get => _statementTransactions;
            set => SetProperty(ref _statementTransactions, value);
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
            set => SetProperty(ref _statementDate, value);
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
            set => SetProperty(ref _reconcileMode, value);
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

        bool _fullRefresh = true;

        public AccountReconcilePageViewModel(Lazy<IResourceContainer> resourceContainer,
                                             INavigationService navigationService,
                                             Lazy<ITransactionLogic> transactionLogic,
                                             Lazy<IAccountLogic> accountLogic, 
                                             IPageDialogService dialogService,
                                             Lazy<ISyncFactory> syncFactory,
                                             Lazy<IEnvelopeLogic> envelopeLogic,
                                             Lazy<IPayeeLogic> payeeLogic,
                                             Lazy<IPurchaseService> purchaseService,
                                             IEventAggregator eventAggregator)
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
            _eventAggregator = eventAggregator;

            Account = new Account();
            Transactions = new ObservableList<Transaction>();
            StatementTransactions = new ObservableList<Transaction>();
            Payees = new ObservableList<Payee>();
            Envelopes = new ObservableList<Envelope>();
            SelectedTransaction = null;
            StatementDate = DateTime.Now;
            StatementAmount = 0;


            ReconcileCommand = new DelegateCommand(async () => await ExecuteReconcileCommand());
            RefreshCommand = new DelegateCommand(async () => await FullRefresh());
            RefreshSummaryCommand = new DelegateCommand(async () => await RefreshSummary());
            ToggleReconcileModeCommand = new DelegateCommand(async () => await ExecuteToggleReconcileModeCommand());
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
            DeleteTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteDeleteTransactionCommand(t));
            TransactionSelectedCommand = new DelegateCommand<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
            SaveTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteSaveTransactionCommand(t));
            RefreshTransactionCommand = new DelegateCommand<Transaction>(RefreshTransaction);

            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(RefreshTransaction);
            _eventAggregator.GetEvent<SplitTransactionSavedEvent>().Subscribe(async () => await FullRefresh());
            _eventAggregator.GetEvent<TransactionStatusUpdatedEvent>().Subscribe(RefreshTransactionStatus);
            _eventAggregator.GetEvent<SplitTransactionStatusUpdatedEvent>().Subscribe(RefreshSplitTransactionStatus);
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(RefreshTransaction);

            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshSummary());
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(async t => await RefreshSummary());

            _eventAggregator.GetEvent<PayeeSavedEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<PayeeDeletedEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<PayeeHiddenEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<PayeeUnhiddenEvent>().Subscribe(RefreshPayee);

            _eventAggregator.GetEvent<AccountSavedEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<AccountDeletedEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<AccountHiddenEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<AccountUnhiddenEvent>().Subscribe(RefreshAccount);

            _eventAggregator.GetEvent<BudgetSavedEvent>().Subscribe(b => RefreshEnvelope(b?.Envelope));
            _eventAggregator.GetEvent<EnvelopeDeletedEvent>().Subscribe(RefreshEnvelope);
            _eventAggregator.GetEvent<EnvelopeHiddenEvent>().Subscribe(RefreshEnvelope);
            _eventAggregator.GetEvent<EnvelopeUnhiddenEvent>().Subscribe(RefreshEnvelope);
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
            }

            var purchasedPro = await _purchaseService.Value.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (_fullRefresh)
            {
                await FullRefresh();
                _fullRefresh = false;
            }
            else
            {
                await RefreshSummary();
            }
        }

        public async void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedTransaction = null;

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

        public async Task ExecuteToggleReconcileModeCommand()
        {
            if (!ReconcileMode)
            {
                UpdateStatementTransactions();
            }
            ReconcileMode = !ReconcileMode;
            await RefreshSummary();
        }

        public async Task ExecuteTogglePostedTransaction(Transaction transaction)
        {
            if (transaction != null && !transaction.Reconciled)
            {
                transaction.Posted = !transaction.Posted;

                Result result;

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
                    await RefreshSummary();
                    if (result is Result<Transaction> tranResult)
                        _eventAggregator.GetEvent<TransactionStatusUpdatedEvent>().Publish(tranResult.Data);
                    else
                        _eventAggregator.GetEvent<SplitTransactionStatusUpdatedEvent>().Publish(transaction);
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
            var result = await _transactionLogic.Value.SoftDeleteTransactionAsync(transaction.Id);

            if (result.Success)
            {
                _eventAggregator.GetEvent<TransactionDeletedEvent>().Publish(result.Data);

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
                    _eventAggregator.GetEvent<TransactionSavedEvent>().Publish(result.Data);
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

        public async Task FullRefresh()
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
                            Payees.ReplaceRange(payeesResult.Data);
                        }
                        else if (!payeesResult.Success)
                        {
                            await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), payeesResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                        }

                        var envelopesResult = await _envelopeLogic.Value.GetEnvelopesForSelectionAsync();
                        if (envelopesResult.Success
                            && (Envelopes == null || !Envelopes.SequenceEqual(envelopesResult.Data)))
                        {
                            Envelopes.ReplaceRange(envelopesResult.Data);
                        }
                        else if (!envelopesResult.Success)
                        {
                            await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), envelopesResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                        }
                    }

                    var result = await _transactionLogic.Value.GetAccountTransactionsAsync(Account);
                    if (result.Success
                        && (Transactions == null || !Transactions.SequenceEqual(result.Data)))
                    {
                        Transactions.ReplaceRange(result.Data);
                        StatementTransactions.ReplaceRange(result.Data);
                    }
                    else if (!result.Success)
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                    }

                    
                }

                UpdateStatementTransactions();
                await RefreshSummary();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task RefreshSummary()
        {
            var accountResult = await _accountLogic.Value.GetAccountAsync(Account.Id);
            if (accountResult.Success)
            {
                RefreshAccount(accountResult.Data);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), accountResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }

            NoTransactions = (StatementTransactions?.Count ?? 0) == 0;

            RaisePropertyChanged(nameof(StatementMode));
            RaisePropertyChanged(nameof(PostedTotal));
            RaisePropertyChanged(nameof(Difference));
        }

        public void RefreshTransaction(Transaction transaction)
        {
            var transactions = Transactions.Where(t => t.Id != transaction.Id).ToList();
            if (transaction != null && transaction.IsActive)
            {
                transactions.Add(transaction);
            }

            Transactions.ReplaceRange(transactions);
        }

        public void RefreshPayee(Payee payee)
        {
            foreach (var transaction in Transactions.Where(t => t.Payee.Id == payee.Id))
            {
                transaction.Payee = payee;
            }

            var payees = Payees.Where(p => p.Id != payee.Id).ToList();

            if (payee != null && payee.IsActive)
            {
                payees.Add(payee);
            }

            Payees.ReplaceRange(payees);
        }

        public void RefreshEnvelope(Envelope envelope)
        {
            foreach (var transaction in Transactions.Where(t => t.Envelope.Id == envelope.Id))
            {
                transaction.Envelope = envelope;
            }

            var envelopes = Envelopes.Where(e => e.Id != envelope.Id).ToList();

            if (envelope != null && envelope.IsActive)
            {
                envelopes.Add(envelope);
            }

            Envelopes.ReplaceRange(envelopes);
        }

        public void RefreshAccount(Account account)
        {
            Account = account;

            foreach (var transaction in Transactions.Where(t => t.Account.Id == account.Id))
            {
                transaction.Account = account;
            }
        }

        public void RefreshTransactionStatus(Transaction transaction)
        {
            var transactions = Transactions.Where(t => t.Id == transaction.Id);
            foreach (var tran in transactions)
            {
                tran.Posted = transaction.Posted;
            }
        }

        public void RefreshSplitTransactionStatus(Transaction transaction)
        {
            var transactions = Transactions.Where(t => t.SplitId == transaction.SplitId);
            foreach (var tran in transactions)
            {
                tran.Posted = transaction.Posted;
            }
        }

        void UpdateStatementTransactions()
        {
            var temp = Transactions.Where(t => !t.Reconciled && t.ServiceDate <= StatementDate).ToList();
            StatementTransactions.ReplaceRange(temp);
        }
    }
}
