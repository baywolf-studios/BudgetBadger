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
using BudgetBadger.Core.Sync;
using BudgetBadger.Core.Settings;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Purchase;
using Xamarin.Forms;
using Prism.Events;
using BudgetBadger.Forms.Events;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeInfoPageViewModel : BindableBase, INavigationAware, IInitializeAsync
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<ITransactionLogic> _transactionLogic;
        readonly INavigationService _navigationService;
        readonly Lazy<IEnvelopeLogic> _envelopeLogic;
        readonly IPageDialogService _dialogService;
        readonly Lazy<ISyncFactory> _syncFactory;
        readonly Lazy<IAccountLogic> _accountLogic;
        readonly Lazy<IPayeeLogic> _payeeLogic;
        readonly Lazy<IPurchaseService> _purchaseService;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand DeleteTransactionCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand TransferCommand { get; set; }
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

        Budget _budget;
        public Budget Budget
        {
            get => _budget;
            set => SetProperty(ref _budget, value);
        }

        ObservableList<Payee> _payees;
        public ObservableList<Payee> Payees
        {
            get => _payees;
            set => SetProperty(ref _payees, value);
        }

        ObservableList<Account> _accounts;
        public ObservableList<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        ObservableList<Transaction> _transactions;
        public ObservableList<Transaction> Transactions
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

        bool _hasPro;
        public bool HasPro
        {
            get => _hasPro;
            set => SetProperty(ref _hasPro, value);
        }

        public EnvelopeInfoPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                         INavigationService navigationService,
                                         Lazy<ITransactionLogic> transactionLogic,
                                         Lazy<IEnvelopeLogic> envelopeLogic,
                                         IPageDialogService dialogService,
                                         Lazy<ISyncFactory> syncFactory,
                                         Lazy<IAccountLogic> accountLogic,
                                         Lazy<IPayeeLogic> payeeLogic,
                                         Lazy<IPurchaseService> purchaseService,
                                         IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _transactionLogic = transactionLogic;
            _navigationService = navigationService;
            _envelopeLogic = envelopeLogic;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _accountLogic = accountLogic;
            _payeeLogic = payeeLogic;
            _purchaseService = purchaseService;
            _eventAggregator = eventAggregator;

            Budget = new Budget();
            Transactions = new ObservableList<Transaction>();
            Accounts = new ObservableList<Account>();
            Payees = new ObservableList<Payee>();
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            DeleteTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteDeleteTransactionCommand(t));
            TransactionSelectedCommand = new DelegateCommand<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
            TransferCommand = new DelegateCommand(async () => await ExecuteTransferCommand());
            SaveTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteSaveTransactionCommand(t));
            RefreshTransactionCommand = new DelegateCommand<Transaction>(ExecuteRefreshTransactionCommand);

            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(ExecuteRefreshTransactionCommand);
            _eventAggregator.GetEvent<SplitTransactionSavedEvent>().Subscribe(async () => await ExecuteRefreshCommand());
            _eventAggregator.GetEvent<TransactionStatusUpdatedEvent>().Subscribe(UpdateTransactionStatus);
            _eventAggregator.GetEvent<SplitTransactionStatusUpdatedEvent>().Subscribe(UpdateSplitTransactionStatus);
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(ExecuteRefreshTransactionCommand);
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
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

            if (!parameters.TryGetValue<Budget>(PageParameter.Budget, out _))
            {
                parameters.Add(PageParameter.Budget, Budget);
            }
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var budget = parameters.GetValue<Budget>(PageParameter.Budget);
            if (budget != null)
            {
                Budget = budget.DeepCopy();
            }

            var purchasedPro = await _purchaseService.Value.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;

            await ExecuteRefreshCommand();
        }

        public async Task ExecuteEditCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Budget, Budget }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeEditPage, parameters);
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

        public async Task ExecuteAddTransactionCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Envelope, Budget.Envelope }
            };

            await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
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

                    var accountsResult = await _accountLogic.Value.GetAccountsForSelectionAsync();
                    if (accountsResult.Success
                        && (Accounts == null || !Accounts.SequenceEqual(accountsResult.Data)))
                    {
                        Accounts.ReplaceRange(accountsResult.Data);
                    }
                    else if (!accountsResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), accountsResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                    }
                }

                Result<Budget> budgetResult;
                if (Budget.IsActive)
                {
                    budgetResult = await _envelopeLogic.Value.GetBudgetAsync(Budget.Id);
                }
                else
                {
                    budgetResult = await _envelopeLogic.Value.GetBudgetAsync(Budget.Envelope.Id, Budget.Schedule);
                }

                if (budgetResult.Success)
                {
                    Budget = budgetResult.Data;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), budgetResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }

                var result = await _transactionLogic.Value.GetEnvelopeTransactionsAsync(Budget.Envelope);
                if (result.Success
                    && (Transactions == null || !Transactions.SequenceEqual(result.Data)))
                {
                    Transactions.ReplaceRange(result.Data);
                }
                else if (!result.Success)
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }
                SelectedTransaction = null;

                NoTransactions = (Transactions?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExecuteRefreshTransactionCommand(Transaction transaction)
        {
            var transactions = Transactions.Where(t => t.Id != transaction.Id).ToList();
            if (transaction != null && transaction.IsActive)
            {
                transactions.Add(transaction);
            }

            Transactions.ReplaceRange(transactions);
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
                _eventAggregator.GetEvent<TransactionDeletedEvent>().Publish(transaction);

                _needToSync = true;
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteTransferCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Envelope, Budget.Envelope },
                { PageParameter.BudgetSchedule, Budget.Schedule }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeTransferPage, parameters);
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

                    Result<Budget> budgetResult;
                    if (Budget.IsActive)
                    {
                        budgetResult = await _envelopeLogic.Value.GetBudgetAsync(Budget.Id);
                    }
                    else
                    {
                        budgetResult = await _envelopeLogic.Value.GetBudgetAsync(Budget.Envelope.Id, Budget.Schedule);
                    }
                    
                    if (budgetResult.Success)
                    {
                        Budget = budgetResult.Data;
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), budgetResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                    }
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

        void UpdateTransactionStatus(Transaction transaction)
        {
            var transactions = Transactions.Where(t => t.Id == transaction.Id);
            foreach (var tran in transactions)
            {
                tran.Posted = transaction.Posted;
            }
        }

        void UpdateSplitTransactionStatus(Transaction transaction)
        {
            var transactions = Transactions.Where(t => t.SplitId == transaction.SplitId);
            foreach (var tran in transactions)
            {
                tran.Posted = transaction.Posted;
            }
        }
    }
}
