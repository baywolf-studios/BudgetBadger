using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using BudgetBadger.Forms.ViewModels;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeGroupsPageViewModel : BaseViewModel
    {
        readonly IEnvelopeLogic EnvelopeLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public EnvelopeGroup SelectedEnvelopeGroup { get; set; }
        public ObservableCollection<EnvelopeGroup> EnvelopeGroups { get; set; }
        public ObservableCollection<GroupedList<EnvelopeGroup>> GroupedEnvelopeGroups { get; set; }
        public string SearchText { get; set; }
        public void OnSearchTextChanged()
        {
            ExecuteSearchCommand();
        }

        public EnvelopeGroupsPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IEnvelopeLogic envelopeLogic)
        {
            Title = "New Payee";
            NavigationService = navigationService;
            DialogService = dialogService;
            EnvelopeLogic = envelopeLogic;

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
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
                    //GroupedEnvelopeGroups = new ObservableCollection<GroupedList<Budget>>(EnvelopeLogic.GroupBudgets(Budgets));
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

        public void ExecuteSearchCommand()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                RefreshCommand.Execute(null);
            }
            else
            {
                var results = EnvelopeGroups.Where(e => e.Description.Contains(SearchText));

                EnvelopeGroups = new ObservableCollection<EnvelopeGroup>(results);
            }
        }

        public override async void OnNavigatingTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }
    }
}
