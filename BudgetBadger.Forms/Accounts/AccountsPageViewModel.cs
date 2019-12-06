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

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
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

        bool _hasPro;
        public bool HasPro
        {
            get => _hasPro;
            set => SetProperty(ref _hasPro, value);
        }

        public AccountsPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                     INavigationService navigationService,
		                             IPageDialogService dialogService,
                                     Lazy<IAccountLogic> accountLogic,
                                     Lazy<ISyncFactory> syncFactory,
                                     Lazy<IPurchaseService> purchaseService)
        {
            _resourceContainer = resourceContainer;
            _accountLogic = accountLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _purchaseService = purchaseService;

            Accounts = new List<Account>();
            SelectedAccount = null;

            SelectedCommand = new DelegateCommand<Account>(async a => await ExecuteSelectedCommand(a));
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<Account>(async a => await ExecuteEditCommand(a));
            AddTransactionCommand = new DelegateCommand(async () => await ExecuteAddTransactionCommand());
            SaveCommand = new DelegateCommand<Account>(async a => await ExecuteSaveCommand(a));
            ReconcileCommand = new DelegateCommand<Account>(async a => await ExecuteReconcileCommand(a));
        }

        public override async void OnActivated()
        {
            var purchasedPro = await _purchaseService.Value.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;

            await ExecuteRefreshCommand();
        }

        public override async void OnDeactivated()
        {
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
        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                if (!Accounts.Any(a => a.Balance < 0) && account.Balance < 0)
                {
                    // show message about debt envelopes
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertDebtEnvelopes"),
                            _resourceContainer.Value.GetResourceString("AlertMessageDebtEnvelopes"),
                            _resourceContainer.Value.GetResourceString("AlertOk"));
                }
            }
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
                    Accounts = result.Data;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
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
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteReconcileCommand(Account account)
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Account, account }
            };

            await _navigationService.NavigateAsync(PageName.AccountReconcilePage, parameters);
        }
    }
}
