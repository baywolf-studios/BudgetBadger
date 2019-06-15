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
using BudgetBadger.Models.Extensions;
using Xamarin.Forms;
using BudgetBadger.Core.LocalizedResources;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopesPageViewModel : BindableBase, IActiveAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISyncFactory _syncFactory;

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

        public EnvelopesPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
		                              IEnvelopeLogic envelopeLogic,
		                              IPageDialogService dialogService,
		                              ISyncFactory syncFactory)
        {
            _resourceContainer = resourceContainer;
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _syncFactory = syncFactory;

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

        private bool _IsActive;
        public bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                _IsActive = value;
                if (value)
                    OnAppearing();
                else
                    OnDisappearing();
            }
        }

        public event EventHandler IsActiveChanged;

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
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), scheduleResult.Message, _resourceContainer.GetResourceString("AlertOk"));
                    }
                }

                var budgetResult = await _envelopeLogic.GetBudgetsAsync(Schedule);

                if (budgetResult.Success)
                {
                    if (budgetResult.Data.Any())
                    {
                        Budgets = budgetResult.Data;
                        Schedule = Budgets.Any() ? Budgets.FirstOrDefault().Schedule.DeepCopy() : Schedule;
                    }
                    else
                    {
                        Budgets = new List<Budget>();
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), budgetResult.Message, _resourceContainer.GetResourceString("AlertOk"));
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
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), scheduleResult.Message, _resourceContainer.GetResourceString("AlertOk"));
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
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), scheduleResult.Message, _resourceContainer.GetResourceString("AlertOk"));
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

                var syncService = _syncFactory.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.SetLastSyncDateTime(DateTime.Now);
                }
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
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
                { PageParameter.Envelope, budget.Envelope },
                { PageParameter.BudgetSchedule, Schedule }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeTransferPage, parameters);
        }
    }
}
