using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using PropertyChanged;

namespace BudgetBadger.Forms.Payees
{
    [AddINotifyPropertyChangedInterface]
    public class PayeesPageViewModel : INavigationAware
    {
        readonly IPayeeLogic PayeeLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand AddCommand { get; set; }

        public bool IsBusy { get; set; }

        public ObservableCollection<Payee> Payees { get; set; }
        public Payee SelectedPayee { get; set; }
        public ObservableCollection<GroupedList<Payee>> GroupedPayees { get; set; }

        public bool SelectorMode { get; set; }
        public bool MainMode { get { return !SelectorMode; }}

        public bool NoSearchResults { get { return !string.IsNullOrWhiteSpace(SearchText) && GroupedPayees.Count == 0; } }
        public string SearchText { get; set; }
        public void OnSearchTextChanged()
        {
            ExecuteSearchCommand();
        }

        public PayeesPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IPayeeLogic payeeLogic)
        {
            PayeeLogic = payeeLogic;
            NavigationService = navigationService;
            DialogService = dialogService;

            Payees = new ObservableCollection<Payee>();
            SelectedPayee = null;
            GroupedPayees = new ObservableCollection<GroupedList<Payee>>();

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
            NewCommand = new DelegateCommand(async () => await ExecuteNewCommand());
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            // returns default bool if none present
            SelectorMode = parameters.GetValue<bool>(NavigationParameterType.SelectorMode);

            await ExecuteRefreshCommand();
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
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
                    GroupedPayees = new ObservableCollection<GroupedList<Payee>>(PayeeLogic.GroupPayees(Payees));
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

        public async Task ExecuteAddCommand()
        {
            var newPayee = new Payee
            {
                Description = SearchText
            };

            var result = await PayeeLogic.UpsertPayeeAsync(newPayee);

            if (result.Success)
            {
                var parameters = new NavigationParameters
                {
                    { NavigationParameterType.Payee, result.Data }
                };

                await NavigationService.GoBackAsync(parameters);
            }
            else
            {
                //show error
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
    }
}

