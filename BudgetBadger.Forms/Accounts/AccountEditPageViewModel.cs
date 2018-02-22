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
        readonly IAccountLogic AccountLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;
        readonly ISync SyncService;

        bool _isBusy;
        public bool IsBusy
        { 
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); } 
        }

        string _busyText;
        public string BusyText
        {
            get { return _busyText; }
            set { SetProperty(ref _busyText, value); }
        }

        Account _account;
        public Account Account
        {
            get { return _account; }
            set { SetProperty(ref _account, value); }
        }

        IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes
        {
            get { return _accountTypes; }
            set { SetProperty(ref _accountTypes, value); }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand DeleteAccountCommand { get; set; }


        public AccountEditPageViewModel(INavigationService navigationService,
                                        IPageDialogService dialogService,
                                        IAccountLogic accountLogic,
                                       ISync syncService)
        {
            NavigationService = navigationService;
            AccountLogic = accountLogic;
            DialogService = dialogService;
            SyncService = syncService;

            AccountTypes = new List<AccountType>();
            Account = new Account();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            // fix this later, null error
            var accountTypes =  await AccountLogic.GetAccountTypesAsync();
            if (accountTypes.Success)
            {
                AccountTypes = accountTypes.Data;
            }

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
            IsBusy = true;

            try
            {
                BusyText = "Saving";
                var result = await AccountLogic.SaveAccountAsync(Account);

                if (result.Success)
                {
                    BusyText = "Syncing";
                    var syncResult = await SyncService.FullSync();
                    if (!syncResult.Success)
                    {
                        await DialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }
                    await NavigationService.GoBackAsync();
                }
                else
                {
                    await DialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
