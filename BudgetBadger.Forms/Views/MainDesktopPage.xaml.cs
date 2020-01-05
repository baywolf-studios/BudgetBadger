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
                EnvelopesLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_600"];
                EnvelopesIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_600"];
            }
            else
            {
                EnvelopesFrame.BackgroundColor = EnvelopesFrame.ActiveBackgroundColor;
                EnvelopesLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_1000"];
                EnvelopesIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_1000"];
            }

            if (pageName != "AccountsPage")
            {
                AccountsFrame.BackgroundColor = AccountsFrame.RestingBackgroundColor;
                AccountsLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_600"];
                AccountsIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_600"];
            }
            else
            {
                AccountsFrame.BackgroundColor = AccountsFrame.ActiveBackgroundColor;
                AccountsLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_1000"];
                AccountsIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_1000"];
            }

            if (pageName != "PayeesPage")
            {
                PayeesFrame.BackgroundColor = PayeesFrame.RestingBackgroundColor;
                PayeesLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_600"];
                PayeesIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_600"];
            }
            else
            {
                PayeesFrame.BackgroundColor = PayeesFrame.ActiveBackgroundColor;
                PayeesLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_1000"];
                PayeesIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_1000"];
            }

            if (pageName != "ReportsPage")
            {
                ReportsFrame.BackgroundColor = ReportsFrame.RestingBackgroundColor;
                ReportsLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_600"];
                ReportsIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_600"];
            }
            else
            {
                ReportsFrame.BackgroundColor = ReportsFrame.ActiveBackgroundColor;
                ReportsLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_1000"];
                ReportsIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_1000"];
            }

            if (pageName != "SettingsPage")
            {
                SettingsFrame.BackgroundColor = SettingsFrame.RestingBackgroundColor;
                SettingsLabel.TextColor = (Color)DynamicResourceProvider.Instance["gray_600"];
                SettingsIconFont.Color = (Color)DynamicResourceProvider.Instance["gray_600"];
            }
            else
            {
                SettingsFrame.BackgroundColor = SettingsFrame.ActiveBackgroundColor;
                SettingsLabel.TextColor = (Color)DynamicResourceProvider.Instance["brand_1000"];
                SettingsIconFont.Color = (Color)DynamicResourceProvider.Instance["brand_1000"];
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
