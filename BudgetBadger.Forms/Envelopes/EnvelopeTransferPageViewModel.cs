using System;
using System.Threading.Tasks;
using System.Windows.Input;
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
    public class EnvelopeTransferPageViewModel : BindableBase, INavigationAware
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISync _syncService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand FromEnvelopeSelectedCommand { get; set; }
        public ICommand ToEnvelopeSelectedCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        bool _toEnvelopeRequested { get; set; }

        Budget _fromEnvelope;
        public Budget FromEnvelope
        {
            get => _fromEnvelope;
            set => SetProperty(ref _fromEnvelope, value);
        }

        Budget _toEnvelope;
        public Budget ToEnvelope
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

        public EnvelopeTransferPageViewModel(INavigationService navigationService,
                                      IEnvelopeLogic envelopeLogic,
                                      IPageDialogService dialogService,
                                      ISync syncService)
        {
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _syncService = syncService;

            _toEnvelopeRequested = false;
            FromEnvelope = new Budget();
            ToEnvelope = new Budget();
        }

        public async void OnNavigatedFrom(INavigationParameters parameters)
        {
            await _syncService.FullSync();
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public void OnNavigatingTo(INavigationParameters parameters)
        {
            var budget = parameters.GetValue<Budget>(PageParameter.Budget);
            if (budget != null)
            {
                if (_toEnvelopeRequested)
                {
                    ToEnvelope = budget.DeepCopy();
                }
                else
                {
                    FromEnvelope = budget.DeepCopy();
                }
            }
        }

        public async Task ExecuteFromEnvelopeSelectedCommand()
        {
            _toEnvelopeRequested = false;
            await _navigationService.NavigateAsync(PageName.EnvelopeSelectionPage);
        }

        public async Task ExecuteToEnvelopeSelectedCommand()
        {
            _toEnvelopeRequested = true;
            await _navigationService.NavigateAsync(PageName.EnvelopeSelectionPage);
        }

        public async Task ExecuteSaveCommand()
        {
            var result = await _envelopeLogic.BudgetTransferAsync(FromEnvelope.Id, ToEnvelope.Id, Amount);

            if (result.Success)
            {
                await _navigationService.GoBackAsync();
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
            }
        }


    }
}
