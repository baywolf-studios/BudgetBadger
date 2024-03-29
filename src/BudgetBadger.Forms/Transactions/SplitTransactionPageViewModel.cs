﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Events;
using BudgetBadger.Models;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Transactions
{
    public class SplitTransactionPageViewModel : ObservableBase, INavigationAware, IInitializeAsync
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly Lazy<ITransactionLogic> _transLogic;
        readonly Lazy<IEnvelopeLogic> _envelopeLogic;
        readonly Lazy<IAccountLogic> _accountLogic;
        readonly Lazy<IPayeeLogic> _payeeLogic;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand AddNewCommand { get; set; }
        public ICommand DeleteTransactionCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        Guid? _splitId;
        public Guid? SplitId
        {
            get => _splitId;
            set => SetProperty(ref _splitId, value);
        }

        Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        ObservableList<Account> _accounts;
        public ObservableList<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
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
            set
            {
                SetProperty(ref _transactions, value);
                RaisePropertyChanged(nameof(RunningTotal));
                RaisePropertyChanged(nameof(Remaining));
            }
        }

        public decimal RunningTotal
        {
            get => Transactions?.Sum(t => t.Amount ?? 0) ?? 0;
        }

        decimal total;
        public decimal Total
        {
            get => total;
            set
            {
                SetProperty(ref total, value);
                RaisePropertyChanged(nameof(Remaining));
            }
        }

        public decimal Remaining
        {
            get => Total - RunningTotal;
        }

        bool _noTransactions;
        public bool NoTransactions
        {
            get => _noTransactions;
            set => SetProperty(ref _noTransactions, value);
        }

        public SplitTransactionPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                             INavigationService navigationService,
                                             IPageDialogService dialogService,
                                             Lazy<ITransactionLogic> transLogic,
                                             Lazy<IAccountLogic> accountLogic,
                                             Lazy<IEnvelopeLogic> envelopeLogic,
                                             Lazy<IPayeeLogic> payeeLogic,
                                             IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _transLogic = transLogic;
            _accountLogic = accountLogic;
            _envelopeLogic = envelopeLogic;
            _payeeLogic = payeeLogic;
            _eventAggregator = eventAggregator;

            Transactions = new ObservableList<Transaction>();
            Accounts = new ObservableList<Account>();
            Payees = new ObservableList<Payee>();
            Envelopes = new ObservableList<Envelope>();

            AddNewCommand = new Command(async () => await ExecuteAddNewCommand());
            DeleteTransactionCommand = new Command<Transaction>(async a => await ExecuteDeleteTransactionCommand(a));
            SaveCommand = new Command(async () => await ExecuteSaveCommand());
            TransactionSelectedCommand = new Command<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
            TogglePostedTransactionCommand = new Command<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            await FullRefresh();

            if (SplitId == null)
            {
                SplitId = parameters.GetValue<Guid?>(PageParameter.SplitTransactionId);
                if (SplitId.HasValue)
                {
                    var result = await _transLogic.Value.GetTransactionsFromSplitAsync(SplitId.Value);
                    if (result.Success)
                    {
                        Transactions.ReplaceRange(result.Data);
                        Total = RunningTotal;
                        RefreshSummary();
                        return;
                    }
                }
            }

            var transaction = parameters.GetValue<Transaction>(PageParameter.Transaction);
            if (transaction != null)
            {
                if (transaction.Id == Guid.Empty)
                {
                    transaction.Id = Guid.NewGuid();
                }
                List<Transaction> tempTransactions = Transactions.Where(t => t.Id != transaction.Id).ToList();
                tempTransactions.Add(transaction);
                tempTransactions.Sort();
                Transactions.ReplaceRange(tempTransactions);
                RefreshSummary();
                return;
            }

            var initialSplitTransaction = parameters.GetValue<Transaction>(PageParameter.InitialSplitTransaction);
            if (initialSplitTransaction != null)
            {
                Total = initialSplitTransaction.Amount ?? 0;

                if (initialSplitTransaction.Account.IsActive || initialSplitTransaction.Payee.IsActive)
                {

                    var tempTrans = initialSplitTransaction.DeepCopy();

                    tempTrans.Amount = null;
                    tempTrans.Envelope = new Envelope();

                    var transResult = await _transLogic.Value.GetCorrectedTransaction(tempTrans);

                    if (transResult.Success)
                    {
                        var split1 = transResult.Data.DeepCopy();
                        if (split1.Id == Guid.Empty)
                        {
                            split1.Id = Guid.NewGuid();
                        }
                        var split2 = split1.DeepCopy();
                        split2.Id = Guid.NewGuid();

                        var splits = new List<Transaction> { split1, split2 };
                        splits.Sort();
                        Transactions.ReplaceRange(splits);
                    }
                }
                RefreshSummary();
                return;
            }

            var deletedTransaction = parameters.GetValue<Transaction>(PageParameter.DeletedTransaction);
            if (deletedTransaction != null)
            {
                var tempTran5 = Transactions.Where(t => t.Id != deletedTransaction.Id);
                Transactions.ReplaceRange(tempTran5);
                RefreshSummary();
                return;
            }

            RefreshSummary();
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedTransaction = null;
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back)
            {
                await InitializeAsync(parameters);
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

                    RefreshSummary();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void RefreshSummary()
        {
            NoTransactions = (Transactions?.Count ?? 0) == 0;

            RaisePropertyChanged(nameof(RunningTotal));
            RaisePropertyChanged(nameof(Remaining));
        }

        public async Task ExecuteAddNewCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.SplitTransactionMode, true }
            };

            if (Transactions.Count > 0)
            {
                parameters.Add(PageParameter.Payee, Transactions.Last().Payee);
                parameters.Add(PageParameter.Account, Transactions.Last().Account);
                parameters.Add(PageParameter.TransactionServiceDate, Transactions.Last().ServiceDate);
            }

            if (Transactions.All(t2 => t2.Amount.HasValue && t2.Amount != 0) && Remaining != 0)
            {
                parameters.Add(PageParameter.TransactionAmount, Remaining);
            }

            await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
        }

        public async Task ExecuteDeleteTransactionCommand(Transaction transaction)
        {
            if (transaction.IsActive)
            {
                if (IsBusy)
                {
                    return;
                }

                IsBusy = true;

                try
                {
                    var result = await _transLogic.Value.SoftDeleteTransactionAsync(transaction.Id);

                    if (!result.Success)
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                        return;
                    }

                    _eventAggregator.GetEvent<TransactionDeletedEvent>().Publish(result.Data);
                }
                finally
                {
                    IsBusy = false;
                }
            }

            var existingTransactions = Transactions.ToList();
            existingTransactions.Remove(transaction);
            Transactions.ReplaceRange(existingTransactions);
            RefreshSummary();
        }

        public async Task ExecuteSaveCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var result = await _transLogic.Value.SaveSplitTransactionAsync(Transactions);
                if (result.Success)
                {
                    _eventAggregator.GetEvent<SplitTransactionSavedEvent>().Publish();

                    var param = new NavigationParameters
                    {
                        { PageParameter.GoBack, true }
                    };
                    await _navigationService.GoBackAsync(param);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteTransactionSelectedCommand(Transaction transaction)
        {
            if (transaction == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Transaction, transaction },
                { PageParameter.SplitTransactionMode, true }
            };

            if (Remaining != 0
                && (!transaction.Amount.HasValue || transaction.Amount == 0)
                && Transactions.Where(t => t.Id != transaction.Id).All(t2 => t2.Amount.HasValue && t2.Amount != 0))
            {
                parameters.Add(PageParameter.TransactionAmount, Remaining);
            }

            await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
        }

        public async Task ExecuteTogglePostedTransaction(Transaction transaction)
        {
            if (transaction != null && !transaction.Reconciled)
            {
                transaction.Posted = !transaction.Posted;

                if (transaction.IsActive)
                {
                    Result result;

                    if (transaction.IsCombined)
                    {
                        result = await _transLogic.Value.UpdateSplitTransactionPostedAsync(transaction.SplitId.Value, transaction.Posted);
                    }
                    else
                    {
                        result = await _transLogic.Value.SaveTransactionAsync(transaction);
                    }

                    if (result.Success)
                    {
                        if (result is Result<Transaction> tranResult)
                            _eventAggregator.GetEvent<TransactionStatusUpdatedEvent>().Publish(tranResult.Data);
                        else
                            _eventAggregator.GetEvent<SplitTransactionStatusUpdatedEvent>().Publish(transaction);

                        RefreshSummary();
                    }
                    else
                    {
                        transaction.Posted = !transaction.Posted;
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                    }
                }
            }
        }
    }
}
