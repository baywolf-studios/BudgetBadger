using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using BudgetBadger.Forms.ViewModels;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Transactions
{
    public class TransactionPageViewModel : BaseViewModel, INavigationAware
    {
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;
        readonly ITransactionLogic TransLogic;

        public Transaction Transaction { get; set; }

        public ICommand SaveCommand { get; set; }
        public ICommand PayeeSelectedCommand { get; set; }
        public ICommand EnvelopeSelectedCommand { get; set; }
        public ICommand AccountSelectedCommand { get; set; }

        public TransactionPageViewModel(INavigationService navigationService, IPageDialogService dialogService, ITransactionLogic transLogic)
        {
            Title = "New Transaction";
            NavigationService = navigationService;
            DialogService = dialogService;
            TransLogic = transLogic;

            Transaction = new Transaction();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            PayeeSelectedCommand = new DelegateCommand(async () => await ExecutePayeeSelectedCommand());
            EnvelopeSelectedCommand = new DelegateCommand(async () => await ExecuteEnvelopeSelectedCommand());
            AccountSelectedCommand = new DelegateCommand(async () => await ExecuteAccountSelectedCommand());
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            var transaction = parameters.GetValue<Transaction>(NavigationParameterType.Transaction);
            if (transaction != null)
            {
                Transaction = transaction.DeepCopy();
                Title = "Edit Transaction";
            }

            var payee = parameters.GetValue<Payee>(NavigationParameterType.Payee);
            if (payee != null)
            {
                Transaction.Payee = payee.DeepCopy();
            }

            var account = parameters.GetValue<Account>(NavigationParameterType.Account);
            if (account != null)
            {
                Transaction.Account = account.DeepCopy();
            }

            var envelope = parameters.GetValue<Envelope>(NavigationParameterType.Envelope);
            if (envelope != null)
            {
                Transaction.Envelope = envelope.DeepCopy();
            }
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
