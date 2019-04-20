using BudgetBadger.Forms.Pages;
using BudgetBadger.Forms.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.Transactions
{
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class SplitTransactionDetailedPage : BasePage
    {
		public SplitTransactionDetailedPage ()
		{
			InitializeComponent ();
		}
	}
}