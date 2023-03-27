﻿using System;
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
    public class EnvelopeGroupsPageViewModel : ObservableBase, INavigationAware, IInitializeAsync
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<IEnvelopeLogic> _envelopeGroupLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand RefreshEnvelopeGroupCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SaveSearchCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public Predicate<object> Filter { get => (envelopeGroup) => _envelopeGroupLogic.Value.FilterEnvelopeGroup((EnvelopeGroupModel)envelopeGroup, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        ObservableList<EnvelopeGroupModel> _envelopeGroups;
        public ObservableList<EnvelopeGroupModel> EnvelopeGroups
        {
            get => _envelopeGroups;
            set => SetProperty(ref _envelopeGroups, value);
        }

        EnvelopeGroupModel _selectedEnvelopeGroup;
        public EnvelopeGroupModel SelectedEnvelopeGroup
        {
            get => _selectedEnvelopeGroup;
            set => SetProperty(ref _selectedEnvelopeGroup, value);
        }

        public bool HasSearchText { get => !string.IsNullOrWhiteSpace(SearchText); }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); RaisePropertyChanged(nameof(HasSearchText)); }
        }

        bool _noEnvelopeGroups;
        public bool NoEnvelopeGroups
        {
            get => _noEnvelopeGroups;
            set => SetProperty(ref _noEnvelopeGroups, value);
        }

        public EnvelopeGroupsPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                   INavigationService navigationService,
                                   IPageDialogService dialogService,
                                   Lazy<IEnvelopeLogic> envelopeGroupLogic,
                                   IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _envelopeGroupLogic = envelopeGroupLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;

            EnvelopeGroups = new ObservableList<EnvelopeGroupModel>();
            SelectedEnvelopeGroup = null;

            SelectedCommand = new Command<EnvelopeGroupModel>(async p => await ExecuteSelectedCommand(p));
            RefreshCommand = new Command(async () => await FullRefresh());
            RefreshEnvelopeGroupCommand = new Command<EnvelopeGroupModel>(RefreshEnvelopeGroup);
            SaveSearchCommand = new Command(async () => await ExecuteSaveSearchCommand());
            SaveCommand = new Command<EnvelopeGroupModel>(async p => await ExecuteSaveCommand(p));
            AddCommand = new Command(async () => await ExecuteAddCommand());
            EditCommand = new Command<EnvelopeGroupModel>(async a => await ExecuteEditCommand(a));

            _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Subscribe(RefreshEnvelopeGroup);
            _eventAggregator.GetEvent<EnvelopeGroupDeletedEvent>().Subscribe(RefreshEnvelopeGroup);
            _eventAggregator.GetEvent<EnvelopeGroupHiddenEvent>().Subscribe(RefreshEnvelopeGroup);
            _eventAggregator.GetEvent<EnvelopeGroupUnhiddenEvent>().Subscribe(RefreshEnvelopeGroup);

            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshEnvelopeGroupFromTransaction(t));
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(async t => await RefreshEnvelopeGroupFromTransaction(t));
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedEnvelopeGroup = null;
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            await FullRefresh();
        }

        public async Task ExecuteSelectedCommand(EnvelopeGroupModel envelopeGroup)
        {
            if (envelopeGroup == null)
            {
                return;
            }

            if (envelopeGroup.IsGenericHiddenEnvelopeGroup)
            {
                await _navigationService.NavigateAsync(PageName.HiddenEnvelopeGroupsPage);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.EnvelopeGroup, envelopeGroup }
                };

                await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage, parameters);
            }
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
                var result = await _envelopeGroupLogic.Value.GetEnvelopeGroupsAsync();

                if (result.Success)
                {
                    EnvelopeGroups.ReplaceRange(result.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }

                NoEnvelopeGroups = (EnvelopeGroups?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSaveSearchCommand()
        {
            var newEnvelopeGroup = new EnvelopeGroupModel
            {
                Description = SearchText
            };

            var result = await _envelopeGroupLogic.Value.SaveEnvelopeGroupAsync(newEnvelopeGroup);

            if (result.Success)
            {
                _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Publish(result.Data);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertAveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteSaveCommand(EnvelopeGroupModel envelopeGroup)
        {
            var result = await _envelopeGroupLogic.Value.SaveEnvelopeGroupAsync(envelopeGroup);

            if (result.Success)
            {
                _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Publish(result.Data);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage);

        }

        public async Task ExecuteEditCommand(EnvelopeGroupModel envelopeGroup)
        {
            if (!envelopeGroup.IsGenericHiddenEnvelopeGroup)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.EnvelopeGroup, envelopeGroup }
                };
                await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage, parameters);
            }
        }

        public void RefreshEnvelopeGroup(EnvelopeGroupModel envelopeGroup)
        {
            var envelopeGroups = EnvelopeGroups.Where(a => a.Id != envelopeGroup.Id).ToList();

            if (envelopeGroup != null && _envelopeGroupLogic.Value.FilterEnvelopeGroup(envelopeGroup, FilterType.Editable))
            {
                envelopeGroups.Add(envelopeGroup);
            }

            EnvelopeGroups.ReplaceRange(envelopeGroups);
        }

        public async Task RefreshEnvelopeGroupFromTransaction(Transaction transaction)
        {
            if (transaction != null && transaction.Envelope != null)
            {
                var updatedGroupResult = await _envelopeGroupLogic.Value.GetEnvelopeGroupAsync(transaction.Envelope.Group.Id);
                if (updatedGroupResult.Success)
                {
                    RefreshEnvelopeGroup(updatedGroupResult.Data);
                }
            }
        }
    }
}

