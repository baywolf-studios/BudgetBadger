﻿using System;
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
using BudgetBadger.Core.Purchase;
using Prism.Events;
using BudgetBadger.Forms.Events;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopesPageViewModel : BaseViewModel, INavigatedAware
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<IEnvelopeLogic> _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly Lazy<ISyncFactory> _syncFactory;
        readonly Lazy<IPurchaseService> _purchaseService;
        readonly IEventAggregator _eventAggregator;

        public ICommand NextCommand { get; set; }
        public ICommand PreviousCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand RefreshBudgetCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public ICommand TransferCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public Predicate<object> Filter { get => (budget) => _envelopeLogic.Value.FilterBudget((Budget)budget, SearchText); }

        bool _needToSync;

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

        bool _hasPro;
        public bool HasPro
        {
            get => _hasPro;
            set => SetProperty(ref _hasPro, value);
        }

        bool _hardRefresh = true;

        public EnvelopesPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                      INavigationService navigationService,
                                      Lazy<IEnvelopeLogic> envelopeLogic,
                                      IPageDialogService dialogService,
                                      Lazy<ISyncFactory> syncFactory,
                                      Lazy<IPurchaseService> purchaseService,
                                      IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _purchaseService = purchaseService;
            _eventAggregator = eventAggregator;

            Schedule = null;
            Budgets = new ObservableList<Budget>();
            SelectedBudget = null;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            RefreshBudgetCommand = new DelegateCommand<Budget>(async e => await ExecuteRefreshBudgetCommand(e?.Envelope));
            NextCommand = new DelegateCommand(async () => await ExecuteNextCommand());
            PreviousCommand = new DelegateCommand(async () => await ExecutePreviousCommand());
            SelectedCommand = new DelegateCommand<Budget>(async b => await ExecuteSelectedCommand(b));
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<Budget>(async a => await ExecuteEditCommand(a));
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
            TransferCommand = new DelegateCommand<Budget>(async e => await ExecuteTransferCommand(e));
            SaveCommand = new DelegateCommand<Budget>(async e => await ExecuteSaveCommand(e));
        }

        public override async void OnActivated()
        {
            var purchasedPro = await _purchaseService.Value.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;

            if (_hardRefresh)
            {
                await ExecuteRefreshCommand();
                _hardRefresh = false;
            }
        }

        public override async void OnDeactivated()
        {
            SelectedBudget = null;

            if (_needToSync)
            {
                var syncService = _syncFactory.Value.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.Value.SetLastSyncDateTime(DateTime.Now);
                    _needToSync = false;
                }
            }
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue(PageParameter.Budget, out Budget budget))
            {
                await ExecuteRefreshBudgetCommand(budget.Envelope);
            }

            if (parameters.TryGetValue(PageParameter.Transaction, out Transaction transaction))
            {
                await ExecuteRefreshBudgetCommand(transaction.Envelope);
            }

            await RefreshSummary();
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
                    var scheduleResult = await _envelopeLogic.Value.GetCurrentBudgetScheduleAsync();
                    if (scheduleResult.Success)
                    {
                        Schedule = scheduleResult.Data;
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), scheduleResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                    }
                }

                var budgetResult = await _envelopeLogic.Value.GetBudgetsAsync(Schedule);

                if (budgetResult.Success)
                {
                    if (budgetResult.Data.Any())
                    {
                        Budgets.ReplaceRange(budgetResult.Data);
                        Schedule = Budgets.Any() ? Budgets.FirstOrDefault().Schedule.DeepCopy() : Schedule;
                    }
                    else
                    {
                        Budgets.RemoveAll();
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), budgetResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }

                await RefreshSummary();
                NoEnvelopes = (Budgets?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteRefreshBudgetCommand(Envelope envelope)
        {
            var budgets = Budgets.Where(a => a.Envelope.Id != envelope.Id).ToList();

            var updatedBudget = await _envelopeLogic.Value.GetBudgetAsync(envelope.Id, Schedule);
            if (updatedBudget.Success && updatedBudget.Data.Envelope.IsActive)
            {
                budgets.Add(updatedBudget.Data);
            }

            Budgets.ReplaceRange(budgets);
        }

        public async Task ExecuteNextCommand()
        {
            var scheduleResult = await _envelopeLogic.Value.GetNextBudgetSchedule(Schedule);
            if (scheduleResult.Success)
            {
                Schedule = scheduleResult.Data;
                await ExecuteRefreshCommand();
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), scheduleResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecutePreviousCommand()
        {
            var scheduleResult = await _envelopeLogic.Value.GetPreviousBudgetSchedule(Schedule);
            if (scheduleResult.Success)
            {
                Schedule = scheduleResult.Data;
                await ExecuteRefreshCommand();
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), scheduleResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteSelectedCommand(Budget budget)
        {
            if (budget == null)
            {
                return;
            }

            if (budget.Envelope.IsGenericHiddenEnvelope)
            {
                _hardRefresh = true;
                await _navigationService.NavigateAsync(PageName.HiddenEnvelopesPage);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Budget, budget }
                };
                await _navigationService.NavigateAsync(PageName.EnvelopeInfoPage, parameters);
            }
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
            if (!budget.Envelope.IsGenericHiddenEnvelope)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Budget, budget }
                };
                await _navigationService.NavigateAsync(PageName.EnvelopeEditPage, parameters);
            }
        }

        public async Task ExecuteAddTransactionCommand()
        {
            await _navigationService.NavigateAsync(PageName.TransactionEditPage);
        }

        public async Task ExecuteTransferCommand(Budget budget)
        {
            if (!budget.Envelope.IsGenericHiddenEnvelope)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Envelope, budget.Envelope },
                    { PageParameter.BudgetSchedule, Schedule }
                };
                await _navigationService.NavigateAsync(PageName.EnvelopeTransferPage, parameters);
            }
        }

        public async Task ExecuteSaveCommand(Budget budget)
        {
            var result = await _envelopeLogic.Value.SaveBudgetAsync(budget);

            if (result.Success)
            {
                _needToSync = true;
                _eventAggregator.GetEvent<BudgetSavedEvent>().Publish(result.Data);
                await RefreshSummary();
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        async Task RefreshSummary()
        {
            Result<BudgetSchedule> scheduleResult;
            if (Schedule != null)
            {
                scheduleResult = await _envelopeLogic.Value.GetBudgetSchedule(Schedule);
            }
            else
            {
                scheduleResult = await _envelopeLogic.Value.GetCurrentBudgetScheduleAsync();
            }

            if (scheduleResult.Success)
            {
                Schedule = scheduleResult.Data;
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), scheduleResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }

            foreach (var budget in Budgets)
            {
                budget.Schedule = Schedule;
            }

            RaisePropertyChanged(nameof(Schedule));
            RaisePropertyChanged(nameof(Schedule.Past));
            RaisePropertyChanged(nameof(Schedule.Income));
            RaisePropertyChanged(nameof(Schedule.ToBudget));
            RaisePropertyChanged(nameof(Schedule.Overspend));
        }
    }
}
