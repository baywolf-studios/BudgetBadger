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
    public class DeletedEnvelopesPageViewModel : BindableBase, INavigatingAware
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
        public ICommand SearchCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        IReadOnlyList<Envelope> _envelopes;
        public IReadOnlyList<Envelope> Envelopes
        {
            get => _envelopes;
            set => SetProperty(ref _envelopes, value);
        }

        IReadOnlyList<Envelope> _filteredEnvelopes;
        public IReadOnlyList<Envelope> FilteredEnvelopes
        {
            get => _filteredEnvelopes;
            set => SetProperty(ref _filteredEnvelopes, value);
        }

        Envelope _selectedEnvelope;
        public Envelope SelectedEnvelope
        {
            get => _selectedEnvelope;
            set => SetProperty(ref _selectedEnvelope, value);
        }

        bool _noEnvelopes;
        public bool NoEnvelopes
        {
            get => _noEnvelopes;
            set => SetProperty(ref _noEnvelopes, value);
        }

        public DeletedEnvelopesPageViewModel(INavigationService navigationService, IEnvelopeLogic envelopeLogic, IPageDialogService dialogService)
        {
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Envelopes = new List<Envelope>();
            SelectedEnvelope = null;
            FilteredEnvelopes = new List<Envelope>();

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            SearchCommand = new DelegateCommand<string>(ExecuteSearchCommand);
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
			await ExecuteRefreshCommand();
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
                var result = await _envelopeLogic.GetDeletedEnvelopesAsync();

                if (result.Success)
                {
                    Envelopes = result.Data;
                    FilteredEnvelopes = result.Data;
                }
                else
                {
                    await Task.Yield();
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "OK");
                }

                NoEnvelopes = (FilteredEnvelopes?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSelectedCommand()
        {
            if (SelectedEnvelope == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Envelope, SelectedEnvelope }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeEditPage, parameters);


            SelectedEnvelope = null;
        }


        public void ExecuteSearchCommand(string searchText)
        {
            FilteredEnvelopes = _envelopeLogic.SearchEnvelopes(Envelopes, searchText);
        }
    }
}

