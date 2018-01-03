using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Navigation;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using PropertyChanged;

namespace BudgetBadger.Forms.Accounts
{
    [AddINotifyPropertyChangedInterface]
    public class AccountsPageViewModel : INavigationAware
    {
        readonly IAccountLogic AccountLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand SearchCommand { get; set; }

        public bool IsBusy { get; set; }

        public ObservableCollection<Account> Accounts { get; set; }
        public Account SelectedAccount { get; set; }
        public ObservableCollection<GroupedList<Account>> GroupedAccounts { get; set; }

        public bool SelectorMode { get; set; }
        public bool NormalMode { get { return !SelectorMode; } }

        public string SearchText { get; set; }
        public void OnSearchTextChanged()
        {
            ExecuteSearchCommand();
        }

        public AccountsPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IAccountLogic accountLogic)
        {
            AccountLogic = accountLogic;
            NavigationService = navigationService;
            DialogService = dialogService;

            Accounts = new ObservableCollection<Account>();
            SelectedAccount = null;
            GroupedAccounts = new ObservableCollection<GroupedList<Account>>();

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            NewCommand = new DelegateCommand(async () => await ExecuteNewCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            // returns default bool if none present
            SelectorMode = parameters.GetValue<bool>(NavigationParameterType.SelectorMode);

            await ExecuteRefreshCommand();
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public async Task ExecuteSelectedCommand()
        {
            if (SelectedAccount == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { NavigationParameterType.Account, SelectedAccount }
            };

            if (SelectorMode)
            {
                await NavigationService.GoBackAsync(parameters);
            }
            else
            {
                await NavigationService.NavigateAsync(NavigationPageName.AccountInfoPage, parameters);
            }


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
                var result = await AccountLogic.GetAccountsAsync();
                if (result.Success)
                {
                    Accounts = new ObservableCollection<Account>(result.Data);
                    GroupedAccounts = new ObservableCollection<GroupedList<Account>>(AccountLogic.GroupAccounts(Accounts));
                }
                else
                {
                    await Task.Yield();
                    await DialogService.DisplayAlertAsync("Error", result.Message, "Okay");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteNewCommand()
        {
            await NavigationService.NavigateAsync(NavigationPageName.AccountEditPage);
            SelectedAccount = null;
        }

        public void ExecuteSearchCommand()
        {
            GroupedAccounts = new ObservableCollection<GroupedList<Account>>(AccountLogic.GroupAccounts(AccountLogic.SearchAccounts(Accounts, SearchText)));
        }
    }
}
