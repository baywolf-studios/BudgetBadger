using Syncfusion.SfPullToRefresh.XForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.UserControls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PullToRefresh : SfPullToRefresh
    {
        public static BindableProperty RefreshCommandProperty = BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(PullToRefresh));
        public ICommand RefreshCommand
        {
            get => (ICommand)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }

        public PullToRefresh ()
		{
			InitializeComponent ();
            Refreshing += PullToRefresh_Refreshing;
		}

        private void PullToRefresh_Refreshing(object sender, EventArgs e)
        {
            RefreshCommand.Execute(null);
        }
    }
}