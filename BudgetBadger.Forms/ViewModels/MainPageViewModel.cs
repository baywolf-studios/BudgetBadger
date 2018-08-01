using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;

namespace BudgetBadger.Forms.ViewModels
{
    public class MainPageViewModel
    {
        readonly INavigationService _navigationService;

        public ICommand NavigateCommand { get; set; }

        public MainPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            NavigateCommand = new DelegateCommand<string>(async a => await ExecuteNavigateCommand(a));
        }

        public async Task ExecuteNavigateCommand(string pageName)
        {
            if (!string.IsNullOrEmpty(pageName))
            {
                await _navigationService.NavigateAsync("/MainPage/NavigationPage/" + pageName);
            }
        }
    }
}
