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
    public class EnvelopeEditPageViewModel : BindableBase, INavigationAware
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISync _syncService;

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        string _busyText;
        public string BusyText
        {
            get { return _busyText; }
            set { SetProperty(ref _busyText, value); }
        }

        Budget _budget;
        public Budget Budget
        { 
            get { return _budget; }
            set { SetProperty(ref _budget, value); }
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
            //DeleteCommand = new DelegateCommand(async () => await ExecuteDeleteCommand());
        }

        public async Task ExecuteSaveCommand()
        {
            IsBusy = true;

            try
            {
                BusyText = "Saving";
                var result = await _envelopeLogic.SaveBudgetAsync(Budget);

                if (result.Success)
                {
                    BusyText = "Syncing";
                    var syncResult = await _syncService.FullSync();
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }
                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteGroupSelectedCommand()
        {
            await _navigationService.NavigateAsync(PageName.EnvelopeGroupsPage);
        }

        public void OnNavigatingTo(NavigationParameters parameters)
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
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }
    }
}
