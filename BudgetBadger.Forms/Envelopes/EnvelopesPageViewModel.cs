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
            set
            {
                _schedule = value;
                RaisePropertyChanged(nameof(Schedule));
            }
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

        bool _fullRefresh = true;

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

            RefreshCommand = new DelegateCommand(async () => await FullRefresh());
            RefreshBudgetCommand = new DelegateCommand<Budget>(async b => await RefreshBudget(b));
            NextCommand = new DelegateCommand(async () => await ExecuteNextCommand());
            PreviousCommand = new DelegateCommand(async () => await ExecutePreviousCommand());
            SelectedCommand = new DelegateCommand<Budget>(async b => await ExecuteSelectedCommand(b));
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<Budget>(async a => await ExecuteEditCommand(a));
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
            TransferCommand = new DelegateCommand<Budget>(async e => await ExecuteTransferCommand(e));
            SaveCommand = new DelegateCommand<Budget>(async e => await ExecuteSaveCommand(e));

            _eventAggregator.GetEvent<BudgetSavedEvent>().Subscribe(async b => await RefreshBudget(b));
            _eventAggregator.GetEvent<EnvelopeDeletedEvent>().Subscribe(async b => await RefreshBudgetFromEnvelope(b));
            _eventAggregator.GetEvent<EnvelopeHiddenEvent>().Subscribe(async b => await RefreshBudgetFromEnvelope(b));
            _eventAggregator.GetEvent<EnvelopeUnhiddenEvent>().Subscribe(async b => await RefreshBudgetFromEnvelope(b));

            _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Subscribe(async b => await FullRefresh());
            _eventAggregator.GetEvent<EnvelopeGroupDeletedEvent>().Subscribe(async b => await FullRefresh());
            _eventAggregator.GetEvent<EnvelopeGroupHiddenEvent>().Subscribe(async b => await FullRefresh());
            _eventAggregator.GetEvent<EnvelopeGroupUnhiddenEvent>().Subscribe(async b => await FullRefresh());

            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshBudgetFromTransaction(t));
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(async t => await RefreshBudgetFromTransaction(t));
        }

        public override async void OnActivated()
        {
            var purchasedPro = await _purchaseService.Value.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;

            if (_fullRefresh)
            {
                await FullRefresh();
                _fullRefresh = false;
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
            await RefreshSummary();
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
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteNextCommand()
        {
            var scheduleResult = await _envelopeLogic.Value.GetNextBudgetSchedule(Schedule);
            if (scheduleResult.Success)
            {
                Schedule = scheduleResult.Data;
                await FullRefresh();
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
                await FullRefresh();
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

        public async Task RefreshSummary()
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

            NoEnvelopes = (Budgets?.Count ?? 0) == 0;

            RaisePropertyChanged(nameof(Schedule));
            RaisePropertyChanged(nameof(Schedule.Past));
            RaisePropertyChanged(nameof(Schedule.Income));
            RaisePropertyChanged(nameof(Schedule.ToBudget));
            RaisePropertyChanged(nameof(Schedule.Overspend));
        }

        public async Task RefreshBudget(Budget budget)
        {
            var budgets = Budgets.Where(a => a.Envelope.Id != budget.Envelope.Id).ToList();

            if (budget != null && budget.Envelope != null && budget.Envelope.IsActive)
            {
                budgets.Add(budget);
            }

            Budgets.ReplaceRange(budgets);

            await RefreshSummary();
        }

        public async Task RefreshBudgetFromEnvelope(Envelope envelope)
        {
            if (envelope != null && envelope.IsActive)
            {
                var schedule = Budgets.FirstOrDefault()?.Schedule;
                if (schedule == null)
                {
                    var currentScheduleResult = await _envelopeLogic.Value.GetCurrentBudgetScheduleAsync();
                    if (currentScheduleResult.Success)
                    {
                        schedule = currentScheduleResult.Data;
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), currentScheduleResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                        return;
                    }
                }
                var budgetResult = await _envelopeLogic.Value.GetBudgetAsync(envelope.Id, schedule);
                if (budgetResult.Success)
                {
                    await RefreshBudget(budgetResult.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), budgetResult.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }
            }
        }

        public async Task RefreshBudgetFromTransaction(Transaction transaction)
        {
            if (transaction != null && transaction.Payee != null)
            {
                var updatedEnvelopeResult = await _envelopeLogic.Value.GetEnvelopeAsync(transaction.Envelope.Id);
                if (updatedEnvelopeResult.Success)
                {
                    await RefreshBudgetFromEnvelope(updatedEnvelopeResult.Data);
                }
            }
        }
    }
}
