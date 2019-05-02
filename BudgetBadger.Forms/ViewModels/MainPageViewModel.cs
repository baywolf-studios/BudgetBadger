using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using Prism.Commands;
using Prism.Ioc;
using Prism.Navigation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.ViewModels
{
    public class MainPageViewModel : INavigatingAware
    {
        readonly INavigationService _navigationService;
        readonly ITransactionLogic _transactionLogic;

        public ICommand NavigateCommand { get; set; }

        public MainPageViewModel(INavigationService navigationService,
            ITransactionLogic transactionLogic)
        {
            _navigationService = navigationService;
            _transactionLogic = transactionLogic;

            NavigateCommand = new DelegateCommand<string>(async a => await ExecuteNavigateCommand(a));
        }

        public async Task ExecuteNavigateCommand(string pageName)
        {
            if (!string.IsNullOrEmpty(pageName))
            {
                await _navigationService.NavigateAsync(pageName);
            }
        }

        public async void OnNavigatingTo(INavigationParameters parameters)
        {
            var transactionCountResult = await _transactionLogic.GetTransactionsCountAsync();
            if (transactionCountResult.Success && transactionCountResult.Data >= 20)
            {
                // ask for review
            }
        }
    }
}
