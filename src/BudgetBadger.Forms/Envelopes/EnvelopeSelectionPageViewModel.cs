using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Core.Models;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeSelectionPageViewModel : ObservableBase, INavigationAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
		public ICommand AddCommand { get; set; }
       
        public Predicate<object> Filter { get => (budget) => _envelopeLogic.FilterBudget((Budget)budget, SearchText); }

        bool _transferEnvelopeSelection { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        ObservableList<Budget> _budgets;
        public ObservableList<Budget> Budgets
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

        public EnvelopeSelectionPageViewModel(
            IResourceContainer resourceContainer,
            INavigationService navigationService,
            IEnvelopeLogic envelopeLogic,
            IPageDialogService dialogService)
        {
            _resourceContainer = resourceContainer;
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Budgets = new ObservableList<Budget>();
            SelectedBudget = null;

            RefreshCommand = new Command(async () => await FullRefresh());
            SelectedCommand = new Command<Budget>(async b => await ExecuteSelectedCommand(b));
			AddCommand = new Command(async () => await ExecuteAddCommand());
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedBudget = null;
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            var envelope = parameters.GetValue<Envelope>(PageParameter.Envelope);
            var budget = parameters.GetValue<Budget>(PageParameter.Budget);
            if (envelope != null || budget != null)
            {
                await _navigationService.GoBackAsync(parameters);
                return;
            }

            _transferEnvelopeSelection = parameters.GetValue<bool>(PageParameter.TransferEnvelopeSelection);

            await FullRefresh();
        }

        public async Task FullRefresh()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {

                var scheduleResult = await _envelopeLogic.GetCurrentBudgetScheduleAsync();
                if (scheduleResult.Success)
                {
                    var schedule = scheduleResult.Data;

                    Result<IReadOnlyList<Budget>> budgetResult;

                    if (_transferEnvelopeSelection)
                    {
                        budgetResult = await _envelopeLogic.GetBudgetsAsync(schedule);
                    }
                    else
                    {
                        budgetResult = await _envelopeLogic.GetBudgetsForSelectionAsync(schedule);
                    }

                    if (budgetResult.Success)
                    {
                        Budgets.ReplaceRange(budgetResult.Data);
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), budgetResult.Message, _resourceContainer.GetResourceString("AlertOk"));
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), scheduleResult.Message, _resourceContainer.GetResourceString("AlertOk"));
                }

                NoEnvelopes = (Budgets?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
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
                { PageParameter.Budget, budget },
                { PageParameter.Envelope, budget.Envelope }
            };
            await _navigationService.GoBackAsync(parameters);
        }

		public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.EnvelopeEditPage);
        }
    }
}

