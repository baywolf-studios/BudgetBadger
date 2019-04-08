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
using BudgetBadger.Core.Sync;
using Prism;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopesPageViewModel : BindableBase, IPageLifecycleAware
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
		readonly ISync _syncService;

        public ICommand NextCommand { get; set; }
        public ICommand PreviousCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public ICommand TransferCommand { get; set; }
        public Predicate<object> Filter { get => (budget) => _envelopeLogic.FilterBudget((Budget)budget, SearchText); }

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

        IReadOnlyList<Budget> _budgets;
        public IReadOnlyList<Budget> Budgets
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

        bool _noEnvelopes;
        public bool NoEnvelopes
        {
            get => _noEnvelopes;
            set => SetProperty(ref _noEnvelopes, value);
        }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public EnvelopesPageViewModel(INavigationService navigationService,
		                              IEnvelopeLogic envelopeLogic,
		                              IPageDialogService dialogService,
		                              ISync syncService)
        {
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
			_syncService = syncService;

            Schedule = null;
            Budgets = new List<Budget>();
            SelectedBudget = null;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            NextCommand = new DelegateCommand(async () => await ExecuteNextCommand());
            PreviousCommand = new DelegateCommand(async () => await ExecutePreviousCommand());
            SelectedCommand = new DelegateCommand<Budget>(async b => await ExecuteSelectedCommand(b));
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<Budget>(async a => await ExecuteEditCommand(a));
            DeleteCommand = new DelegateCommand<Budget>(async a => await ExecuteDeleteCommand(a));
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
            TransferCommand = new DelegateCommand<Budget>(async e => await ExecuteTransferCommand(e));
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
                        await _dialogService.DisplayAlertAsync("Error", scheduleResult.Message, "OK");
                    }
                }

                var budgetResult = await _envelopeLogic.GetBudgetsAsync(Schedule);

                if (budgetResult.Success)
                {
                    Budgets = budgetResult.Data;
                    Schedule = Budgets.Any() ? Budgets.FirstOrDefault().Schedule.DeepCopy() : Schedule;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Error", budgetResult.Message, "OK");
                }

                NoEnvelopes = (Budgets?.Count ?? 0) == 0;
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
                await _dialogService.DisplayAlertAsync("Error", scheduleResult.Message, "OK");
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
                await _dialogService.DisplayAlertAsync("Error", scheduleResult.Message, "OK");
            }
        }

        public async Task ExecuteSelectedCommand(Budget budget)
        {
            if (budget == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Budget, budget }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeInfoPage, parameters);
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

                var syncResult = await _syncService.FullSync();

                if (!syncResult.Success)
                {
                    await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                }
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
            }
        }

        public async Task ExecuteAddTransactionCommand()
        {
            await _navigationService.NavigateAsync(PageName.TransactionEditPage);
        }

        public async Task ExecuteTransferCommand(Budget budget)
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Envelope, budget.Envelope }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeTransferPage, parameters);
        }
    }
}
