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

namespace BudgetBadger.Forms.Payees
{
    public class PayeesPageViewModel : BindableBase, INavigationAware
    {
        readonly IPayeeLogic PayeeLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        IEnumerable<Payee> _payees;
        public IEnumerable<Payee> Payees
        {
            get { return _payees; }
            set { SetProperty(ref _payees, value); }
        }

        Payee _selectedPayee;
        public Payee SelectedPayee
        {
            get { return _selectedPayee; }
            set { SetProperty(ref _selectedPayee, value); }
        }

        ILookup<string, Payee> _groupedPayees;
        public ILookup<string, Payee> GroupedPayees
        {
            get { return _groupedPayees; }
            set { SetProperty(ref _groupedPayees, value); }
        }

        bool _selectorMode;
        public bool SelectorMode
        {
            get { return _selectorMode; }
            set { SetProperty(ref _selectorMode, value); }
        }

        public bool MainMode { get { return !SelectorMode; }}

        public bool NoSearchResults { get { return !string.IsNullOrWhiteSpace(SearchText) && GroupedPayees.Count == 0; } }

        string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); ExecuteSearchCommand(); }
        }

        public PayeesPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IPayeeLogic payeeLogic)
        {
            PayeeLogic = payeeLogic;
            NavigationService = navigationService;
            DialogService = dialogService;

            Payees = new List<Payee>();
            SelectedPayee = null;
            GroupedPayees = Payees.ToLookup(p => "");

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            // returns default bool if none present
            SelectorMode = parameters.GetValue<bool>(PageParameter.SelectorMode);

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
                { PageParameter.Payee, SelectedPayee }
            };

            if (SelectorMode)
            {
                await NavigationService.GoBackAsync(parameters);
            }
            else
            {
                await NavigationService.NavigateAsync(PageName.PayeeInfoPage, parameters);
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
                    Payees = result.Data;
                    GroupedPayees = PayeeLogic.GroupPayees(Payees);
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

        public async Task ExecuteSaveCommand()
        {
            var newPayee = new Payee
            {
                Description = SearchText
            };

            var result = await PayeeLogic.SavePayeeAsync(newPayee);

            if (result.Success)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Payee, result.Data }
                };

                await NavigationService.GoBackAsync(parameters);
            }
            else
            {
                //show error
            }
        }

        public async Task ExecuteAddCommand()
        {
            await NavigationService.NavigateAsync(PageName.PayeeEditPage);

            SelectedPayee = null;
        }

        public void ExecuteSearchCommand()
        {
            GroupedPayees = PayeeLogic.GroupPayees(PayeeLogic.SearchPayees(Payees, SearchText));
        }
    }
}

