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
    public class AccountEditPageViewModel : BindableBase, INavigatingAware
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

        string _selectedAccountType;
        public string SelectedAccountType
        {
            get => _selectedAccountType;
            set { SetProperty(ref _selectedAccountType, value); Account.OnBudget = (value == "Budget"); }
        }

        IEnumerable<string> _accountTypes;
        public IEnumerable<string> AccountTypes
        {
            get => _accountTypes;
            set => SetProperty(ref _accountTypes, value);
        }

        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand UndoDeleteCommand { get; set; }

        public AccountEditPageViewModel(INavigationService navigationService,
                                        IPageDialogService dialogService,
                                        IAccountLogic accountLogic,
                                       ISync syncService)
        {
            _navigationService = navigationService;
            _accountLogic = accountLogic;
            _dialogService = dialogService;
            _syncService = syncService;

            AccountTypes = _accountLogic.GetAccountTypes();

            Account = new Account();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            DeleteCommand = new DelegateCommand(async () => await ExecuteDeleteCommand());
            UndoDeleteCommand = new DelegateCommand(async () => await ExecuteUndoDeleteCommand());
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
                SelectedAccountType = AccountTypes.FirstOrDefault(t => t == Account.Type);
            }
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
                    var syncTask = await _syncService.FullSync();

					var parameters = new NavigationParameters
                    {
                        { PageParameter.Account, result.Data }
                    };
                    await _navigationService.GoBackAsync(parameters);

                    var syncResult = syncTask;
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

        public async Task ExecuteDeleteCommand()
        {
			if (IsBusy)
            {
                return;
            }

            IsBusy = true;

			try
			{
				BusyText = "Deleting";
				var result = await _accountLogic.DeleteAccountAsync(Account.Id);
				if (result.Success)
				{
					BusyText = "Syncing";
                    var syncTask = _syncService.FullSync();

                    await _navigationService.GoBackToRootAsync();

                    var syncResult = await syncTask;
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }
				}
				else
				{
					await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
				}
			}
			finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteUndoDeleteCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = "Undoing Delete";
                var result = await _accountLogic.UndoDeleteAccountAsync(Account.Id);
                if (result.Success)
                {
                    BusyText = "Syncing";
                    var syncTask = _syncService.FullSync();

                    await _navigationService.GoBackToRootAsync();

                    var syncResult = await syncTask;
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
