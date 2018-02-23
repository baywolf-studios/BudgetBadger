using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using BudgetBadger.Core.Extensions;
using Prism.Mvvm;
using BudgetBadger.Core.Sync;

namespace BudgetBadger.Forms.Transactions
{
    public class TransactionPageViewModel : BindableBase, INavigationAware
    {
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ITransactionLogic _transLogic;
        readonly ISync _syncService;

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        string _busyText;
        public string BusyText
        {
            get { return _busyText; }
            set { SetProperty(ref _busyText, value); }
        }

        Transaction _transaction;
        public Transaction Transaction
        {
            get { return _transaction; }
            set { SetProperty(ref _transaction, value); }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand PayeeSelectedCommand { get; set; }
        public ICommand EnvelopeSelectedCommand { get; set; }
        public ICommand AccountSelectedCommand { get; set; }

        public TransactionPageViewModel(INavigationService navigationService,
                                        IPageDialogService dialogService,
                                        ITransactionLogic transLogic,
                                        ISync syncService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _transLogic = transLogic;
            _syncService = syncService;

            Transaction = new Transaction();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            PayeeSelectedCommand = new DelegateCommand(async () => await ExecutePayeeSelectedCommand());
            EnvelopeSelectedCommand = new DelegateCommand(async () => await ExecuteEnvelopeSelectedCommand(), CanExecuteEnvelopeSelectedCommand).ObservesProperty(() => Transaction.Envelope);
            AccountSelectedCommand = new DelegateCommand(async () => await ExecuteAccountSelectedCommand());
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            var transaction = parameters.GetValue<Transaction>(PageParameter.Transaction);
            if (transaction != null)
            {
                Transaction = transaction.DeepCopy();
            }

            var transactionAmount = parameters.GetValue<decimal?>(PageParameter.TransactionAmount);
            if (transactionAmount != null)
            {
                Transaction.Amount = transactionAmount.Value;
            }

            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                Transaction.Account = account.DeepCopy();
            }

            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                Transaction.Payee = payee.DeepCopy();
            }

            var envelope = parameters.GetValue<Envelope>(PageParameter.Envelope);
            if (envelope != null)
            {
                Transaction.Envelope = envelope.DeepCopy();
            }

            var result = await _transLogic.GetCorrectedTransaction(Transaction);
            if (result.Success)
            {
                Transaction = result.Data;
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Error", result.Message, "Okay");
                await _navigationService.GoBackAsync();
            }
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
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
                BusyText = "Saving";
                var result = await _transLogic.SaveTransactionAsync(Transaction);

                if (result.Success)
                {

                    BusyText = "Syncing";
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

        public async Task ExecutePayeeSelectedCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.SelectorMode, true }
            };
            await _navigationService.NavigateAsync(PageName.PayeesPage, parameters);
        }

        public async Task ExecuteEnvelopeSelectedCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.SelectorMode, true }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopesPage, parameters);
        }

        public bool CanExecuteEnvelopeSelectedCommand()
        {
            return !Transaction.Envelope.IsSystem();
        }

        public async Task ExecuteAccountSelectedCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.SelectorMode, true }
            };
            await _navigationService.NavigateAsync(PageName.AccountsPage, parameters);
        }
    }
}
