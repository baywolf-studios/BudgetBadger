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

namespace BudgetBadger.Forms.Transactions
{
    public class SplitTransactionPageViewModel : BindableBase, INavigatingAware
    {
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ITransactionLogic _transLogic;
		readonly ISync _syncService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
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

        IReadOnlyList<Transaction> _transactions;
        public IReadOnlyList<Transaction> Transactions
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

        public SplitTransactionPageViewModel(INavigationService navigationService,
                                             IPageDialogService dialogService,
                                             ITransactionLogic transLogic,
		                                     ISync syncService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _transLogic = transLogic;
			_syncService = syncService;

            Transactions = new List<Transaction>();

            AddNewCommand = new DelegateCommand(async () => await ExecuteAddNewCommand());
            DeleteTransactionCommand = new DelegateCommand<Transaction>(async a => await ExecuteDeleteTransactionCommand(a));
            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            TransactionSelectedCommand = new DelegateCommand<Transaction>(async t => await ExecuteTransactionSelectedCommand(t));
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            if (SplitId == null)
            {
                SplitId = parameters.GetValue<Guid?>(PageParameter.SplitTransactionId);
                if (SplitId.HasValue)
                {
                    var result = await _transLogic.GetTransactionsFromSplitAsync(SplitId.Value);
                    if (result.Success)
                    {
                        Transactions = result.Data;
                        Total = RunningTotal;
                        NoTransactions = (Transactions?.Count ?? 0) == 0;
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
                Transactions = tempTransactions;
                NoTransactions = (Transactions?.Count ?? 0) == 0;
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

                    var transResult = await _transLogic.GetCorrectedTransaction(tempTrans);

                    if (transResult.Success)
                    {
                        var split1 = transResult.Data.DeepCopy();
                        if (split1.Id == Guid.Empty)
                        {
                            split1.Id = Guid.NewGuid();
                        }
                        var split2 = split1.DeepCopy();
                        split2.Id = Guid.NewGuid();

                        Transactions = new List<Transaction>
                        {
                            split1,
                            split2
                        };
                    }
                }
                NoTransactions = (Transactions?.Count ?? 0) == 0;
                return;
            }

            var deletedTransaction = parameters.GetValue<Transaction>(PageParameter.DeletedTransaction);
            if (deletedTransaction != null)
            {
                Transactions = Transactions.Where(t => t.Id != deletedTransaction.Id).ToList();
                NoTransactions = (Transactions?.Count ?? 0) == 0;
                return;
            }

            Transactions = Transactions.ToList();
            NoTransactions = (Transactions?.Count ?? 0) == 0;
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

            await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
        }

        public async Task ExecuteDeleteTransactionCommand(Transaction transaction)
        {
            if (transaction.IsActive)
            {
                if (!IsBusy)
                {
                    IsBusy = true;
                }
                
                try
                {
                    var result = await _transLogic.DeleteTransactionAsync(transaction.Id);

                    if (!result.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
                        return;
                    }  

					var syncResult = await _syncService.FullSync();            
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    } 
                }
                finally
                {
                    IsBusy = false;
                }
            }

            var existingTransactions = Transactions.Where(t => t.Id != transaction.Id).ToList();
            Transactions = existingTransactions;
            NoTransactions = (Transactions?.Count ?? 0) == 0;
        }

        public async Task ExecuteSaveCommand()
        {
            if (!IsBusy)
            {
                IsBusy = true;
            }

            try
            {
                var result = await _transLogic.SaveSplitTransactionAsync(Transactions);
                if (result.Success)
                {
					var syncTask = _syncService.FullSync();

                    await _navigationService.GoBackToRootAsync();

					var syncResult = await syncTask;
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }  
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
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

            await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
        }

        public async Task ExecuteTogglePostedTransaction(Transaction transaction)
        {
            if (transaction != null && !transaction.Reconciled)
            {
                transaction.Posted = !transaction.Posted;

                if (transaction.IsActive)
                {
                    Result result = new Result();

                    if (transaction.IsCombined)
                    {
                        result = await _transLogic.UpdateSplitTransactionPostedAsync(transaction.SplitId.Value, transaction.Posted);
                    }
                    else
                    {
                        result = await _transLogic.SaveTransactionAsync(transaction);
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
}
