﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace BudgetBadger.Forms.Transactions
{
    public class SplitTransactionPageViewModel : BindableBase, INavigatingAware
    {
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ITransactionLogic _transLogic;

        public ICommand AddNewCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        ObservableCollection<Transaction> _transactions;
        public ObservableCollection<Transaction> Transactions
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

        public SplitTransactionPageViewModel(INavigationService navigationService,
                                             IPageDialogService dialogService,
                                             ITransactionLogic transLogic)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _transLogic = transLogic;

            Transactions = new ObservableCollection<Transaction>();
            SelectedTransaction = null;

            AddNewCommand = new DelegateCommand(async () => await ExecuteAddNewCommand());
            EditCommand = new DelegateCommand<Transaction>(async a => await ExecuteEditCommand(a));
            DeleteCommand = new DelegateCommand<Transaction>(async a => await ExecuteDeleteCommand(a));
            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            var transactions = new List<Transaction>();

            var splitTransaction = parameters.GetValue<IEnumerable<Transaction>>(PageParameter.SplitTransaction);
            if (splitTransaction != null)
            {
                transactions.AddRange(splitTransaction);
            }

            var trans = parameters.GetValue<Transaction>(PageParameter.Transaction);
            if (trans != null)
            {
                transactions.Add(trans);
            }

            foreach (var transaction in transactions)
            {
                if (Transactions.Any(t => t.Id == transaction.Id))
                {
                    var existingTransaction = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
                    Transactions.Remove(existingTransaction);
                }

                Transactions.Add(transaction.DeepCopy());
            }
        }

        public async Task ExecuteAddNewCommand()
        {
            await _navigationService.NavigateAsync(PageName.TransactionPage);
        }

        public async Task ExecuteEditCommand(Transaction transaction)
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Transaction, transaction }
            };
            await _navigationService.NavigateAsync(PageName.TransactionPage, parameters);
        }

        async Task RemoveTransaction(Transaction transaction)
        {
            var existingTransaction = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (!Transactions.Remove(existingTransaction))
            {
                await _dialogService.DisplayAlertAsync("Delete Unsuccessful", "Transaction does not exist", "OK");
            }
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
                }
                finally
                {
                    IsBusy = false;
                }

            }

            await RemoveTransaction(transaction);
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
                }
                finally
                {
                    IsBusy = false;
                }
            }

            await RemoveTransaction(transaction);
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
                    await _navigationService.GoBackAsync();
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
    }
}
