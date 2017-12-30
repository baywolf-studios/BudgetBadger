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

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeEditPageViewModel : BaseViewModel, INavigationAware
    {
        readonly IEnvelopeLogic EnvelopeLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public Budget Budget { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand GroupSelectedCommand { get; set; }

        public EnvelopeEditPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IEnvelopeLogic envelopeLogic)
        {
            Title = "New Payee";
            NavigationService = navigationService;
            DialogService = dialogService;
            EnvelopeLogic = envelopeLogic;

            Budget = new Budget();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            GroupSelectedCommand = new DelegateCommand(async () => await ExecuteGroupSelectedCommand());
            //DeleteCommand = new DelegateCommand(async () => await ExecuteDeleteCommand());
        }

        public async Task ExecuteSaveCommand()
        {
            var result = await EnvelopeLogic.UpsertBudgetAsync(Budget);

            if (result.Success)
            {
                await NavigationService.GoBackAsync();
            }
            else
            {
                await DialogService.DisplayAlertAsync("Error", result.Message, "Okay");
            }
        }

        public async Task ExecuteGroupSelectedCommand()
        {
            await NavigationService.NavigateAsync(NavigationPageName.EnvelopeGroupsPage);
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            var budget = parameters.GetValue<Budget>(NavigationParameterType.Budget);
            if (budget != null)
            {
                Budget = budget.DeepCopy();
                Title = "Edit Budget";
            }

            var envelopeGroup = parameters.GetValue<EnvelopeGroup>(NavigationParameterType.EnvelopeGroup);
            if (envelopeGroup != null)
            {
                Budget.Envelope.Group = envelopeGroup.DeepCopy();
            }

            var budgetSchedule = parameters.GetValue<BudgetSchedule>(NavigationParameterType.BudgetSchedule);
            if (budgetSchedule != null)
            {
                Budget.Schedule = budgetSchedule.DeepCopy();
            }
        }
    }
}
