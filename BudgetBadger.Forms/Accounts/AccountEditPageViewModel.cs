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
using BudgetBadger.Core.LocalizedResources;

namespace BudgetBadger.Forms.Accounts
{
    public class AccountEditPageViewModel : BindableBase, INavigationAware, IInitializeAsync
    {
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISyncFactory _syncFactory;
        readonly IResourceContainer _resourceContainer;

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

        bool _noAccounts;
        public bool NoAccounts
        {
            get => _noAccounts;
            set => SetProperty(ref _noAccounts, value);
        }

        public IList<string> AccountTypes
        {
            get => Enum.GetNames(typeof(AccountType)).Select(b => _resourceContainer.GetResourceString(b)).ToList();
        }

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SaveCommand { get; set; }
        public ICommand HideCommand { get; set; }
        public ICommand UnhideCommand { get; set; }
        public ICommand SoftDeleteCommand { get; set; }

        public AccountEditPageViewModel(INavigationService navigationService,
                                        IPageDialogService dialogService,
                                        IAccountLogic accountLogic,
                                        ISyncFactory syncFactory,
                                        IResourceContainer resourceContainer)
        {
            _navigationService = navigationService;
            _accountLogic = accountLogic;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _resourceContainer = resourceContainer;

            Account = new Account();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            HideCommand = new DelegateCommand(async () => await ExecuteHideCommand());
            UnhideCommand = new DelegateCommand(async () => await ExecuteUnhideCommand());
            SoftDeleteCommand = new DelegateCommand(async () => await ExecuteSoftDeleteCommand());
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
                    _needToSync = false;
                }
            }
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(PageParameter.Account);
            if (account != null)
            {
                Account = account.DeepCopy();
            }

            var accountCountResult = await _accountLogic.GetAccountsCountAsync();
            if (accountCountResult.Success)
            {
                NoAccounts = accountCountResult.Data == 0;
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
                BusyText = _resourceContainer.GetResourceString("BusyTextSaving");
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
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSoftDeleteCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextDeleting");

                var result = await _accountLogic.SoftDeleteAccountAsync(Account.Id);
                if (result.Success)
                {
                    _needToSync = true;

                    if (Device.RuntimePlatform == Device.macOS)
                    {
                        await _navigationService.GoBackAsync();
                    }
                    else
                    {
                        await _navigationService.GoBackToRootAsync();
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));

                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteHideCommand()
        {
			if (IsBusy)
            {
                return;
            }

            IsBusy = true;

			try
			{
				BusyText = _resourceContainer.GetResourceString("BusyTextHiding");

                var result = await _accountLogic.HideAccountAsync(Account.Id);
				if (result.Success)
				{
                    _needToSync = true;

                    if (Device.RuntimePlatform == Device.macOS)
                    {
                        await _navigationService.GoBackAsync();
                    }
                    else
                    {
                        await _navigationService.GoBackToRootAsync();
                    }
                }
				else
				{
					await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertHideUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));

                }
			}
			finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteUnhideCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextUnhiding");
                var result = await _accountLogic.UnhideAccountAsync(Account.Id);
                if (result.Success)
                {
                    _needToSync = true;

                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertHideUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
