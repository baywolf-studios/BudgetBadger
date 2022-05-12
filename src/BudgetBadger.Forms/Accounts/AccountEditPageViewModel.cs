using System;
using System.Collections.Generic;
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
    public class AccountEditPageViewModel : ObservableBase, INavigationAware, IInitializeAsync
    {
        readonly IAccountLogic _accountLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IResourceContainer _resourceContainer;
        readonly IEventAggregator _eventAggregator;

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
            get => Enum.GetNames(typeof(AccountType)).Select(_resourceContainer.GetResourceString).ToList();
        }

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand SaveCommand { get; set; }
        public ICommand HideCommand { get; set; }
        public ICommand UnhideCommand { get; set; }
        public ICommand SoftDeleteCommand { get; set; }

        public AccountEditPageViewModel(INavigationService navigationService,
                                        IPageDialogService dialogService,
                                        IAccountLogic accountLogic,
                                        IResourceContainer resourceContainer,
                                        IEventAggregator eventAggregator)
        {
            _navigationService = navigationService;
            _accountLogic = accountLogic;
            _dialogService = dialogService;
            _resourceContainer = resourceContainer;
            _eventAggregator = eventAggregator;

            Account = new Account();

            SaveCommand = new Command(async () => await ExecuteSaveCommand());
            HideCommand = new Command(async () => await ExecuteHideCommand());
            UnhideCommand = new Command(async () => await ExecuteUnhideCommand());
            SoftDeleteCommand = new Command(async () => await ExecuteSoftDeleteCommand());
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if(parameters.GetNavigationMode() == NavigationMode.Back)
            {
                await InitializeAsync(parameters);
            }
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
                    _eventAggregator.GetEvent<AccountSavedEvent>().Publish(result.Data);

                    await _navigationService.GoBackAsync();
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

            var confirm = await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertConfirmation"),
                _resourceContainer.GetResourceString("AlertConfirmDelete"),
                _resourceContainer.GetResourceString("AlertOk"),
                _resourceContainer.GetResourceString("AlertCancel"));

            if (confirm)
            {
                IsBusy = true;

                try
                {
                    BusyText = _resourceContainer.GetResourceString("BusyTextDeleting");

                    var result = await _accountLogic.SoftDeleteAccountAsync(Account.Id);
                    if (result.Success)
                    {
                        _eventAggregator.GetEvent<AccountDeletedEvent>().Publish(result.Data);

                        await _navigationService.GoBackAsync();
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
                    _eventAggregator.GetEvent<AccountHiddenEvent>().Publish(result.Data);

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
                    _eventAggregator.GetEvent<AccountUnhiddenEvent>().Publish(result.Data);

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
