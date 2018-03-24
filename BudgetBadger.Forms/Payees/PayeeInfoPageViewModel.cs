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

namespace BudgetBadger.Forms.Payees
{
    public class PayeeInfoPageViewModel : BindableBase, INavigatingAware
    {
        readonly ITransactionLogic _transactionLogic;
        readonly INavigationService _navigationService;
        readonly IPayeeLogic _payeeLogic;
        readonly IPageDialogService _dialogService;

        public ICommand EditCommand { get; set; }
        public ICommand TransactionSelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NewTransactionCommand { get; set; }

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

        IEnumerable<Transaction> _transactions;
        public IEnumerable<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        IReadOnlyList<IGrouping<string, Transaction>> _groupedTransactions;
        public IReadOnlyList<IGrouping<string, Transaction>> GroupedTransactions
        {
            get => _groupedTransactions;
            set => SetProperty(ref _groupedTransactions, value);
        }

        Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        public decimal LifetimeSpent { get => Transactions.Sum(t => t.Amount); }

        public PayeeInfoPageViewModel(INavigationService navigationService, ITransactionLogic transactionLogic, IPayeeLogic payeeLogic, IPageDialogService dialogService)
        {
            _transactionLogic = transactionLogic;
            _navigationService = navigationService;
            _payeeLogic = payeeLogic;
            _dialogService = dialogService;

            Payee = new Payee();
            Transactions = new List<Transaction>();
            GroupedTransactions = Transactions.GroupBy(t => "").ToList();
            SelectedTransaction = null;

            EditCommand = new DelegateCommand(async () => await ExecuteEditCommand());
            TransactionSelectedCommand = new DelegateCommand(async () => await ExecuteTransactionSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            NewTransactionCommand = new DelegateCommand(async () => await ExecuteNewTransactionCommand());
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
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

        public async Task ExecuteTransactionSelectedCommand()
        {
            if (SelectedTransaction == null)
            {
                return;
            }

            if (SelectedTransaction.IsSplit)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.SplitTransactionId, SelectedTransaction.SplitId }
                };
                await _navigationService.NavigateAsync(PageName.SplitTransactionPage, parameters);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Transaction, SelectedTransaction }
                };
                await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
            }

            SelectedTransaction = null;
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
                    var payeeResult = await _payeeLogic.GetPayeeAsync(Payee.Id);
                    if (payeeResult.Success)
                    {
                        Payee = payeeResult.Data;
                    }
                    else
                    {
                        //show alert that account data may be stale
                    }

                    var result = await _transactionLogic.GetPayeeTransactionsAsync(Payee);
                    if (result.Success)
                    {
                        Transactions = result.Data;
                        GroupedTransactions = _transactionLogic.GroupTransactions(Transactions);
                        SelectedTransaction = null;
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteNewTransactionCommand()
        {
            


            var simpleAction = ActionSheetButton.CreateButton("Simple", async () =>
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Payee, Payee }
                };
                await _navigationService.NavigateAsync(PageName.TransactionEditPage, parameters);
            });

            var splitAction = ActionSheetButton.CreateButton("Split", async () =>
            {
                await _navigationService.NavigateAsync(PageName.SplitTransactionPage);
            });

            var cancelAction = ActionSheetButton.CreateCancelButton("Cancel", () => { });

            await _dialogService.DisplayActionSheetAsync("New Transaction", simpleAction, splitAction, cancelAction);
        }
    }
}
