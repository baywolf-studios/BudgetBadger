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

namespace BudgetBadger.Forms.Accounts
{
    public class AccountsPageViewModel : BindableBase, INavigationAware
    {
        readonly IAccountLogic AccountLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SearchCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        IEnumerable<Account> _accounts;
        public IEnumerable<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        Account _selectedAccount;
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set => SetProperty(ref _selectedAccount, value);
        }

        ILookup<string, Account> _groupedAccounts;
        public ILookup<string, Account> GroupedAccounts
        {
            get => _groupedAccounts;
            set => SetProperty(ref _groupedAccounts, value);
        }

        bool _selectorMode;
        public bool SelectorMode
        {
            get => _selectorMode;
            set => SetProperty(ref _selectorMode, value);
        }

        public bool NormalMode { get => !SelectorMode; }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); ExecuteSearchCommand(); }
        }

        public AccountsPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IAccountLogic accountLogic)
        {
            AccountLogic = accountLogic;
            NavigationService = navigationService;
            DialogService = dialogService;

            Accounts = new List<Account>();
            SelectedAccount = null;
            GroupedAccounts = Accounts.ToLookup(a => "");

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
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
            if (SelectedAccount == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Account, SelectedAccount }
            };

            if (SelectorMode)
            {
                await NavigationService.GoBackAsync(parameters);
            }
            else
            {
                await NavigationService.NavigateAsync(PageName.AccountInfoPage, parameters);
            }


            SelectedAccount = null;
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
                var result = await AccountLogic.GetAccountsAsync();
                if (result.Success)
                {
                    Accounts = result.Data;
                    GroupedAccounts = AccountLogic.GroupAccounts(Accounts);
                }
                else
                {
                    await Task.Yield();
                    await DialogService.DisplayAlertAsync("Error", result.Message, "Okay");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteAddCommand()
        {
            await NavigationService.NavigateAsync(PageName.AccountEditPage);
            SelectedAccount = null;
        }

        public void ExecuteSearchCommand()
        {
            GroupedAccounts = AccountLogic.GroupAccounts(AccountLogic.SearchAccounts(Accounts, SearchText));
        }
    }
}
