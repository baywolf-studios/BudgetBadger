using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Events;
using BudgetBadger.Core.Models;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeInfoPageViewModel : ObservableBase, INavigationAware, IInitialize
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<ITransactionLogic> _transactionLogic;
        readonly INavigationService _navigationService;
        readonly Lazy<IEnvelopeLogic> _envelopeLogic;
        readonly IPageDialogService _dialogService;
        readonly Lazy<IAccountLogic> _accountLogic;
        readonly Lazy<IPayeeLogic> _payeeLogic;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
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

        bool _fullRefresh = true;

        public EnvelopeInfoPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                         INavigationService navigationService,
                                         Lazy<ITransactionLogic> transactionLogic,
                                         Lazy<IEnvelopeLogic> envelopeLogic,
                                         IPageDialogService dialogService,
                                         Lazy<IAccountLogic> accountLogic,
                                         Lazy<IPayeeLogic> payeeLogic,
                                         IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _transactionLogic = transactionLogic;
            _navigationService = navigationService;
            _envelopeLogic = envelopeLogic;
            _dialogService = dialogService;
            _accountLogic = accountLogic;
            _payeeLogic = payeeLogic;
            _eventAggregator = eventAggregator;

            Budget = new Budget();
            Transactions = new ObservableList<Transaction>();
            Accounts = new ObservableList<Account>();
            Payees = new ObservableList<Payee>();
            SelectedTransaction = null;

            EditCommand = new Command(async () => await ExecuteEditCommand());
            DeleteTransactionCommand = new Command<Transaction>(async t => await ExecuteDeleteTransactionCommand(t));
            TransactionSelectedCommand = new Command<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
            AddTransactionCommand = new Command(async () => await ExecuteAddTransactionCommand());
            RefreshCommand = new Command(async () => await FullRefresh());
            TogglePostedTransactionCommand = new Command<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
            TransferCommand = new Command(async () => await ExecuteTransferCommand());
            SaveTransactionCommand = new Command<Transaction>(async t => await ExecuteSaveTransactionCommand(t));
            RefreshTransactionCommand = new Command<Transaction>(RefreshTransaction);

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

            _eventAggregator.GetEvent<BudgetSavedEvent>().Subscribe(RefreshBudget);
            _eventAggregator.GetEvent<EnvelopeDeletedEvent>().Subscribe(async e => await RefreshEnvelope(e));
            _eventAggregator.GetEvent<EnvelopeHiddenEvent>().Subscribe(async e => await RefreshEnvelope(e));
            _eventAggregator.GetEvent<EnvelopeUnhiddenEvent>().Subscribe(async e => await RefreshEnvelope(e));
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

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedTransaction = null;
        }

        public void Initialize(INavigationParameters parameters)
        {
            var budget = parameters.GetValue<Budget>(PageParameter.Budget);
            if (budget != null)
            {
                Budget = budget.DeepCopy();
            }
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

        public async Task FullRefresh()
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

                await RefreshSummary();
            }
            finally
            {
                IsBusy = false;
            }
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
        
        public async Task RefreshSummary()
        {
            Result<BudgetSchedule> scheduleResult;
            if (Budget.Schedule != null)
            {
                scheduleResult = await _envelopeLogic.Value.GetBudgetSchedule(Budget.Schedule);
            }
            else
            {
                scheduleResult = await _envelopeLogic.Value.GetCurrentBudgetScheduleAsync();
            }

            if (scheduleResult.Success)
            {
                var budgetResult = await _envelopeLogic.Value.GetBudgetAsync(Budget.Envelope.Id, scheduleResult.Data);
                if (budgetResult.Success)
                {
                    RefreshBudget(budgetResult.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), budgetResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), scheduleResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }

            NoTransactions = (Transactions?.Count ?? 0) == 0;

            RaisePropertyChanged(nameof(Budget));
            RaisePropertyChanged(nameof(Budget.Schedule));
            RaisePropertyChanged(nameof(Budget.Schedule.Past));
            RaisePropertyChanged(nameof(Budget.Schedule.Income));
            RaisePropertyChanged(nameof(Budget.Schedule.ToBudget));
            RaisePropertyChanged(nameof(Budget.Schedule.Overspend));
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
            foreach(var transaction in Transactions.Where(t => t.Payee.Id == payee.Id))
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

        public void RefreshBudget(Budget budget)
        {
            Budget = budget;

            foreach (var transaction in Transactions.Where(t => t.Envelope.Id == budget.Envelope.Id))
            {
                transaction.Envelope = budget.Envelope;
            }
        }

        public async Task RefreshEnvelope(Envelope envelope)
        {
            var budgetResult = await _envelopeLogic.Value.GetBudgetAsync(envelope.Id, Budget.Schedule);
            if (budgetResult.Success)
            {
                RefreshBudget(budgetResult.Data);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), budgetResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public void RefreshAccount(Account account)
        {
            foreach (var transaction in Transactions.Where(t => t.Account.Id == account.Id))
            {
                transaction.Account = account;
            }

            var accounts = Accounts.Where(p => p.Id != account.Id).ToList();

            if (account != null && account.IsActive)
            {
                accounts.Add(account);
            }

            Accounts.ReplaceRange(accounts);
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
    }
}
