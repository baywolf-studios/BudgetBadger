using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace BudgetBadger.Forms.Views
{
    public partial class MainDesktopPage : MasterDetailPage
    {
        void Handle_Tapped(object sender, EventArgs e)
        {
            EnvelopesFrame.BackgroundColor = Color.White;
            AccountsFrame.BackgroundColor = Color.White;
            PayeesFrame.BackgroundColor = Color.White;
            ReportsFrame.BackgroundColor = Color.White;
            SettingsFrame.BackgroundColor = Color.White;

            var frame = (Frame)sender;
            frame.BackgroundColor = (Color)Application.Current.Resources["SelectedNavigationItemColor"];
        }

        public MainDesktopPage()
        {
            InitializeComponent();
            EnvelopesFrame.BackgroundColor = (Color)Application.Current.Resources["SelectedNavigationItemColor"];
        }
    }
}
