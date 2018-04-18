using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using Prism.Mvvm;
using BudgetBadger.Core.Sync;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeGroupsPageViewModel : BindableBase, INavigatingAware
    {
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISync _syncService;

        public ICommand SelectedCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        string _busyText;
        public string BusyText
        {
            get => _busyText;
            set => SetProperty(ref _busyText, value);
        }

        EnvelopeGroup _selectedEnvelopeGroup;
        public EnvelopeGroup SelectedEnvelopeGroup
        {
            get => _selectedEnvelopeGroup;
            set => SetProperty(ref _selectedEnvelopeGroup, value);
        }

        IReadOnlyList<EnvelopeGroup> _envelopeGroups;
        public IReadOnlyList<EnvelopeGroup> EnvelopeGroups
        {
            get => _envelopeGroups;
            set => SetProperty(ref _envelopeGroups, value);
        }

        IReadOnlyList<EnvelopeGroup> _filteredEnvelopeGroups;
        public IReadOnlyList<EnvelopeGroup> FilteredEnvelopeGroups
        {
            get => _filteredEnvelopeGroups;
            set => SetProperty(ref _filteredEnvelopeGroups, value);
        }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); RaisePropertyChanged(nameof(HasSearchText)); }
        }

        public bool HasSearchText { get => !string.IsNullOrWhiteSpace(SearchText); }

        public EnvelopeGroupsPageViewModel(INavigationService navigationService,
                                           IPageDialogService dialogService,
                                           IEnvelopeLogic envelopeLogic,
                                           ISync syncService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _envelopeLogic = envelopeLogic;
            _syncService = syncService;

            SelectedEnvelopeGroup = null;
            EnvelopeGroups = new List<EnvelopeGroup>();
            FilteredEnvelopeGroups = new List<EnvelopeGroup>();

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
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
                var envelopeGroupsResult = await _envelopeLogic.GetEnvelopeGroupsAsync();

                if (envelopeGroupsResult.Success)
                {
                    EnvelopeGroups = envelopeGroupsResult.Data;
                    FilteredEnvelopeGroups = envelopeGroupsResult.Data;
                }
                else
                {
                    //show error
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSelectedCommand()
        {
            if (SelectedEnvelopeGroup == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.EnvelopeGroup, SelectedEnvelopeGroup }
            };

            await _navigationService.GoBackAsync(parameters);

            SelectedEnvelopeGroup = null;
        }

        public async Task ExecuteSaveCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var newEnvelopeGroup = new EnvelopeGroup
                {
                    Description = SearchText
                };

                BusyText = "Saving";
                var result = await _envelopeLogic.SaveEnvelopeGroupAsync(newEnvelopeGroup);

                if (result.Success)
                {

                    BusyText = "Syncing";
                    var syncTask = _syncService.FullSync();
                    var parameters = new NavigationParameters
                    {
                        { PageParameter.EnvelopeGroup, result.Data }
                    };

                    await _navigationService.GoBackAsync(parameters);

                    var syncResult = await syncTask;
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }


                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExecuteSearchCommand()
        {
            FilteredEnvelopeGroups = _envelopeLogic.SearchEnvelopeGroups(EnvelopeGroups, SearchText);
        }
    }
}
