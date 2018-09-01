using BudgetBadger.Forms.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.Accounts
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AccountsDetailedPage : RootSearchPage
	{
		public AccountsDetailedPage ()
		{
			InitializeComponent ();
            PropertyChanged += AccountsDetailedPage_PropertyChanged;
		}

        private void AccountsDetailedPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchText))
            {
            }
        }
    }
}