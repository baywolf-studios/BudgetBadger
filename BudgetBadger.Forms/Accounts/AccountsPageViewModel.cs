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
using Prism.AppModel;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountsPageViewModel : BindableBase, INavigatingAware, IPageLifecycleAware
    {
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
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
            _accountLogic = accountLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Accounts = new List<Account>();
            SelectedAccount = null;
            GroupedAccounts = Accounts.ToLookup(a => "");

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<Account>(async a => await ExecuteEditCommand(a));
            DeleteCommand = new DelegateCommand<Account>(async a => await ExecuteDeleteCommand(a));
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            // returns default bool if none present
            SelectorMode = parameters.GetValue<bool>(PageParameter.SelectorMode);

            await ExecuteRefreshCommand();
        }

        public async void OnAppearing()
        {
            await ExecuteRefreshCommand();
        }

        public void OnDisappearing()
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
                await _navigationService.GoBackAsync(parameters);
            }
            else
            {
                await _navigationService.NavigateAsync(PageName.AccountInfoPage, parameters);
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
                var result = await _accountLogic.GetAccountsAsync();
                if (result.Success)
                {
                    Accounts = result.Data;
                    GroupedAccounts = _accountLogic.GroupAccounts(Accounts);
                }
                else
                {
                    await Task.Yield();
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "Okay");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.AccountEditPage);
            SelectedAccount = null;
        }

        public async Task ExecuteEditCommand(Account account)
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Account, account }
            };
            await _navigationService.NavigateAsync(PageName.AccountEditPage, parameters);
        }

        public async Task ExecuteDeleteCommand(Account account)
        {
            var result = await _accountLogic.DeleteAccountAsync(account.Id);

            if (result.Success)
            {
                await ExecuteRefreshCommand();
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
            }
        }

        public void ExecuteSearchCommand()
        {
            GroupedAccounts = _accountLogic.GroupAccounts(_accountLogic.SearchAccounts(Accounts, SearchText));
        }
    }
}
