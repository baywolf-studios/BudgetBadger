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
    public class AccountEditPageViewModel : INavigationAware
    {
        readonly IAccountLogic AccountLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;

        public Account Account { get; set; }
        public ObservableCollection<AccountType> AccountTypes { get; set; }

        public bool NewMode { get => Account.CreatedDateTime == null; }
        public bool EditMode { get => !NewMode; }

        public ICommand SaveCommand { get; set; }
        public ICommand DeleteAccountCommand { get; set; }


        public AccountEditPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IAccountLogic accountLogic)
        {
            NavigationService = navigationService;
            AccountLogic = accountLogic;
            DialogService = dialogService;

            var typesResult = AccountLogic.GetAccountTypesAsync().Result;
            AccountTypes = new ObservableCollection<AccountType>(typesResult.Data);
            Account = new Account();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand(), CanExecuteSaveCommand);
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            var account = parameters.GetValue<Account>(NavigationParameterType.Account);
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
            var result = await AccountLogic.UpsertAccountAsync(Account);

            if (result.Success)
            {
                await NavigationService.GoBackAsync();
            }
            else
            {
                await DialogService.DisplayAlertAsync("Error", result.Message, "Okay");
            }
        }

        public bool CanExecuteSaveCommand()
        {
            return true;
        }
    }
}
