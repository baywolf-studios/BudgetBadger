using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Navigation;
using Prism.Commands;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using BudgetBadger.Core.Logic;
using PropertyChanged;
using System.Collections.Generic;

namespace BudgetBadger.Forms.Envelopes
{
    [AddINotifyPropertyChangedInterface]
    public class EnvelopesPageViewModel : INavigationAware
    {
        readonly IEnvelopeLogic EnvelopeLogic;
        readonly INavigationService NavigationService;

        public ICommand NextCommand { get; set; }
        public ICommand PreviousCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SearchCommand { get; set; }

        public bool IsBusy { get; set; }

        private Guid? CurrentScheduleId { get; set; }
        public BudgetSchedule Schedule { get; set; }
        public IEnumerable<Budget> Budgets { get; set; }
        public Budget SelectedBudget { get; set; }
        public ILookup<string, Budget> GroupedBudgets { get; set; }

        public bool SelectorMode { get; set; }
        public bool MainMode { get { return !SelectorMode; }}

        public string SearchText { get; set; }
        public void OnSearchTextChanged()
        {
            ExecuteSearchCommand();
        }

        public EnvelopesPageViewModel(INavigationService navigationService, IEnvelopeLogic envelopeLogic)
        {
            EnvelopeLogic = envelopeLogic;
            NavigationService = navigationService;

            Schedule = null;
            Budgets = new List<Budget>();
            SelectedBudget = null;
            GroupedBudgets = Budgets.ToLookup(b => "");

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            NextCommand = new DelegateCommand(async () => await ExecuteNextCommand());
            PreviousCommand = new DelegateCommand(async () => await ExecutePreviousCommand());
            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            // returns default bool if none present
            SelectorMode = parameters.GetValue<bool>(PageParameter.SelectorMode);

            await ExecuteRefreshCommand();
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
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
                if (Schedule == null)
                {
                    var scheduleResult = await EnvelopeLogic.GetCurrentBudgetScheduleAsync();
                    if (scheduleResult.Success)
                    {
                        Schedule = scheduleResult.Data;
                    }
                    else
                    {
                        //show error
                    }
                }

                var budgetResult = await EnvelopeLogic.GetBudgetsAsync(Schedule, SelectorMode);
                if (budgetResult.Success)
                {
                    Budgets = budgetResult.Data;
                    GroupedBudgets = EnvelopeLogic.GroupBudgets(Budgets);
                    Schedule = Budgets.Any() ? Budgets.FirstOrDefault().Schedule.DeepCopy() : Schedule;
                }
                else
                {
                    //show error
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteNextCommand()
        {
            var scheduleResult = await EnvelopeLogic.GetNextBudgetSchedule(Schedule);
            if (scheduleResult.Success)
            {
                Schedule = scheduleResult.Data;
                await ExecuteRefreshCommand();
            }
            else
            {
                //show error
            }
        }

        public async Task ExecutePreviousCommand()
        {
            var scheduleResult = await EnvelopeLogic.GetPreviousBudgetSchedule(Schedule);
            if (scheduleResult.Success)
            {
                Schedule = scheduleResult.Data;
                await ExecuteRefreshCommand();
            }
            else
            {
                //show error
            }
        }

        public async Task ExecuteSelectedCommand()
        {
            if (SelectedBudget == null)
            {
                return;
            }

            if (SelectorMode)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Envelope, SelectedBudget.Envelope }
                };
                await NavigationService.GoBackAsync(parameters);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Budget, SelectedBudget }
                };
                await NavigationService.NavigateAsync(PageName.EnvelopeInfoPage, parameters);
            }

            SelectedBudget = null;
        }

        public async Task ExecuteAddCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.BudgetSchedule, Schedule }
            };

            await NavigationService.NavigateAsync(PageName.EnvelopeEditPage, parameters);

            SelectedBudget = null;
        }

        public void ExecuteSearchCommand()
        {
            GroupedBudgets = EnvelopeLogic.GroupBudgets(EnvelopeLogic.SearchBudgets(Budgets, SearchText));
        }
    }
}
