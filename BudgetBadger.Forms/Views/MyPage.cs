using System;

using Xamarin.Forms;
using Prism.Navigation;

namespace BudgetBadger.Forms.Views
{
    public class MyPage : NavigationPage, INavigationPageOptions
    {
        public bool ClearNavigationStackOnNavigation => false;
    }
}

