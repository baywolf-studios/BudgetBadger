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

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeGroupsPageViewModel : BindableBase, INavigationAware
    {
        readonly IEnvelopeLogic EnvelopeLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        EnvelopeGroup _selectedEnvelopeGroup;
        public EnvelopeGroup SelectedEnvelopeGroup
        {
            get { return _selectedEnvelopeGroup; }
            set { SetProperty(ref _selectedEnvelopeGroup, value); }
        }

        IEnumerable<EnvelopeGroup> _envelopeGroups;
        public IEnumerable<EnvelopeGroup> EnvelopeGroups
        {
            get { return _envelopeGroups; }
            set { SetProperty(ref _envelopeGroups, value); }
        }

        IEnumerable<EnvelopeGroup> _filteredEnvelopeGroups;
        public IEnumerable<EnvelopeGroup> FilteredEnvelopeGroups
        {
            get { return _filteredEnvelopeGroups; }
            set { SetProperty(ref _filteredEnvelopeGroups, value); }
        }

        string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); ExecuteSearchCommand(); }
        }

        public bool NoSearchResults { get { return !string.IsNullOrWhiteSpace(SearchText) && FilteredEnvelopeGroups.Count() == 0; } }

        public EnvelopeGroupsPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IEnvelopeLogic envelopeLogic)
        {
            NavigationService = navigationService;
            DialogService = dialogService;
            EnvelopeLogic = envelopeLogic;

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

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
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
                var envelopeGroupsResult = await EnvelopeLogic.GetEnvelopeGroupsAsync();

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

            await NavigationService.GoBackAsync(parameters);

            SelectedEnvelopeGroup = null;
        }

        public async Task ExecuteSaveCommand()
        {
            var newEnvelopeGroup = new EnvelopeGroup
            {
                Description = SearchText
            };

            var result = await EnvelopeLogic.SaveEnvelopeGroupAsync(newEnvelopeGroup);

            if (result.Success)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.EnvelopeGroup, result.Data }
                };

                await NavigationService.GoBackAsync(parameters);
            }
            else
            {
                //show error
            }
        }

        public void ExecuteSearchCommand()
        {
            FilteredEnvelopeGroups = EnvelopeLogic.SearchEnvelopeGroups(EnvelopeGroups, SearchText);
        }
    }
}
