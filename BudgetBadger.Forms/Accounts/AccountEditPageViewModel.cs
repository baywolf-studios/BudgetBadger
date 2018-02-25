using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;
using Prism.Mvvm;
using System.Collections.Generic;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountEditPageViewModel : BindableBase, INavigationAware
    {
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISync _syncService;

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        string _busyText;
        public string BusyText
        {
            get => _busyText;
            set => SetProperty(ref _busyText, value);
        }

        Account _account;
        public Account Account
        {
            get => _account;
            set => SetProperty(ref _account, value);
        }

        IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes
        {
            get => _accountTypes;
            set => SetProperty(ref _accountTypes, value);
        }

        public ICommand SaveCommand { get; set; }
        public ICommand DeleteAccountCommand { get; set; }


        public AccountEditPageViewModel(INavigationService navigationService,
                                        IPageDialogService dialogService,
                                        IAccountLogic accountLogic,
                                       ISync syncService)
        {
            _navigationService = navigationService;
            _accountLogic = accountLogic;
            _dialogService = dialogService;
            _syncService = syncService;

            var accountTypes = _accountLogic.GetAccountTypesAsync().Result;
            if (accountTypes.Success)
            {
                AccountTypes = accountTypes.Data;
            }
            else
            {
                AccountTypes = new List<AccountType>();
            }

            Account = new Account();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
                Account.Type = AccountTypes.FirstOrDefault(t => t.Id == Account.Type.Id);
            }
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public async Task ExecuteSaveCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = "Saving";
                var result = await _accountLogic.SaveAccountAsync(Account);

                if (result.Success)
                {
                    
                    BusyText = "Syncing";
                    var syncTask = _syncService.FullSync();
                    await _navigationService.GoBackAsync();

                    var syncResult = await syncTask;
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }


                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
