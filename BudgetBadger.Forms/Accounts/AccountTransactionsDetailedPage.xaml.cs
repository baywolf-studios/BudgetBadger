﻿using BudgetBadger.Forms.UserControls;
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
	public partial class AccountTransactionsDetailedPage : ChildSearchPage
	{
		public AccountTransactionsDetailedPage ()
		{
			InitializeComponent ();
		}
	}
}