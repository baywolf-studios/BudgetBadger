using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Settings;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Envelopes;
using BudgetBadger.Forms.Style;
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
                EnvelopesLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_700"];
                EnvelopesIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_700"];
            }
            else
            {
                EnvelopesFrame.BackgroundColor = EnvelopesFrame.ActiveBackgroundColor;
                EnvelopesLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_700"];
                EnvelopesIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_700"];
            }

            if (pageName != "AccountsPage")
            {
                AccountsFrame.BackgroundColor = AccountsFrame.RestingBackgroundColor;
                AccountsLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_700"];
                AccountsIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_700"];
            }
            else
            {
                AccountsFrame.BackgroundColor = AccountsFrame.ActiveBackgroundColor;
                AccountsLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_700"];
                AccountsIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_700"];
            }

            if (pageName != "PayeesPage")
            {
                PayeesFrame.BackgroundColor = PayeesFrame.RestingBackgroundColor;
                PayeesLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_700"];
                PayeesIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_700"];
            }
            else
            {
                PayeesFrame.BackgroundColor = PayeesFrame.ActiveBackgroundColor;
                PayeesLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_700"];
                PayeesIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_700"];
            }

            if (pageName != "ReportsPage")
            {
                ReportsFrame.BackgroundColor = ReportsFrame.RestingBackgroundColor;
                ReportsLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_700"];
                ReportsIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_700"];
            }
            else
            {
                ReportsFrame.BackgroundColor = ReportsFrame.ActiveBackgroundColor;
                ReportsLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_700"];
                ReportsIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_700"];
            }

            if (pageName != "SettingsPage")
            {
                SettingsFrame.BackgroundColor = SettingsFrame.RestingBackgroundColor;
                SettingsLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_700"];
                SettingsIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_700"];
            }
            else
            {
                SettingsFrame.BackgroundColor = SettingsFrame.ActiveBackgroundColor;
                SettingsLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_700"];
                SettingsIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_700"];
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
