using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using PropertyChanged;

namespace BudgetBadger.Forms.Transactions
{
    [AddINotifyPropertyChangedInterface]
    public class TransactionPageViewModel : INavigationAware
    {
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;
        readonly ITransactionLogic TransLogic;

        public Transaction Transaction { get; set; }

        public bool NewMode { get => Transaction.CreatedDateTime == null; }
        public bool EditMode { get => !NewMode; }

        public ICommand SaveCommand { get; set; }
        public ICommand PayeeSelectedCommand { get; set; }
        public ICommand EnvelopeSelectedCommand { get; set; }
        public ICommand AccountSelectedCommand { get; set; }

        public TransactionPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ITransactionLogic transLogic)
        {
            NavigationService = navigationService;
            DialogService = dialogService;
            TransLogic = transLogic;

            Transaction = new Transaction();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            PayeeSelectedCommand = new DelegateCommand(async () => await ExecutePayeeSelectedCommand());
            EnvelopeSelectedCommand = new DelegateCommand(async () => await ExecuteEnvelopeSelectedCommand());
            AccountSelectedCommand = new DelegateCommand(async () => await ExecuteAccountSelectedCommand());
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            var transaction = parameters.GetValue<Transaction>(NavigationParameterType.Transaction);
            if (transaction != null)
            {
                Transaction = transaction.DeepCopy();
            }

            var payee = parameters.GetValue<Payee>(NavigationParameterType.Payee);
            var account = parameters.GetValue<Account>(NavigationParameterType.Account);
            var envelope = parameters.GetValue<Envelope>(NavigationParameterType.Envelope);

            var result = await TransLogic.GetPopulatedTransaction(Transaction, account, envelope, payee);
            if (result.Success)
            {
                Transaction = result.Data;
            }
            else
            {
                await DialogService.DisplayAlertAsync("Error", result.Message, "Okay");
                await NavigationService.GoBackAsync();
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
            var result = await TransLogic.UpsertTransactionAsync(Transaction);

            if (result.Success)
            {
                await NavigationService.GoBackAsync();
            }
            else
            {
                await DialogService.DisplayAlertAsync("Error", result.Message, "Ok");
            }
        }

        public async Task ExecutePayeeSelectedCommand()
        {
            var parameters = new NavigationParameters
            {
                { NavigationParameterType.SelectorMode, true }
            };
            await NavigationService.NavigateAsync(NavigationPageName.PayeesPage, parameters);
        }

        public async Task ExecuteEnvelopeSelectedCommand()
        {
            var parameters = new NavigationParameters
            {
                { NavigationParameterType.SelectorMode, true }
            };
            await NavigationService.NavigateAsync(NavigationPageName.EnvelopesPage, parameters);
        }

        public async Task ExecuteAccountSelectedCommand()
        {
            var parameters = new NavigationParameters
            {
                { NavigationParameterType.SelectorMode, true }
            };
            await NavigationService.NavigateAsync(NavigationPageName.AccountsPage, parameters);
        }
    }
}
