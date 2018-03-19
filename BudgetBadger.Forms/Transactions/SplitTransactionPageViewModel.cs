using System;
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

        public ICommand RefreshCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

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

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<Transaction>(async a => await ExecuteEditCommand(a));
            DeleteCommand = new DelegateCommand<Transaction>(async a => await ExecuteDeleteCommand(a));
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            //splitTransaction = parameters.GetValue<IEnumerable<Transaction>>(PageParameter.SplitTransaction);
            //case when an existing split is opened

            var transaction = parameters.GetValue<Transaction>(PageParameter.Transaction);

            if (transaction != null)
            {
                if (Transactions.Any(t => t.Id == transaction.Id))
                {
                    var existingTransaction = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
                    Transactions.Remove(existingTransaction);
                }

                Transactions.Add(transaction.DeepCopy());
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
                if (SplitId.HasValue)
                {
                    var result = await _transLogic.GetSplitTransactionsAsync(SplitId.Value);
                    if (result.Success)
                    {
                        Transactions = new ObservableCollection<Transaction>(result.Data);
                    }
                    else
                    {
                        //show error
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteAddCommand()
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

        public async Task ExecuteDeleteCommand(Transaction transaction)
        {
            if (transaction.IsActive)
            {
                var result = await _transLogic.DeleteTransactionAsync(transaction.Id);

                if (result.Success)
                {
                    await ExecuteRefreshCommand();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
                }
            }
            else
            {
                var existingTransaction = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
                if (!Transactions.Remove(existingTransaction))
                {
                    await _dialogService.DisplayAlertAsync("Delete Unsuccessful", "Transaction does not exist", "OK");
                }
            }
        }
    }
}
