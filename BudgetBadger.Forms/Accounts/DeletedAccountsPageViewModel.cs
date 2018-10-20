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
	public class DeletedAccountsPageViewModel : BindableBase, INavigatingAware
    {
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
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

        public DeletedAccountsPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IAccountLogic accountLogic)
        {
            _accountLogic = accountLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Accounts = new List<Account>();
            SelectedAccount = null;

            SelectedCommand = new DelegateCommand<Account>(async a => await ExecuteSelectedCommand(a));
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
        }

        public async void OnNavigatingTo(INavigationParameters parameters)
        {
			var account = parameters.GetValue<Account>(PageParameter.Account);
			if (account != null)
			{
				await _navigationService.GoBackAsync(parameters);
			}
			else
			{
				await ExecuteRefreshCommand();
			}
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

        public async Task ExecuteRefreshCommand()
        {
            if (!IsBusy)
            {
                IsBusy = true;
            }

            try
            {
                var result = await _accountLogic.GetDeletedAccountsAsync();

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
    }
}
