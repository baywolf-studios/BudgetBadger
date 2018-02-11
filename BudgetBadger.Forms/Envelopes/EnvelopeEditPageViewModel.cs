using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using PropertyChanged;

namespace BudgetBadger.Forms.Envelopes
{
    [AddINotifyPropertyChangedInterface]
    public class EnvelopeEditPageViewModel : INavigationAware
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
            var result = await EnvelopeLogic.SaveBudgetAsync(Budget);

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
            await NavigationService.NavigateAsync(PageName.EnvelopeGroupsPage);
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            var budget = parameters.GetValue<Budget>(PageParameter.Budget);
            if (budget != null)
            {
                Budget = budget.DeepCopy();
            }

            var envelopeGroup = parameters.GetValue<EnvelopeGroup>(PageParameter.EnvelopeGroup);
            if (envelopeGroup != null)
            {
                Budget.Envelope.Group = envelopeGroup.DeepCopy();
            }

            var budgetSchedule = parameters.GetValue<BudgetSchedule>(PageParameter.BudgetSchedule);
            if (budgetSchedule != null)
            {
                Budget.Schedule = budgetSchedule.DeepCopy();
            }
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }
    }
}
