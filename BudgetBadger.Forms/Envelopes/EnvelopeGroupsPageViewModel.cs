using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using PropertyChanged;

namespace BudgetBadger.Forms.Envelopes
{
    [AddINotifyPropertyChangedInterface]
    public class EnvelopeGroupsPageViewModel : INavigationAware
    {
        readonly IEnvelopeLogic EnvelopeLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NewCommand { get; set; }

        public bool IsBusy { get; set; }

        public EnvelopeGroup SelectedEnvelopeGroup { get; set; }
        public ObservableCollection<EnvelopeGroup> EnvelopeGroups { get; set; }
        public ObservableCollection<EnvelopeGroup> FilteredEnvelopeGroups { get; set; }

        public bool NoSearchResults { get { return !string.IsNullOrWhiteSpace(SearchText) && FilteredEnvelopeGroups.Count == 0; } }
        public string SearchText { get; set; }
        public void OnSearchTextChanged()
        {
            ExecuteSearchCommand();
        }

        public EnvelopeGroupsPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IEnvelopeLogic envelopeLogic)
        {
            NavigationService = navigationService;
            DialogService = dialogService;
            EnvelopeLogic = envelopeLogic;

            SelectedEnvelopeGroup = null;
            EnvelopeGroups = new ObservableCollection<EnvelopeGroup>();
            FilteredEnvelopeGroups = new ObservableCollection<EnvelopeGroup>();

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            NewCommand = new DelegateCommand(async () => await ExecuteNewCommand());
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
                    EnvelopeGroups = new ObservableCollection<EnvelopeGroup>(envelopeGroupsResult.Data);
                    FilteredEnvelopeGroups = new ObservableCollection<EnvelopeGroup>(envelopeGroupsResult.Data);
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
                { NavigationParameterType.EnvelopeGroup, SelectedEnvelopeGroup }
            };

            await NavigationService.GoBackAsync(parameters);

            SelectedEnvelopeGroup = null;
        }

        public async Task ExecuteNewCommand()
        {
            var newEnvelopeGroup = new EnvelopeGroup
            {
                Description = SearchText
            };

            var result = await EnvelopeLogic.UpsertEnvelopeGroupAsync(newEnvelopeGroup);

            if (result.Success)
            {
                var parameters = new NavigationParameters
                {
                    { NavigationParameterType.EnvelopeGroup, result.Data }
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
            FilteredEnvelopeGroups = new ObservableCollection<EnvelopeGroup>(EnvelopeLogic.SearchEnvelopeGroups(EnvelopeGroups, SearchText));
        }
    }
}
