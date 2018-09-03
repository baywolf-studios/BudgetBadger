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
using BudgetBadger.Core.Sync;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountsPageViewModel : BindableBase, INavigatingAware
    {
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
		readonly ISync _syncService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public Predicate<object> Filter { get => (ac) => _accountLogic.FilterAccount((Account)ac, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        IReadOnlyList<Account> _accounts;
        public IReadOnlyList<Account> Accounts
        {
            get => _accounts;
            set { SetProperty(ref _accounts, value); RaisePropertyChanged(nameof(NetWorth)); }
        }

        Account _selectedAccount;
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set => SetProperty(ref _selectedAccount, value);
        }

        public decimal NetWorth { get => Accounts.Sum(a => a.Balance ?? 0); }

        bool _noAccounts;
        public bool NoAccounts
        {
            get => _noAccounts;
            set => SetProperty(ref _noAccounts, value);
        }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public AccountsPageViewModel(INavigationService navigationService,
		                             IPageDialogService dialogService,
		                             IAccountLogic accountLogic,
		                             ISync syncService)
        {
            _accountLogic = accountLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
			_syncService = syncService;

            Accounts = new List<Account>();
            SelectedAccount = null;

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<Account>(async a => await ExecuteEditCommand(a));
            DeleteCommand = new DelegateCommand<Account>(async a => await ExecuteDeleteCommand(a));
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
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

            await _navigationService.NavigateAsync(PageName.AccountInfoPage, parameters);

            SelectedAccount = null;
        }

        public async Task ExecuteRefreshCommand()
        {
            if (!IsBusy)
            {
                IsBusy = true;
            }

            try
            {
                var result = await _accountLogic.GetAccountsAsync();

                if (result.Success)
                {
                    Accounts = result.Data;
                }
                else
                {
                    await Task.Yield();
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "Okay");
                }

                NoAccounts = (Accounts?.Count ?? 0) == 0;
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
				var syncResult = await _syncService.FullSync();

                if (!syncResult.Success)
                {
                    await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                }

                await ExecuteRefreshCommand();
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
            }
        }

        public async Task ExecuteAddTransactionCommand()
        {
            await _navigationService.NavigateAsync(PageName.TransactionEditPage);
        }
    }
}
