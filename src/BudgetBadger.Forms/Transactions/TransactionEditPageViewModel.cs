using System;
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

namespace BudgetBadger.Forms.Transactions
{
    public class TransactionEditPageViewModel : ObservableBase, INavigationAware, IInitializeAsync
    {
        readonly IResourceContainer _resourceContainer;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ITransactionLogic _transLogic;
        readonly IEventAggregator _eventAggregator;

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        string _busyText;
        public string BusyText
        {
            get => _busyText;
            set => SetProperty(ref _busyText, value);
        }

        Transaction _transaction;
        public Transaction Transaction
        {
            get => _transaction;
            set => SetProperty(ref _transaction, value);
        }

        bool _splitTransactionMode;
        public bool SplitTransactionMode
        {
            get => _splitTransactionMode;
            set => SetProperty(ref _splitTransactionMode, value);
        }

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand SaveCommand { get; set; }
        public ICommand PayeeSelectedCommand { get; set; }
        public ICommand EnvelopeSelectedCommand { get; set; }
        public ICommand AccountSelectedCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SplitCommand { get; set; }
        public ICommand TogglePostedTransactionCommand { get; set; }

        public TransactionEditPageViewModel(IResourceContainer resourceContainer,
                                        INavigationService navigationService,
                                        IPageDialogService dialogService,
                                        ITransactionLogic transLogic,
                                        IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _transLogic = transLogic;
            _eventAggregator = eventAggregator;

            Transaction = new Transaction();

            SaveCommand = new Command(async () => await ExecuteSaveCommand());
            PayeeSelectedCommand = new Command(async () => await ExecutePayeeSelectedCommand());
            EnvelopeSelectedCommand = new Command(async () => await ExecuteEnvelopeSelectedCommand(), CanExecuteEnvelopeSelectedCommand);
            AccountSelectedCommand = new Command(async () => await ExecuteAccountSelectedCommand());
            DeleteCommand = new Command(async () => await ExecuteDeleteCommand());
            SplitCommand = new Command(async () => await ExecuteSplitCommand());
            TogglePostedTransactionCommand = new Command<Transaction>(ExecuteTogglePostedTransaction);
        }

		public async Task InitializeAsync(INavigationParameters parameters)
		{
            var goBack = parameters.GetValue<bool>(PageParameter.GoBack);
            if (goBack)
            {
                await _navigationService.GoBackAsync();
                return;
            }

            if (!SplitTransactionMode)
            {
                SplitTransactionMode = parameters.GetValue<bool>(PageParameter.SplitTransactionMode);
                RaisePropertyChanged(nameof(SplitTransactionMode));
            }

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

			var transactionServiceDate = parameters.GetValue<DateTime?>(PageParameter.TransactionServiceDate);
			if (transactionServiceDate != null)
			{
				Transaction.ServiceDate = transactionServiceDate.Value;
			}

            var account = parameters.GetValue<AccountModel>(PageParameter.Account);
            if (account != null)
            {
                Transaction.Account = account.DeepCopy();
            }

            var payee = parameters.GetValue<PayeeModel>(PageParameter.Payee);
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
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertGeneralError"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                await _navigationService.GoBackAsync();
            }
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back)
            {
                await InitializeAsync(parameters);
            }
        }

        public async Task ExecuteSaveCommand()
        {
            if (SplitTransactionMode)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Transaction, Transaction}
                };
                await _navigationService.GoBackAsync(parameters);
                return;
            }

            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextSaving");
                var result = await _transLogic.SaveTransactionAsync(Transaction);

                if (result.Success)
                {
                    _eventAggregator.GetEvent<TransactionSavedEvent>().Publish(result.Data);

                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecutePayeeSelectedCommand()
        {
            await _navigationService.NavigateAsync(PageName.PayeeSelectionPage);
        }

        public async Task ExecuteEnvelopeSelectedCommand()
        {
            if (Transaction.Envelope.IsSystem)
            {
                return;
            }
            await _navigationService.NavigateAsync(PageName.EnvelopeSelectionPage);
        }

        public bool CanExecuteEnvelopeSelectedCommand()
        {
            return !Transaction.Envelope.IsSystem;
        }

        public async Task ExecuteAccountSelectedCommand()
        {
            await _navigationService.NavigateAsync(PageName.AccountSelectionPage);
        }

        public async Task ExecuteDeleteCommand()
        {
			if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextDeleting");
                var result = await _transLogic.SoftDeleteTransactionAsync(Transaction.Id);
                if (result.Success)
                {
                    _eventAggregator.GetEvent<TransactionDeletedEvent>().Publish(result.Data);

                    var parameters = new NavigationParameters
                    {
                        { PageParameter.DeletedTransaction, Transaction }
                    };
                    await _navigationService.GoBackAsync(parameters);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSplitCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.InitialSplitTransaction, Transaction }
            };
            await _navigationService.NavigateAsync(PageName.SplitTransactionPage, parameters);
        }

        public void ExecuteTogglePostedTransaction(Transaction transaction)
        {
            if (transaction != null && !transaction.Reconciled)
            {
                transaction.Posted = !transaction.Posted;
                Transaction.Posted = transaction.Posted;
            }
        }
    }
}
