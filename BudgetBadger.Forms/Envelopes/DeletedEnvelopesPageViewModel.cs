using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Envelopes
{
    public class DeletedEnvelopesPageViewModel : BindableBase, INavigationAware
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
        public Predicate<object> Filter { get => (env) => _envelopeLogic.FilterEnvelope((Envelope)env, SearchText); }

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

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public DeletedEnvelopesPageViewModel(INavigationService navigationService, IEnvelopeLogic envelopeLogic, IPageDialogService dialogService)
        {
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Envelopes = new List<Envelope>();
            SelectedEnvelope = null;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SelectedCommand = new DelegateCommand<Envelope>(async e => await ExecuteSelectedCommand(e));
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public async void OnNavigatingTo(INavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
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
                }
                else
                {
                    await Task.Yield();
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "OK");
                }

                NoEnvelopes = (Envelopes?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSelectedCommand(Envelope envelope)
        {
            if (envelope == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Envelope, envelope }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeEditPage, parameters);
        }
    }
}

