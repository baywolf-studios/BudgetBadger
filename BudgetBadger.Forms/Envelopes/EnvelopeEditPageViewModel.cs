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
            set => SetProperty(ref _budget, value);
        }

        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
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
            DeleteCommand = new DelegateCommand(async () => await ExecuteDeleteCommand());
        }
        
        public async void OnNavigatingTo(NavigationParameters parameters)
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
			else if (!Budget.Schedule.IsActive)
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

        public async Task ExecuteDeleteCommand()
        {
            var result = await _envelopeLogic.DeleteEnvelopeAsync(Budget.Envelope.Id);
            if (result.Success)
            {
                await _navigationService.GoBackToRootAsync();
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
            }
        }
    }
}
