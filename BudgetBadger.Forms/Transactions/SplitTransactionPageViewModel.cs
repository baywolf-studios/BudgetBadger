using System;
using BudgetBadger.Core.Logic;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Transactions
{
    public class SplitTransactionPageViewModel : BindableBase
    {
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ITransactionLogic _transLogic;
    
        public SplitTransactionPageViewModel(INavigationService navigationService,
                                             IPageDialogService dialogService,
                                             ITransactionLogic transLogic)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _transLogic = transLogic;
        }
    }
}
