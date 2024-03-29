﻿using System;
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
    public class HiddenAccountsPageViewModel : ObservableBase, INavigationAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public Predicate<object> Filter { get => (ac) => _accountLogic.FilterAccount((Account)ac, SearchText); }

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

        public HiddenAccountsPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService, 
            IPageDialogService dialogService, 
            IAccountLogic accountLogic,
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

            _eventAggregator.GetEvent<AccountSavedEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<AccountDeletedEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<AccountHiddenEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<AccountUnhiddenEvent>().Subscribe(RefreshAccount);
            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshAccountFromTransaction(t));
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(async t => await RefreshAccountFromTransaction(t));
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedAccount = null;
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            await FullRefresh();
        }

        public async Task ExecuteSelectedCommand(Account account)
        {
            if (account == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Account, account }
            };

            await _navigationService.NavigateAsync(PageName.AccountEditPage, parameters);

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
                var result = await _accountLogic.GetHiddenAccountsAsync();

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

        public void RefreshAccount(Account account)
        {
            var accounts = Accounts.Where(a => a.Id != account.Id).ToList();

            if (account != null && _accountLogic.FilterAccount(account, FilterType.Hidden))
            {
                accounts.Add(account);
            }

            Accounts.ReplaceRange(accounts);
        }

        public async Task RefreshAccountFromTransaction(Transaction transaction)
        {
            if (transaction != null && transaction.Account != null)
            {
                var updatedAccountResult = await _accountLogic.GetAccountAsync(transaction.Account.Id);
                if (updatedAccountResult.Success)
                {
                    RefreshAccount(updatedAccountResult.Data);
                }
            }
        }
    }
}
