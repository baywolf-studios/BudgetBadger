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
using BudgetBadger.Models.Extensions;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountEditPageViewModel : BindableBase, INavigationAware
    {
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISyncFactory _syncFactory;

        bool _needToSync;

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

        public IList<AccountType> AccountTypes
        {
            get => Enumeration.GetAll<AccountType>().ToList();
        }

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand UndoDeleteCommand { get; set; }

        public AccountEditPageViewModel(INavigationService navigationService,
                                        IPageDialogService dialogService,
                                        IAccountLogic accountLogic,
                                        ISyncFactory syncFactory)
        {
            _navigationService = navigationService;
            _accountLogic = accountLogic;
            _dialogService = dialogService;
            _syncFactory = syncFactory;

            Account = new Account();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            DeleteCommand = new DelegateCommand(async () => await ExecuteDeleteCommand());
            UndoDeleteCommand = new DelegateCommand(async () => await ExecuteUndoDeleteCommand());
        }

        public async void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (_needToSync)
            {
                var syncService = _syncFactory.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.SetLastSyncDateTime(DateTime.Now);
                }
            }
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public void OnNavigatingTo(INavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
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

                    _needToSync = true;

					var parameters = new NavigationParameters
                    {
                        { PageParameter.Account, result.Data }
                    };
                    await _navigationService.GoBackAsync(parameters);
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
                    _needToSync = true;

                    var parameter = new NavigationParameters
                    {
                        { PageParameter.GoBackToRoot, true }
                    };
                    await _navigationService.GoBackAsync(parameter);
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
                    _needToSync = true;

                    await _navigationService.GoBackAsync();
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
