﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountSelectionPageViewModel : BindableBase, INavigatingAware
    {
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SearchCommand { get; set; }

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
            set => SetProperty(ref _accounts, value);
        }

        Account _selectedAccount;
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set => SetProperty(ref _selectedAccount, value);
        }

        IReadOnlyList<IGrouping<string, Account>> _groupedAccounts;
        public IReadOnlyList<IGrouping<string, Account>> GroupedAccounts
        {
            get => _groupedAccounts;
            set => SetProperty(ref _groupedAccounts, value);
        }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public AccountSelectionPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IAccountLogic accountLogic)
        {
            _accountLogic = accountLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Accounts = new List<Account>();
            SelectedAccount = null;
            GroupedAccounts = Accounts.GroupBy(a => "").ToList();

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
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

            await _navigationService.GoBackAsync(parameters);

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
                var result = await _accountLogic.GetAccountsForSelectionAsync();

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

        public void ExecuteSearchCommand()
        {
            GroupedAccounts = _accountLogic.GroupAccounts(_accountLogic.SearchAccounts(Accounts, SearchText));
        }
    }
}
