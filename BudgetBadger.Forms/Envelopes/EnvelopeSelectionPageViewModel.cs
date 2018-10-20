using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeSelectionPageViewModel : BindableBase, INavigationAware
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
		public ICommand AddCommand { get; set; }
        public Predicate<object> Filter { get => (budget) => _envelopeLogic.FilterBudget((Budget)budget, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
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

        public EnvelopeSelectionPageViewModel(INavigationService navigationService, IEnvelopeLogic envelopeLogic, IPageDialogService dialogService)
        {
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Budgets = new List<Budget>();
            SelectedBudget = null;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SelectedCommand = new DelegateCommand<Budget>(async b => await ExecuteSelectedCommand(b));
			AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            var envelope = parameters.GetValue<Envelope>(PageParameter.Envelope);
            if (envelope != null)
            {
                await _navigationService.GoBackAsync(parameters);
            }
        }

        public async void OnNavigatingTo(INavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async Task ExecuteRefreshCommand()
        {
            if (!IsBusy)
            {
                IsBusy = true;
            }

            try
            {

                var scheduleResult = await _envelopeLogic.GetCurrentBudgetScheduleAsync();
                if (scheduleResult.Success)
                {
                    var schedule = scheduleResult.Data;

                    var budgetResult = await _envelopeLogic.GetBudgetsForSelectionAsync(schedule);

                    if (budgetResult.Success)
                    {
                        Budgets = budgetResult.Data;
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync("Error", budgetResult.Message, "OK");
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Error", scheduleResult.Message, "OK");
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

