using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using Prism.AppModel;
using BudgetBadger.Core.Sync;
using Prism;
using System.Collections.ObjectModel;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Core.LocalizedResources;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeGroupsPageViewModel : BindableBase, IPageLifecycleAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly IEnvelopeLogic _envelopeGroupLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISyncFactory _syncFactory;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public Predicate<object> Filter { get => (envelopeGroup) => _envelopeGroupLogic.FilterEnvelopeGroup((EnvelopeGroup)envelopeGroup, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        IReadOnlyList<EnvelopeGroup> _envelopeGroups;
        public IReadOnlyList<EnvelopeGroup> EnvelopeGroups
        {
            get => _envelopeGroups;
            set => SetProperty(ref _envelopeGroups, value);
        }

        EnvelopeGroup _selectedEnvelopeGroup;
        public EnvelopeGroup SelectedEnvelopeGroup
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

        public EnvelopeGroupsPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
                                   IPageDialogService dialogService,
                                   IEnvelopeLogic envelopeGroupLogic,
                                   ISyncFactory syncFactory)
        {
            _resourceContainer = resourceContainer;
            _envelopeGroupLogic = envelopeGroupLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _syncFactory = syncFactory;

            EnvelopeGroups = new List<EnvelopeGroup>();
            SelectedEnvelopeGroup = null;

            SelectedCommand = new DelegateCommand<EnvelopeGroup>(async p => await ExecuteSelectedCommand(p));
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<EnvelopeGroup>(async a => await ExecuteEditCommand(a));
            DeleteCommand = new DelegateCommand<EnvelopeGroup>(async a => await ExecuteDeleteCommand(a));
        }

        public async void OnAppearing()
        {
            await ExecuteRefreshCommand();
        }

        public void OnDisappearing()
        {
        }

        public async Task ExecuteSelectedCommand(EnvelopeGroup envelopeGroup)
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

        public async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var result = await _envelopeGroupLogic.GetEnvelopeGroupsAsync();

                if (result.Success)
                {
                    EnvelopeGroups = result.Data;
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

        public async Task ExecuteSaveCommand()
        {
            var newEnvelopeGroup = new EnvelopeGroup
            {
                Description = SearchText
            };

            var result = await _envelopeGroupLogic.SaveEnvelopeGroupAsync(newEnvelopeGroup);

            if (result.Success)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.EnvelopeGroup, result.Data }
                };

                await ExecuteRefreshCommand();
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertAveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage);

            SelectedEnvelopeGroup = null;
        }

        public async Task ExecuteEditCommand(EnvelopeGroup envelopeGroup)
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.EnvelopeGroup, envelopeGroup }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage, parameters);
        }

        public async Task ExecuteDeleteCommand(EnvelopeGroup envelopeGroup)
        {
            var result = await _envelopeGroupLogic.DeleteEnvelopeGroupAsync(envelopeGroup.Id);

            if (result.Success)
            {
                await ExecuteRefreshCommand();

                var syncService = _syncFactory.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.SetLastSyncDateTime(DateTime.Now);
                }
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
            }
        }
    }
}

