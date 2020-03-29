using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using Prism.Mvvm;
using BudgetBadger.Core.Sync;
using BudgetBadger.Models.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using BudgetBadger.Core.LocalizedResources;
using Prism.Events;
using BudgetBadger.Forms.Events;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeEditPageViewModel : BindableBase, INavigationAware, IInitializeAsync
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISyncFactory _syncFactory;
        readonly IResourceContainer _resourceContainer;
        readonly IEventAggregator _eventAggregator;

        bool _needToSync;

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        string _busyText;
        public string BusyText
        {
            get => _busyText;
            set => SetProperty(ref _busyText, value);
        }

        Budget _budget;
        public Budget Budget
        {
            get => _budget;
			set
			{
				SetProperty(ref _budget, value);
				RaisePropertyChanged(nameof(IsNotDebt));
			}
        }
        
        public bool IsNotDebt
		{
			get => !Budget.Envelope.Group.IsDebt;
		}

        public IList<string> OverspendingTypes
        {
            get => Enum.GetNames(typeof(OverspendingType)).Select(b => _resourceContainer.GetResourceString(b)).ToList();
        }

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand QuickBudgetCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand SoftDeleteCommand { get; set; }
        public ICommand HideCommand { get; set; }
        public ICommand UnhideCommand { get; set; }
        public ICommand GroupSelectedCommand { get; set; }

        public EnvelopeEditPageViewModel(INavigationService navigationService,
                                         IPageDialogService dialogService,
                                         IEnvelopeLogic envelopeLogic,
                                         ISyncFactory syncFactory,
                                         IResourceContainer resourceContainer,
                                         IEventAggregator eventAggregator)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _envelopeLogic = envelopeLogic;
            _syncFactory = syncFactory;
            _resourceContainer = resourceContainer;
            _eventAggregator = eventAggregator;

            Budget = new Budget();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            GroupSelectedCommand = new DelegateCommand(async () => await ExecuteGroupSelectedCommand());
            SoftDeleteCommand = new DelegateCommand(async () => await ExecuteSoftDeleteCommand(), CanExecuteSoftDeleteCommand).ObservesProperty(() => Budget);
            HideCommand = new DelegateCommand(async () => await ExecuteHideCommand(), CanExecuteHideCommand).ObservesProperty(() => Budget);
            UnhideCommand = new DelegateCommand(async () => await ExecuteUnhideCommand());
            QuickBudgetCommand = new DelegateCommand(async () => await ExecuteQuickBudgetCommand());
        }
        
        public async Task InitializeAsync(INavigationParameters parameters)
        {

            var envelope = parameters.GetValue<Envelope>(PageParameter.Envelope);
            if (envelope != null)
            {
                var result = await _envelopeLogic.GetCurrentBudgetScheduleAsync();
                if (result.Success)
                {
                    var budgetResult = await _envelopeLogic.GetBudgetAsync(envelope.Id, result.Data);
                    if (budgetResult.Success)
                    {
                        Budget = budgetResult.Data.DeepCopy();
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertGeneralError"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                        await _navigationService.GoBackAsync();
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertGeneralError"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                    await _navigationService.GoBackAsync();
                }
            }

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
            else if (Budget.Schedule.Id == Guid.Empty)
            {
                var result = await _envelopeLogic.GetCurrentBudgetScheduleAsync();
                if (result.Success)
                {
                    Budget.Schedule = result.Data;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertGeneralError"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                    await _navigationService.GoBackAsync();
                }
            }
        }

        public async void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (_needToSync)
            {
                var syncService = _syncFactory.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.SetLastSyncDateTime(DateTime.Now);
                    _needToSync = false;
                }
            }
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back)
            {
                await InitializeAsync(parameters);
            }
        }

        public async Task ExecuteSaveCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextSaving");
                var result = await _envelopeLogic.SaveBudgetAsync(Budget);

                if (result.Success)
                {

                    _needToSync = true;

                    _eventAggregator.GetEvent<BudgetSavedEvent>().Publish(result.Data);

                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteGroupSelectedCommand()
        {
            await _navigationService.NavigateAsync(PageName.EnvelopeGroupSelectionPage);
        }

        public bool CanExecuteSoftDeleteCommand()
        {
            if (Budget != null && Budget.Envelope != null)
            {
                return Budget.Envelope.IsHidden;
            }
            else
            {
                return false;
            }
        }

        public async Task ExecuteSoftDeleteCommand()
        {
			if (IsBusy)
            {
                return;
            }
            var confirm = await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertConfirmation"),
                _resourceContainer.GetResourceString("AlertConfirmDelete"),
                _resourceContainer.GetResourceString("AlertOk"),
                _resourceContainer.GetResourceString("AlertCancel"));

            if (confirm)
            {
                IsBusy = true;

                try
                {
                    BusyText = _resourceContainer.GetResourceString("BusyTextDeleting");
                    var result = await _envelopeLogic.SoftDeleteEnvelopeAsync(Budget.Envelope.Id);
                    if (result.Success)
                    {
                        _needToSync = true;

                        _eventAggregator.GetEvent<EnvelopeDeletedEvent>().Publish(result.Data);

                        await _navigationService.GoBackAsync();
                    }
                    else
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public bool CanExecuteHideCommand()
        {
            if (Budget != null && Budget.Envelope != null)
            {
                return Budget.Envelope.IsActive;
            }
            else
            {
                return false;
            }
        }

        public async Task ExecuteHideCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextHiding");
                var result = await _envelopeLogic.HideEnvelopeAsync(Budget.Envelope.Id);
                if (result.Success)
                {
                    _needToSync = true;

                    _eventAggregator.GetEvent<EnvelopeHiddenEvent>().Publish(result.Data);

                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertHideUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteUnhideCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextUnhiding");
                var result = await _envelopeLogic.UnhideEnvelopeAsync(Budget.Envelope.Id);
                if (result.Success)
                {
                    _needToSync = true;

                    _eventAggregator.GetEvent<EnvelopeUnhiddenEvent>().Publish(result.Data);

                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertUnhideUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteQuickBudgetCommand()
        {
            var quickBudgetResult = await _envelopeLogic.GetQuickBudgetsAsync(Budget);

            if (quickBudgetResult.Success)
            {
                if (quickBudgetResult.Data.Count > 0)
                {
                    var buttons = new List<IActionSheetButton>();

                    foreach (var quickBudget in quickBudgetResult.Data)
                    {
                        var buttonText = quickBudget.Description + " = " + quickBudget.Amount.ToString("c");
                        var action = ActionSheetButton.CreateButton(buttonText, () =>
                        {
                            Budget.Amount = quickBudget.Amount;
                        });

                        buttons.Add(action);
                    }

                    buttons.Add(ActionSheetButton.CreateCancelButton(_resourceContainer.GetResourceString("AlertCancel"), () => { }));

                    await _dialogService.DisplayActionSheetAsync(_resourceContainer.GetResourceString("AlertQuickBudget"), buttons.ToArray());
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertQuickBudget"), _resourceContainer.GetResourceString("AlertQuickBudgetNotEnoughData"), _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertGeneralError"), quickBudgetResult.Message, _resourceContainer.GetResourceString("AlertOk"));
            }
        }
    }
}
