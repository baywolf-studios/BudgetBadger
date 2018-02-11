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
using PropertyChanged;
using System.Collections.Generic;

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
        public ICommand SaveCommand { get; set; }

        public bool IsBusy { get; set; }

        public EnvelopeGroup SelectedEnvelopeGroup { get; set; }
        public IEnumerable<EnvelopeGroup> EnvelopeGroups { get; set; }
        public IEnumerable<EnvelopeGroup> FilteredEnvelopeGroups { get; set; }

        public bool NoSearchResults { get { return !string.IsNullOrWhiteSpace(SearchText) && FilteredEnvelopeGroups.Count() == 0; } }
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
