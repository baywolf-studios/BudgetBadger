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
using PropertyChanged;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;

namespace BudgetBadger.Forms.Accounts
{
    [AddINotifyPropertyChangedInterface]
    public class AccountEditPageViewModel : INavigationAware
    {
        readonly IAccountLogic AccountLogic;
        readonly INavigationService NavigationService;
        readonly IPageDialogService DialogService;
        readonly ISync SyncService;

        public Account Account { get; set; }
        public ObservableCollection<AccountType> AccountTypes { get; set; }

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

            var typesResult = AccountLogic.GetAccountTypesAsync().Result;
            AccountTypes = new ObservableCollection<AccountType>(typesResult.Data);
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

            //Task.Run(() => SyncService.Sync());
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public async Task ExecuteSaveCommand()
        {
            var result = await AccountLogic.SaveAccountAsync(Account);

            if (result.Success)
            {
                await SyncService.FullSync();
                await NavigationService.GoBackAsync();
            }
            else
            {
                await DialogService.DisplayAlertAsync("Error", result.Message, "Okay");
            }
        }
    }
}
