using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Events;
using BudgetBadger.Models;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountsPageViewModel : BaseViewModel, INavigatedAware
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<IAccountLogic> _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand RefreshAccountCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ReconcileCommand { get; set; }
        public Predicate<object> Filter { get => (ac) => _accountLogic.Value.FilterAccount((Account)ac, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        ObservableList<Account> _accounts;
        public ObservableList<Account> Accounts
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

        public decimal NetWorth { get => Accounts.Sum(a => a.Balance ?? 0); }
        public decimal Assests { get => Accounts.Where(a => (a.Balance ?? 0) > 0).Sum(a => a.Balance ?? 0); }
        public decimal Debts { get => Accounts.Where(a => (a.Balance ?? 0) < 0).Sum(a => a.Balance ?? 0); }

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

        bool _fullRefresh = true;

        public AccountsPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                     INavigationService navigationService,
		                             IPageDialogService dialogService,
                                     Lazy<IAccountLogic> accountLogic,
                                     IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _accountLogic = accountLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;

            Accounts = new ObservableList<Account>();
            SelectedAccount = null;

            SelectedCommand = new Command<Account>(async a => await ExecuteSelectedCommand(a));
            RefreshCommand = new Command(async () => await FullRefresh());
            AddCommand = new Command(async () => await ExecuteAddCommand());
            EditCommand = new Command<Account>(async a => await ExecuteEditCommand(a));
            AddTransactionCommand = new Command(async () => await ExecuteAddTransactionCommand());
            SaveCommand = new Command<Account>(async a => await ExecuteSaveCommand(a));
            ReconcileCommand = new Command<Account>(async a => await ExecuteReconcileCommand(a));
            RefreshAccountCommand = new Command<Account>(RefreshAccount);

            _eventAggregator.GetEvent<AccountSavedEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<AccountDeletedEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<AccountHiddenEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<AccountUnhiddenEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshAccountFromTransaction(t));
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(async t => await RefreshAccountFromTransaction(t));
        }

        public override async void OnActivated()
        {
            if (_fullRefresh)
            {
                await FullRefresh();
                _fullRefresh = false;
            }
        }

        public override void OnDeactivated()
        {
            SelectedAccount = null;
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        // this gets hit before the OnActivated
        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (!_fullRefresh)
                RefreshSummary();
        }

        public async Task ExecuteSelectedCommand(Account account)
        {
            if (account == null)
            {
                return;
            }

            if (account.IsGenericHiddenAccount)
            {
                await _navigationService.NavigateAsync(PageName.HiddenAccountsPage);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Account, account }
                };

                await _navigationService.NavigateAsync(PageName.AccountInfoPage, parameters);
            }
        }

        public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.AccountEditPage);
            SelectedAccount = null;
        }

        public async Task ExecuteEditCommand(Account account)
        {
            if (!account.IsGenericHiddenAccount)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Account, account }
                };
                await _navigationService.NavigateAsync(PageName.AccountEditPage, parameters);
            }
        }

        public async Task ExecuteAddTransactionCommand()
        {
            await _navigationService.NavigateAsync(PageName.TransactionEditPage);
        }

        public async Task ExecuteSaveCommand(Account account)
        {
            var result = await _accountLogic.Value.SaveAccountAsync(account);

            if (result.Success)
            {
                _eventAggregator.GetEvent<AccountSavedEvent>().Publish(result.Data);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteReconcileCommand(Account account)
        {
            if (!account.IsGenericHiddenAccount)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Account, account }
                };

                await _navigationService.NavigateAsync(PageName.AccountReconcilePage, parameters);
            }
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
                var result = await _accountLogic.Value.GetAccountsAsync();

                if (result.Success)
                {
                    Accounts.ReplaceRange(result.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }
                
                RefreshSummary();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void RefreshSummary()
        {
            NoAccounts = (Accounts?.Count ?? 0) == 0;

            RaisePropertyChanged(nameof(NetWorth));
            RaisePropertyChanged(nameof(Assests));
            RaisePropertyChanged(nameof(Debts));
        }

        public void RefreshAccount(Account account)
        {
            var accounts = Accounts.Where(a => a.Id != account.Id).ToList();

            if (account != null && _accountLogic.Value.FilterAccount(account, FilterType.Standard))
            {
                accounts.Add(account);
            }

            Accounts.ReplaceRange(accounts);

            RefreshSummary();
        }

        public async Task RefreshAccountFromTransaction(Transaction transaction)
        {
            if (transaction != null && transaction.Account != null)
            {
                var updatedAccountResult = await _accountLogic.Value.GetAccountAsync(transaction.Account.Id);
                if (updatedAccountResult.Success)
                {
                    RefreshAccount(updatedAccountResult.Data);
                }
            }
        }
    }
}
