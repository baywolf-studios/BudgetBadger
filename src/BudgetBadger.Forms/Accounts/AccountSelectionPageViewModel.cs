using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Core.Models;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;
using BudgetBadger.Forms.Extensions;
using BudgetBadger.Logic;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountSelectionPageViewModel : ObservableBase, INavigationAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
		public ICommand AddCommand { get; set; }
        public Predicate<object> Filter { get => (ac) => _accountLogic.FilterAccount((AccountModel)ac, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        ObservableList<AccountModel> _accounts;
        public ObservableList<AccountModel> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        AccountModel _selectedAccount;
        public AccountModel SelectedAccount
        {
            get => _selectedAccount;
            set => SetProperty(ref _selectedAccount, value);
        }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        bool _noAccounts;
        public bool NoAccounts
        {
            get => _noAccounts;
            set => SetProperty(ref _noAccounts, value);
        }

        public AccountSelectionPageViewModel(
            IResourceContainer resourceContainer,
            INavigationService navigationService,
            IPageDialogService dialogService,
            IAccountLogic accountLogic)
        {
            _resourceContainer = resourceContainer;
            _accountLogic = accountLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Accounts = new ObservableList<AccountModel>();
            SelectedAccount = null;

            SelectedCommand = new Command<AccountModel>(async a => await ExecuteSelectedCommand(a));
            RefreshCommand = new Command(async () => await FullRefresh());
            AddCommand = new Command(async () => await ExecuteAddCommand());
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedAccount = null;
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            var account = parameters.GetValue<AccountModel>(PageParameter.Account);
            if (account != null)
            {
                await _navigationService.GoBackAsync(parameters);
                return;
            }

            await FullRefresh();
        }

        public async Task ExecuteSelectedCommand(AccountModel account)
        {
            if (account == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Account, account }
            };

            await _navigationService.GoBackAsync(parameters);
        }

        public async Task FullRefresh()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var result = await _accountLogic.GetAccountsForSelectionAsync();

                if (result.Success)
                {
                    Accounts.ReplaceRange(result.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
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
        }
    }
}
