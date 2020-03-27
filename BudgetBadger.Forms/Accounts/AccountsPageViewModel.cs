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
using Prism;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Purchase;
using BudgetBadger.Forms.Events;
using Prism.Events;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountsPageViewModel : BaseViewModel, INavigatedAware
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<IAccountLogic> _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
		readonly Lazy<ISyncFactory> _syncFactory;
        readonly Lazy<IPurchaseService> _purchaseService;
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

        bool _needToSync;

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

        bool _hasPro;
        public bool HasPro
        {
            get => _hasPro;
            set => SetProperty(ref _hasPro, value);
        }

        bool _hardRefresh = true;

        public AccountsPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                     INavigationService navigationService,
		                             IPageDialogService dialogService,
                                     Lazy<IAccountLogic> accountLogic,
                                     Lazy<ISyncFactory> syncFactory,
                                     Lazy<IPurchaseService> purchaseService,
                                     IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _accountLogic = accountLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _purchaseService = purchaseService;
            _eventAggregator = eventAggregator;

            Accounts = new ObservableList<Account>();
            SelectedAccount = null;

            SelectedCommand = new DelegateCommand<Account>(async a => await ExecuteSelectedCommand(a));
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<Account>(async a => await ExecuteEditCommand(a));
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
            SaveCommand = new DelegateCommand<Account>(async a => await ExecuteSaveCommand(a));
            ReconcileCommand = new DelegateCommand<Account>(async a => await ExecuteReconcileCommand(a));
            RefreshAccountCommand = new DelegateCommand<Account>(ExecuteRefreshAccountCommand);

            _eventAggregator.GetEvent<AccountSavedEvent>().Subscribe(ExecuteRefreshAccountCommand);
            _eventAggregator.GetEvent<AccountDeletedEvent>().Subscribe(ExecuteRefreshAccountCommand);
            _eventAggregator.GetEvent<AccountHiddenEvent>().Subscribe(ExecuteRefreshAccountCommand);
            _eventAggregator.GetEvent<AccountUnhiddenEvent>().Subscribe(ExecuteRefreshAccountCommand);
            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshAccountFromTransaction(t));
        }

        public override async void OnActivated()
        {
            var purchasedPro = await _purchaseService.Value.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;

            if (_hardRefresh)
            {
                await ExecuteRefreshCommand();
                _hardRefresh = false;
            }
        }

        public override async void OnDeactivated()
        {
            SelectedAccount = null;

            if (_needToSync)
            {
                var syncService = _syncFactory.Value.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.Value.SetLastSyncDateTime(DateTime.Now);
                    _needToSync = false;
                }
            }
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        // this gets hit before the OnActivated
        public void OnNavigatedTo(INavigationParameters parameters)
        {
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
                _hardRefresh = true;
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

        public async Task ExecuteRefreshCommand()
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
                NoAccounts = (Accounts?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExecuteRefreshAccountCommand(Account account)
        {
            var accounts = Accounts.Where(a => a.Id != account.Id).ToList();

            if (account != null && account.IsActive)
            {
                accounts.Add(account);
            }

            Accounts.ReplaceRange(accounts);
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
                _needToSync = true;
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

        void RefreshSummary()
        {
            RaisePropertyChanged(nameof(NetWorth));
            RaisePropertyChanged(nameof(Assests));
            RaisePropertyChanged(nameof(Debts));
        }

        async Task RefreshAccountFromTransaction(Transaction transaction)
        {
            if (transaction != null && transaction.Account != null)
            {
                var updatedAccountResult = await _accountLogic.Value.GetAccountAsync(transaction.Account.Id);
                if (updatedAccountResult.Success)
                {
                    ExecuteRefreshAccountCommand(updatedAccountResult.Data);
                }
            }
        }
    }
}
