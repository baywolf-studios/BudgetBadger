using System;
using System.Collections.Generic;
using BudgetBadger.Forms.Pages;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.Transactions
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SplitTransactionDetailedPage : BaseDetailedPage
    {
        public SplitTransactionDetailedPage()
        {
            InitializeComponent();
        }
    }
}
