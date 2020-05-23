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
using BudgetBadger.Models.Extensions;
using BudgetBadger.Core.LocalizedResources;
using Xamarin.Forms;
using BudgetBadger.Core.Purchase;
using Prism.Events;
using BudgetBadger.Forms.Events;

namespace BudgetBadger.Forms.Payees
{
    public class PayeeInfoPageViewModel : ObservableBase, INavigationAware, IInitializeAsync
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<ITransactionLogic> _transactionLogic;
        readonly INavigationService _navigationService;
        readonly Lazy<IPayeeLogic> _payeeLogic;
        readonly IPageDialogService _dialogService;
        readonly Lazy<ISyncFactory> _syncFactory;
        readonly Lazy<IAccountLogic> _accountLogic;
        readonly Lazy<IEnvelopeLogic> _envelopeLogic;
        readonly Lazy<IPurchaseService> _purchaseService;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand DeleteTransactionCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public ICommand SaveTransactionCommand { get; set; }
        public ICommand RefreshTransactionCommand { get; set; }
        public Predicate<object> Filter { get => (t) => _transactionLogic.Value.FilterTransaction((Transaction)t, SearchText); }

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

        ObservableList<Account> _accounts;
        public ObservableList<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
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

        bool _hasPro;
        public bool HasPro
        {
            get => _hasPro;
            set => SetProperty(ref _hasPro, value);
        }

        bool _fullRefresh = true;

        public PayeeInfoPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                      INavigationService navigationService,
                                      Lazy<ITransactionLogic> transactionLogic,
                                      Lazy<IPayeeLogic> payeeLogic,
                                      IPageDialogService dialogService,
                                      Lazy<ISyncFactory> syncFactory,
                                      Lazy<IAccountLogic> accountLogic,
                                      Lazy<IEnvelopeLogic> envelopeLogic,
                                      Lazy<IPurchaseService> purchaseService,
                                      IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _transactionLogic = transactionLogic;
            _navigationService = navigationService;
            _payeeLogic = payeeLogic;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _accountLogic = accountLogic;
            _envelopeLogic = envelopeLogic;
            _purchaseService = purchaseService;
            _eventAggregator = eventAggregator;

            Payee = new Payee();
            Transactions = new ObservableList<Transaction>();
            Accounts = new ObservableList<Account>();
            Envelopes = new ObservableList<Envelope>();
            SelectedTransaction = null;

            EditCommand = new Command(async () => await ExecuteEditCommand());
            DeleteTransactionCommand = new Command<Transaction>(async t => await ExecuteDeleteTransactionCommand(t));
            TransactionSelectedCommand = new Command<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
            RefreshCommand = new Command(async () => await FullRefresh());
            AddTransactionCommand = new Command(async () => await ExecuteAddTransactionCommand());
            TogglePostedTransactionCommand = new Command<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
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

            _eventAggregator.GetEvent<BudgetSavedEvent>().Subscribe(b => RefreshEnvelope(b?.Envelope));
            _eventAggregator.GetEvent<EnvelopeDeletedEvent>().Subscribe(RefreshEnvelope);
            _eventAggregator.GetEvent<EnvelopeHiddenEvent>().Subscribe(RefreshEnvelope);
            _eventAggregator.GetEvent<EnvelopeUnhiddenEvent>().Subscribe(RefreshEnvelope);
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

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                Payee = payee.DeepCopy();
            }

            var purchasedPro = await _purchaseService.Value.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;
        }

        public async Task ExecuteEditCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Payee, Payee }
            };
            await _navigationService.NavigateAsync(PageName.PayeeEditPage, parameters);
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
                { PageParameter.Payee, Payee }
            };

            await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
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
                if (Payee.IsActive)
                {
                    if (Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet)
                    {
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

                    var payeeResult = await _payeeLogic.Value.GetPayeeAsync(Payee.Id);
                    if (payeeResult.Success)
                    {
                        Payee = payeeResult.Data;
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), payeeResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                    }

                    var result = await _transactionLogic.Value.GetPayeeTransactionsAsync(Payee);
                    if (result.Success
                        && (Transactions == null || !Transactions.SequenceEqual(result.Data)))
                    {
                        Transactions.ReplaceRange(result.Data);
                    }
                    else if (!result.Success)
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                    }

                    await RefreshSummary();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task RefreshSummary()
        {
            var payeeResult = await _payeeLogic.Value.GetPayeeAsync(Payee.Id);
            if (payeeResult.Success)
            {
                RefreshPayee(payeeResult.Data);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), payeeResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }

            NoTransactions = (Transactions?.Count ?? 0) == 0;

            RaisePropertyChanged();
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
            Payee = payee;

            foreach (var transaction in Transactions.Where(t => t.Payee.Id == payee.Id))
            {
                transaction.Payee = payee;
            }
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
            foreach (var transaction in Transactions.Where(t => t.Account.Id == account.Id))
            {
                transaction.Account = account;
            }

            var accounts = Accounts.Where(a => a.Id != account.Id).ToList();

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
