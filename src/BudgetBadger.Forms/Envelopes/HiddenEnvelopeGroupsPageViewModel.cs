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
using BudgetBadger.Forms.Extensions;

namespace BudgetBadger.Forms.Envelopes
{
    public class HiddenEnvelopeGroupsPageViewModel : ObservableBase, INavigationAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public Predicate<object> Filter { get => (env) => _envelopeLogic.FilterEnvelopeGroup((EnvelopeGroupModel)env, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        EnvelopeGroupModel _selectedEnvelopeGroup;
        public EnvelopeGroupModel SelectedEnvelopeGroup
        {
            get => _selectedEnvelopeGroup;
            set => SetProperty(ref _selectedEnvelopeGroup, value);
        }

        ObservableList<EnvelopeGroupModel> _envelopeGroups;
        public ObservableList<EnvelopeGroupModel> EnvelopeGroups
        {
            get => _envelopeGroups;
            set => SetProperty(ref _envelopeGroups, value);
        }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        bool _noEnvelopeGroups;
        public bool NoEnvelopeGroups
        {
            get => _noEnvelopeGroups;
            set => SetProperty(ref _noEnvelopeGroups, value);
        }

        public HiddenEnvelopeGroupsPageViewModel(IResourceContainer resourceContainer,
                                           INavigationService navigationService,
                                           IPageDialogService dialogService,
                                           IEnvelopeLogic envelopeLogic,
                                           IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _envelopeLogic = envelopeLogic;
            _eventAggregator = eventAggregator;

            SelectedEnvelopeGroup = null;
            EnvelopeGroups = new ObservableList<EnvelopeGroupModel>();

            SelectedCommand = new Command<EnvelopeGroupModel>(async eg => await ExecuteSelectedCommand(eg));
            RefreshCommand = new Command(async () => await FullRefresh());

            _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Subscribe(RefreshEnvelopeGroup);
            _eventAggregator.GetEvent<EnvelopeGroupDeletedEvent>().Subscribe(RefreshEnvelopeGroup);
            _eventAggregator.GetEvent<EnvelopeGroupHiddenEvent>().Subscribe(RefreshEnvelopeGroup);
            _eventAggregator.GetEvent<EnvelopeGroupUnhiddenEvent>().Subscribe(RefreshEnvelopeGroup);
            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshEnvelopeGroupFromTransaction(t));
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(async t => await RefreshEnvelopeGroupFromTransaction(t));
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            await FullRefresh();
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedEnvelopeGroup = null;
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
                var result = await _envelopeLogic.GetHiddenEnvelopeGroupsAsync();

                if (result.Success)
                {
                    EnvelopeGroups.ReplaceRange(result.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }

                NoEnvelopeGroups = (EnvelopeGroups?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSelectedCommand(EnvelopeGroupModel envelopeGroup)
        {
            if (envelopeGroup == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.EnvelopeGroup, envelopeGroup }
            };

            await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage, parameters);
        }

        public void RefreshEnvelopeGroup(EnvelopeGroupModel envelopeGroup)
        {
            var envelopeGroups = EnvelopeGroups.Where(a => a.Id != envelopeGroup.Id).ToList();

            if (envelopeGroup != null && envelopeGroup.IsHidden && !envelopeGroup.IsDeleted)
            {
                envelopeGroups.Add(envelopeGroup);
            }

            EnvelopeGroups.ReplaceRange(envelopeGroups);
        }

        public async Task RefreshEnvelopeGroupFromTransaction(Transaction transaction)
        {
            if (transaction != null && transaction.Envelope != null)
            {
                var updatedGroupResult = await _envelopeLogic.GetEnvelopeGroupAsync(transaction.Envelope.Group.Id);
                if (updatedGroupResult.Success)
                {
                    RefreshEnvelopeGroup(updatedGroupResult.Data);
                }
            }
        }
    }
}
