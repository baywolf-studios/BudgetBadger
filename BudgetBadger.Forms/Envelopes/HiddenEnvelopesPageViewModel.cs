using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
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
    public class HiddenEnvelopesPageViewModel : BindableBase, INavigationAware, IInitializeAsync
    {
        readonly IResourceContainer _resourceContainer;
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
        public ICommand RefreshEnvelopeCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
        public Predicate<object> Filter { get => (env) => _envelopeLogic.FilterEnvelope((Envelope)env, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        ObservableList<Envelope> _envelopes;
        public ObservableList<Envelope> Envelopes
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

        public HiddenEnvelopesPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
            IEnvelopeLogic envelopeLogic,
            IPageDialogService dialogService)
        {
            _resourceContainer = resourceContainer;
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Envelopes = new ObservableList<Envelope>();
            SelectedEnvelope = null;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            RefreshEnvelopeCommand = new DelegateCommand<Envelope>(async e => await ExecuteRefreshEnvelopeCommand(e));
            SelectedCommand = new DelegateCommand<Envelope>(async e => await ExecuteSelectedCommand(e));
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue(PageParameter.Envelope, out Envelope envelope))
            {
                await ExecuteRefreshEnvelopeCommand(envelope);
            }

            if (parameters.TryGetValue(PageParameter.Transaction, out Transaction transaction))
            {
                await ExecuteRefreshEnvelopeCommand(transaction.Envelope);
            }
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedEnvelope = null;
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
                var result = await _envelopeLogic.GetHiddenEnvelopesAsync();

                if (result.Success)
                {
                    Envelopes.ReplaceRange(result.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }

                NoEnvelopes = (Envelopes?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteRefreshEnvelopeCommand(Envelope envelope)
        {
            var envelopeToRemove = Envelopes.FirstOrDefault(a => a.Id == envelope.Id);
            Envelopes.Remove(envelopeToRemove);
            Envelopes.Add(envelope);
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

