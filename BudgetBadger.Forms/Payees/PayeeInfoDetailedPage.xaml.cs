using BudgetBadger.Forms.Pages;
using BudgetBadger.Forms.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.Payees
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class PayeeInfoDetailedPage : BaseDetailedPage
    {
		public PayeeInfoDetailedPage()
		{
			InitializeComponent ();
		}
	}
}