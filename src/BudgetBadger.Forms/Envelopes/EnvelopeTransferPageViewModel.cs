using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Events;
using BudgetBadger.Core.Models;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeTransferPageViewModel : ObservableBase, INavigationAware, IInitializeAsync
    {
        readonly IResourceContainer _resourceContainer;
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand FromEnvelopeSelectedCommand { get; set; }
        public ICommand ToEnvelopeSelectedCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        bool _fromEnvelopeRequested;

        Envelope _fromEnvelope;
        public Envelope FromEnvelope
        {
            get => _fromEnvelope;
            set => SetProperty(ref _fromEnvelope, value);
        }

        Envelope _toEnvelope;
        public Envelope ToEnvelope
        {
            get => _toEnvelope;
            set => SetProperty(ref _toEnvelope, value);
        }

        decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        BudgetSchedule _schedule;
        public BudgetSchedule Schedule
        {
            get => _schedule;
            set => SetProperty(ref _schedule, value);
        }

        public EnvelopeTransferPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
                                      IEnvelopeLogic envelopeLogic,
                                      IPageDialogService dialogService,
                                      IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            
            _fromEnvelopeRequested = false;
            FromEnvelope = new Envelope();
            ToEnvelope = new Envelope();
            Schedule = new BudgetSchedule();

            SaveCommand = new Command(async () => await ExecuteSaveCommand());
            FromEnvelopeSelectedCommand = new Command(async () => await ExecuteFromEnvelopeSelectedCommand());
            ToEnvelopeSelectedCommand = new Command(async () => await ExecuteToEnvelopeSelectedCommand());
        }

        public  void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back)
            {
                await InitializeAsync(parameters);
            }
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var envelope = parameters.GetValue<Envelope>(PageParameter.Envelope);
            if (envelope != null)
            {
                if (_fromEnvelopeRequested)
                {
                    FromEnvelope = envelope.DeepCopy();
                }
                else
                {
                    ToEnvelope = envelope.DeepCopy();
                }
            }

            var schedule = parameters.GetValue<BudgetSchedule>(PageParameter.BudgetSchedule);
            if (schedule != null)
            {
                Schedule = schedule.DeepCopy();
            }
            else if (Schedule.Id == Guid.Empty)
            {
                var scheduleResult = await _envelopeLogic.GetCurrentBudgetScheduleAsync();
                if (scheduleResult.Success)
                {
                    Schedule = scheduleResult.Data;
                }
            }
        }

        public async Task ExecuteFromEnvelopeSelectedCommand()
        {
            _fromEnvelopeRequested = true;

            var parameters = new NavigationParameters
            {
                { PageParameter.TransferEnvelopeSelection, true }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeSelectionPage, parameters);
        }

        public async Task ExecuteToEnvelopeSelectedCommand()
        {
            _fromEnvelopeRequested = false;

            var parameters = new NavigationParameters
            {
                { PageParameter.TransferEnvelopeSelection, true }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeSelectionPage, parameters);
        }

        public async Task ExecuteSaveCommand()
        {
            var result = await _envelopeLogic.BudgetTransferAsync(Schedule, FromEnvelope.Id, ToEnvelope.Id, Amount);

            if (result.Success)
            {
                foreach (var budget in result.Data)
                {
                    _eventAggregator.GetEvent<BudgetSavedEvent>().Publish(budget);
                }

                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
            }
        }


    }
}
