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

namespace BudgetBadger.Forms.Payees
{
    public class PayeesPageViewModel : BaseViewModel, INavigationAware
    {
        readonly IPayeeLogic PayeeLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand SearchCommand { get; set; }

        public ObservableCollection<Payee> Payees { get; set; }
        public Payee SelectedPayee { get; set; }
        public ObservableCollection<GroupedList<Payee>> GroupedPayees { get; set; }
        public bool SelectorMode { get; set; }
        public bool NormalMode { get { return !SelectorMode; }}

        public string SearchText { get; set; }
        public void OnSearchTextChanged()
        {
            ExecuteSearchCommand();
        }

        public PayeesPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IPayeeLogic payeeLogic)
        {
            Title = "Payees";
            PayeeLogic = payeeLogic;
            NavigationService = navigationService;
            DialogService = dialogService;

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            NewCommand = new DelegateCommand(async () => await ExecuteNewCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
        }


        public async Task ExecuteSelectedCommand()
        {
            if (SelectedPayee == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { NavigationParameterType.Payee, SelectedPayee }
            };

            if (SelectorMode)
            {
                await NavigationService.GoBackAsync(parameters);
            }
            else
            {
                await NavigationService.NavigateAsync(NavigationPageName.PayeeInfoPage, parameters);
            }


            SelectedPayee = null;
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
                var result = await PayeeLogic.GetPayeesAsync();
                if (result.Success)
                {
                    Payees = new ObservableCollection<Payee>(result.Data);
                    GroupedPayees = new ObservableCollection<GroupedList<Payee>>(PayeeLogic.GroupPayees(Payees, true));
                }
                else
                {
                    await Task.Yield();
                    await DialogService.DisplayAlertAsync("Error", result.Message, "Ok");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteNewCommand()
        {
            await NavigationService.NavigateAsync(NavigationPageName.PayeeEditPage);

            SelectedPayee = null;
        }

        public void ExecuteSearchCommand()
        {
            GroupedPayees = new ObservableCollection<GroupedList<Payee>>(PayeeLogic.GroupPayees(PayeeLogic.SearchPayees(Payees, SearchText)));
        }

        public override  async void OnNavigatedTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public override  async void OnNavigatingTo(NavigationParameters parameters)
        {
            // returns default bool if none present
            SelectorMode = parameters.GetValue<bool>(NavigationParameterType.SelectorMode);

            await ExecuteRefreshCommand();
        }
    }
}

