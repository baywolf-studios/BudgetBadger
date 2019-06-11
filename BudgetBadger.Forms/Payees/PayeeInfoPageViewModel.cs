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

namespace BudgetBadger.Forms.Payees
{
    public class PayeeInfoPageViewModel : BindableBase, INavigationAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly ITransactionLogic _transactionLogic;
        readonly INavigationService _navigationService;
        readonly IPayeeLogic _payeeLogic;
        readonly IPageDialogService _dialogService;
        readonly ISyncFactory _syncFactory;
        readonly IAccountLogic _accountLogic;
        readonly IEnvelopeLogic _envelopeLogic;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand DeleteTransactionCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public Predicate<object> Filter { get => (t) => _transactionLogic.FilterTransaction((Transaction)t, SearchText); }

        bool _needToSync;

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

        IReadOnlyList<Account> _accounts;
        public IReadOnlyList<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
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

        public PayeeInfoPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
                                      ITransactionLogic transactionLogic,
                                      IPayeeLogic payeeLogic,
                                      IPageDialogService dialogService,
                                      ISyncFactory syncFactory,
                                      IAccountLogic accountLogic,
                                      IEnvelopeLogic envelopeLogic)
        {
            _resourceContainer = resourceContainer;
            _transactionLogic = transactionLogic;
            _navigationService = navigationService;
            _payeeLogic = payeeLogic;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _accountLogic = accountLogic;
            _envelopeLogic = envelopeLogic;

            Payee = new Payee();
            Transactions = new List<Transaction>();
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            DeleteTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteDeleteTransactionCommand(t));
            TransactionSelectedCommand = new DelegateCommand<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
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

        public async void OnNavigatingTo(INavigationParameters parameters)
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
                if (Payee.IsActive)
                {
                    if (Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet)
                    {
                        var accountsResult = await _accountLogic.GetAccountsForSelectionAsync();
                        if (accountsResult.Success)
                        {
                            Accounts = accountsResult.Data;
                        }
                        else
                        {
                            await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), accountsResult.Message, _resourceContainer.GetResourceString("AlertOk"));
                        }

                        var payeesResult = await _payeeLogic.GetPayeesForSelectionAsync();
                        if (payeesResult.Success)
                        {
                            Payees = payeesResult.Data;
                        }
                        else
                        {
                            await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), payeesResult.Message, _resourceContainer.GetResourceString("AlertOk"));
                        }

                        var envelopesResult = await _envelopeLogic.GetEnvelopesForSelectionAsync();
                        if (envelopesResult.Success)
                        {
                            Envelopes = envelopesResult.Data;
                        }
                        else
                        {
                            await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), envelopesResult.Message, _resourceContainer.GetResourceString("AlertOk"));
                        }
                    }

                    var payeeResult = await _payeeLogic.GetPayeeAsync(Payee.Id);
                    if (payeeResult.Success)
                    {
                        Payee = payeeResult.Data;
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), payeeResult.Message, _resourceContainer.GetResourceString("AlertOk"));
                    }

                    var result = await _transactionLogic.GetPayeeTransactionsAsync(Payee);
                    if (result.Success)
                    {
                        Transactions = result.Data;
                        SelectedTransaction = null;
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
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
                    _needToSync = true;
                }
                else
                {
                    transaction.Posted = !transaction.Posted;
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
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
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
            }
        }
    }
}
