using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Navigation;
using Prism.Commands;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using BudgetBadger.Core.Logic;
using System.Collections.Generic;
using Prism.Mvvm;
using Prism.AppModel;
using Prism.Services;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopesPageViewModel : BindableBase, INavigatingAware, IPageLifecycleAware
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand NextCommand { get; set; }
        public ICommand PreviousCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        Guid? _currentScheduleId { get; set; }

        BudgetSchedule _schedule;
        public BudgetSchedule Schedule
        {
            get => _schedule;
            set => SetProperty(ref _schedule, value);
        }

        IEnumerable<Budget> _budgets;
        public IEnumerable<Budget> Budgets
        {
            get => _budgets;
            set => SetProperty(ref _budgets, value);
        }

        Budget _selectedBudget;
        public Budget SelectedBudget
        {
            get => _selectedBudget;
            set => SetProperty(ref _selectedBudget, value);
        }

        ILookup<string, Budget> _groupedBudgets;
        public ILookup<string, Budget> GroupedBudgets
        {
            get => _groupedBudgets;
            set => SetProperty(ref _groupedBudgets, value);
        }

        bool _selectorMode;
        public bool SelectorMode
        {
            get => _selectorMode;
            set { SetProperty(ref _selectorMode, value); RaisePropertyChanged("MainMode"); }
        }

        public bool MainMode { get => !SelectorMode; }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); ExecuteSearchCommand(); }
        }

        public EnvelopesPageViewModel(INavigationService navigationService, IEnvelopeLogic envelopeLogic, IPageDialogService dialogService)
        {
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

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
            EditCommand = new DelegateCommand<Budget>(async b => await ExecuteEditCommand(b));
            DeleteCommand = new DelegateCommand<Budget>(async b => await ExecuteDeleteCommand(b));
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            // returns default bool if none present
            SelectorMode = parameters.GetValue<bool>(PageParameter.SelectorMode);

            await ExecuteRefreshCommand();
        }

        public async void OnAppearing()
        {
            await ExecuteRefreshCommand();
        }

        public void OnDisappearing()
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
                    var scheduleResult = await _envelopeLogic.GetCurrentBudgetScheduleAsync();
                    if (scheduleResult.Success)
                    {
                        Schedule = scheduleResult.Data;
                    }
                    else
                    {
                        //show error
                    }
                }

                var budgetResult = await _envelopeLogic.GetBudgetsAsync(Schedule, SelectorMode);
                if (budgetResult.Success)
                {
                    Budgets = budgetResult.Data;
                    GroupedBudgets = _envelopeLogic.GroupBudgets(Budgets);
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
            var scheduleResult = await _envelopeLogic.GetNextBudgetSchedule(Schedule);
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
            var scheduleResult = await _envelopeLogic.GetPreviousBudgetSchedule(Schedule);
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
                await _navigationService.GoBackAsync(parameters);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Budget, SelectedBudget }
                };
                await _navigationService.NavigateAsync(PageName.EnvelopeInfoPage, parameters);
            }

            SelectedBudget = null;
        }

        public async Task ExecuteAddCommand()
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.BudgetSchedule, Schedule }
            };

            await _navigationService.NavigateAsync(PageName.EnvelopeEditPage, parameters);

            SelectedBudget = null;
        }

        public void ExecuteSearchCommand()
        {
            GroupedBudgets = _envelopeLogic.GroupBudgets(_envelopeLogic.SearchBudgets(Budgets, SearchText));
        }

        public async Task ExecuteEditCommand(Budget budget)
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Budget, budget }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeEditPage, parameters);
        }

        public async Task ExecuteDeleteCommand(Budget budget)
        {
            var result = await _envelopeLogic.DeleteEnvelopeAsync(budget.Envelope.Id);

            if (result.Success)
            {
                await ExecuteRefreshCommand();
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
            }
        }
    }
}
