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

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeEditPageViewModel : BindableBase, INavigatingAware
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISync _syncService;

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

        public IList<OverspendingType> OverspendingTypes
        {
            get => Enumeration.GetAll<OverspendingType>().ToList();
        }

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand QuickBudgetCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand UndoDeleteCommand { get; set; }
        public ICommand GroupSelectedCommand { get; set; }

        public EnvelopeEditPageViewModel(INavigationService navigationService,
                                         IPageDialogService dialogService,
                                         IEnvelopeLogic envelopeLogic,
                                         ISync syncService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _envelopeLogic = envelopeLogic;
            _syncService = syncService;

            Budget = new Budget();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            GroupSelectedCommand = new DelegateCommand(async () => await ExecuteGroupSelectedCommand());
            DeleteCommand = new DelegateCommand(async () => await ExecuteDeleteCommand(), CanExecuteDeleteCommand).ObservesProperty(() => Budget);
            UndoDeleteCommand = new DelegateCommand(async () => await ExecuteUndoDeleteCommand());
            QuickBudgetCommand = new DelegateCommand(async () => await ExecuteQuickBudgetCommand());
        }
        
        public async void OnNavigatingTo(INavigationParameters parameters)
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
            else if (Budget.Schedule.Id == Guid.Empty)
            {
                var result = await _envelopeLogic.GetCurrentBudgetScheduleAsync();
                if (result.Success)
                {
                    Budget.Schedule = result.Data;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "OK");
                    await _navigationService.GoBackAsync();
                }
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
                BusyText = "Saving";
                var result = await _envelopeLogic.SaveBudgetAsync(Budget);

                if (result.Success)
                {

                    BusyText = "Syncing";
                    var syncTask = _syncService.FullSync();

					var parameters = new NavigationParameters
                    {
                        { PageParameter.Budget, result.Data },
                        { PageParameter.Envelope, result.Data.Envelope }
                    };
                    await _navigationService.GoBackAsync(parameters);

                    var syncResult = await syncTask;
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }


                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
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

        public bool CanExecuteDeleteCommand()
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

        public async Task ExecuteDeleteCommand()
        {
			if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = "Deleting";
                var result = await _envelopeLogic.DeleteEnvelopeAsync(Budget.Envelope.Id);
                if (result.Success)
                {
                    BusyText = "Syncing";
                    var syncTask = _syncService.FullSync();

                    await _navigationService.GoBackToRootAsync();

                    var syncResult = await syncTask;
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
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteUndoDeleteCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = "Undoing Delete";
                var result = await _envelopeLogic.UndoDeleteEnvelopeAsync(Budget.Envelope.Id);
                if (result.Success)
                {
                    BusyText = "Syncing";
                    var syncTask = _syncService.FullSync();

                    await _navigationService.GoBackToRootAsync();

                    var syncResult = await syncTask;
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
                        var buttonText = quickBudget.Description + ": " + quickBudget.Amount.ToString("c");
                        var action = ActionSheetButton.CreateButton(buttonText, () =>
                        {
                            Budget.Amount = quickBudget.Amount;
                        });

                        buttons.Add(action);
                    }

                    buttons.Add(ActionSheetButton.CreateCancelButton("Cancel", () => { }));

                    await _dialogService.DisplayActionSheetAsync("Quick Budget", buttons.ToArray());
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Not Enough Data for Quick Budgets", quickBudgetResult.Message, "OK");
                }
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Unable to retrive Quick Budgets", quickBudgetResult.Message, "OK");
            }
        }
    }
}
