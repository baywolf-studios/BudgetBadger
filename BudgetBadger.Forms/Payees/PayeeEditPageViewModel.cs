using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using BudgetBadger.Forms.ViewModels;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Payees
{
    public class PayeeEditPageViewModel : BaseViewModel, INavigationAware
    {
        readonly IPayeeLogic PayeeLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public Payee Payee { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public PayeeEditPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IPayeeLogic payeeLogic)
        {
            Title = "New Payee";
            NavigationService = navigationService;
            DialogService = dialogService;
            PayeeLogic = payeeLogic;

            Payee = new Payee();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            DeleteCommand = new DelegateCommand(async () => await ExecuteDeleteCommand());
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            var payee = parameters.GetValue<Payee>(NavigationParameterType.Payee);
            if (payee != null)
            {
                Payee = payee.DeepCopy();
                Title = "Edit Payee";
            }
        }

        public async Task ExecuteSaveCommand()
        {
            var result = await PayeeLogic.UpsertPayeeAsync(Payee);

            if (result.Success)
            {
                await NavigationService.GoBackAsync();
            }
            else
            {
                await DialogService.DisplayAlertAsync("Error", result.Message, "Okay");
            }
        }

        public async Task ExecuteDeleteCommand()
        {
            var result = await PayeeLogic.DeletePayeeAsync(Payee);

            if (result.Success)
            {
                await NavigationService.GoBackToRootAsync();
            }
            else
            {
                await DialogService.DisplayAlertAsync("Error", result.Message, "Okay");
            }
        }
    }
}
