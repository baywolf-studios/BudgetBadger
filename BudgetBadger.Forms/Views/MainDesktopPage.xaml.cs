using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Settings;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Envelopes;
using BudgetBadger.Forms.UserControls;
using FFImageLoading.Svg.Forms;
using Prism.Ioc;
using Prism.Navigation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Views
{
    public partial class MainDesktopPage : MasterDetailPage, IInitialize
    {
        void SetAllInactive(string pageName)
        {
            if (pageName != "EnvelopesPage")
            {
                EnvelopesFrame.BackgroundColor = EnvelopesFrame.RestingBackgroundColor;
            }
            else
            {
                EnvelopesFrame.BackgroundColor = EnvelopesFrame.ActiveBackgroundColor;
            }

            if (pageName != "AccountsPage")
            {
                AccountsFrame.BackgroundColor = AccountsFrame.RestingBackgroundColor;
            }
            else
            {
                AccountsFrame.BackgroundColor = AccountsFrame.ActiveBackgroundColor;
            }

            if (pageName != "PayeesPage")
            {
                PayeesFrame.BackgroundColor = PayeesFrame.RestingBackgroundColor;
            }
            else
            {
                PayeesFrame.BackgroundColor = PayeesFrame.ActiveBackgroundColor;
            }

            if (pageName != "ReportsPage")
            {
                ReportsFrame.BackgroundColor = ReportsFrame.RestingBackgroundColor;
            }
            else
            {
                ReportsFrame.BackgroundColor = ReportsFrame.ActiveBackgroundColor;
            }

            if (pageName != "SettingsPage")
            {
                SettingsFrame.BackgroundColor = SettingsFrame.RestingBackgroundColor;
            }
            else
            {
                SettingsFrame.BackgroundColor = SettingsFrame.ActiveBackgroundColor;
            }
        }

        public MainDesktopPage()
        {
            InitializeComponent();
        }

        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.TryGetValue<string>(PageParameter.PageName, out string pageName))
            {
                SetAllInactive(pageName);
            }
        }
    }
}
