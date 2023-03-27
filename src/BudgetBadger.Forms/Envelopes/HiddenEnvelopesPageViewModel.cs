using System;
using System.Linq;
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
    public class HiddenEnvelopesPageViewModel : ObservableBase, INavigationAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
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
            IPageDialogService dialogService,
            IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _envelopeLogic = envelopeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;

            Envelopes = new ObservableList<Envelope>();
            SelectedEnvelope = null;

            RefreshCommand = new Command(async () => await FullRefresh());
            SelectedCommand = new Command<Envelope>(async e => await ExecuteSelectedCommand(e));

            _eventAggregator.GetEvent<BudgetSavedEvent>().Subscribe(b => RefreshEnvelope(b.Envelope));
            _eventAggregator.GetEvent<EnvelopeDeletedEvent>().Subscribe(RefreshEnvelope);
            _eventAggregator.GetEvent<EnvelopeHiddenEvent>().Subscribe(RefreshEnvelope);
            _eventAggregator.GetEvent<EnvelopeUnhiddenEvent>().Subscribe(RefreshEnvelope);

            _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Subscribe(async b => await FullRefresh());
            _eventAggregator.GetEvent<EnvelopeGroupDeletedEvent>().Subscribe(async b => await FullRefresh());
            _eventAggregator.GetEvent<EnvelopeGroupHiddenEvent>().Subscribe(async b => await FullRefresh());
            _eventAggregator.GetEvent<EnvelopeGroupUnhiddenEvent>().Subscribe(async b => await FullRefresh());

            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshEnvelopeFromTransaction(t));
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(async t => await RefreshEnvelopeFromTransaction(t));
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            await FullRefresh();
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedEnvelope = null;
        }

        public async Task FullRefresh()
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

        public void RefreshEnvelope(Envelope envelope)
        {
            var envelopes = Envelopes.Where(a => a.Id != envelope.Id).ToList();

            if (envelope != null && _envelopeLogic.FilterEnvelope(envelope, Core.Logic.FilterType.Hidden))
            {
                envelopes.Add(envelope);
            }

            Envelopes.ReplaceRange(envelopes);
        }

        public async Task RefreshEnvelopeFromTransaction(Transaction transaction)
        {
            if (transaction != null && transaction.Payee != null)
            {
                var updatedEnvelopeResult = await _envelopeLogic.GetEnvelopeAsync(transaction.Envelope.Id);
                if (updatedEnvelopeResult.Success)
                {
                    RefreshEnvelope(updatedEnvelopeResult.Data);
                }
            }
        }
    }
}

