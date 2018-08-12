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

        public ICommand TogglePostedTransactionCommand { get; set; }
        public ICommand AddNewCommand { get; set; }
        public ICommand AddExistingCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
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
				RaisePropertyChanged(nameof(Total));
			}
        }

        public decimal Total
        {
            get => Transactions?.Sum(t => t.Amount ?? 0) ?? 0;
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
            AddExistingCommand = new DelegateCommand(async () => await ExecuteAddExistingCommand());
            EditCommand = new DelegateCommand<Transaction>(async a => await ExecuteEditCommand(a));
            RemoveCommand = new DelegateCommand<Transaction>(async a => await ExecuteRemoveCommand(a));
            DeleteCommand = new DelegateCommand<Transaction>(async a => await ExecuteDeleteCommand(a));
            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            TransactionSelectedCommand = new DelegateCommand(async () => await ExecuteTransactionSelectedCommand());
            TogglePostedTransactionCommand = new DelegateCommand<Transaction>(async t => await ExecuteTogglePostedTransaction(t));
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            if (SplitId == null)
            {
                SplitId = parameters.GetValue<Guid>(PageParameter.SplitTransactionId);
                if (SplitId.HasValue)
                {
                    var result = await _transLogic.GetTransactionsFromSplitAsync(SplitId.Value);
                    if (result.Success)
                    {
                        Transactions = result.Data;
                    }
                }
            }

            var transaction = parameters.GetValue<Transaction>(PageParameter.Transaction);
            if (transaction != null)
            {
                var transactionsToUpsert = new List<Transaction>();
                if (transaction.IsCombined)
                {
                    var result = await _transLogic.GetTransactionsFromSplitAsync(transaction.SplitId.Value);
                    if (result.Success)
                    {
                        transactionsToUpsert.AddRange(result.Data);
                    }
                }
                else
                {
                    transactionsToUpsert.Add(transaction.DeepCopy());
                }
                foreach (var tran in transactionsToUpsert)
                {
                    List<Transaction> tempTransactions = Transactions.Where(t => t.Id != tran.Id).ToList();
                    tempTransactions.Add(tran);
                    Transactions = tempTransactions;
                }
            }

			RaisePropertyChanged(nameof(Total));
        }

        public async Task ExecuteAddNewCommand()
        {
			if (Transactions.Count > 0)
			{
				//var envelope = Transactions.Last().Envelope; not needed normally
				var parameters = new NavigationParameters
                {
					{ PageParameter.Payee, Transactions.Last().Payee },
					{ PageParameter.Account, Transactions.Last().Account },
					{ PageParameter.TransactionServiceDate, Transactions.Last().ServiceDate }
                };
                await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
			}
			else
			{
				await _navigationService.NavigateAsync(PageName.TransactionEditPage);
			}
        }

        public async Task ExecuteAddExistingCommand()
        {
            await _navigationService.NavigateAsync(PageName.TransactionSelectionPage);
        }

        public async Task ExecuteEditCommand(Transaction transaction)
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Transaction, transaction }
            };
            await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
        }

        void RemoveTransaction(Transaction transaction)
        {
            var existingTransactions = Transactions.Where(t => t.Id != transaction.Id).ToList();
            Transactions = existingTransactions;

            RaisePropertyChanged(nameof(Total));
        }

        public async Task ExecuteRemoveCommand(Transaction transaction)
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
                    var result = await _transLogic.RemoveTransactionFromSplitAsync(transaction.Id);
                    if (!result.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Remove Unsuccessful", result.Message, "OK");
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

            RemoveTransaction(transaction);
        }

        public async Task ExecuteDeleteCommand(Transaction transaction)
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

            RemoveTransaction(transaction);
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
                var result = await _transLogic.SaveSplitTransactionAsync(Transactions.Select(t => t.Id));
                if (result.Success)
                {
					var syncTask = _syncService.FullSync();

                    await _navigationService.GoBackAsync();

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

        public async Task ExecuteTransactionSelectedCommand()
        {
            if (SelectedTransaction == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Transaction, SelectedTransaction }
            };

            await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);


            SelectedTransaction = null;
        }

        public async Task ExecuteTogglePostedTransaction(Transaction transaction)
        {
            if (transaction != null)
            {
                transaction.Posted = !transaction.Posted;

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
                    //var syncTask = _syncService.FullSync();

                    //var syncResult = await syncTask;
                    //if (!syncResult.Success)
                    //{
                    //    await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    //}
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
