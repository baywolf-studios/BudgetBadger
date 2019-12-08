using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeTransferPageViewModel : BindableBase, INavigationAware, IInitializeAsync
    {
        readonly IResourceContainer _resourceContainer;
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISyncFactory _syncFactory;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand FromEnvelopeSelectedCommand { get; set; }
        public ICommand ToEnvelopeSelectedCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        bool _needToSync;
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
                                      ISyncFactory syncFactory)
        {
            _resourceContainer = resourceContainer;
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _syncFactory = syncFactory;

            _fromEnvelopeRequested = false;
            FromEnvelope = new Envelope();
            ToEnvelope = new Envelope();
            Schedule = new BudgetSchedule();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            FromEnvelopeSelectedCommand = new DelegateCommand(async () => await ExecuteFromEnvelopeSelectedCommand());
            ToEnvelopeSelectedCommand = new DelegateCommand(async () => await ExecuteToEnvelopeSelectedCommand());
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
                _needToSync = true;
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
            }
        }


    }
}
