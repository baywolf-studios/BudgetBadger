using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Settings;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Envelopes;
using BudgetBadger.Forms.Style;
using BudgetBadger.Forms.UserControls;
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
                EnvelopesFrame.UpdateResting();
                EnvelopesLabel.SetBinding(Label.TextColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
                EnvelopesIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
            }
            else
            {
                EnvelopesFrame.UpdateActive();
                EnvelopesLabel.SetBinding(Label.TextColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
                EnvelopesIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
            }

            if (pageName != "AccountsPage")
            {
                AccountsFrame.UpdateResting();
                AccountsLabel.SetBinding(Label.TextColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
                AccountsIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
            }
            else
            {
                AccountsFrame.UpdateActive();
                AccountsLabel.SetBinding(Label.TextColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
                AccountsIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
            }

            if (pageName != "PayeesPage")
            {
                PayeesFrame.UpdateResting();
                PayeesLabel.SetBinding(Label.TextColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
                PayeesIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
            }
            else
            {
                PayeesFrame.UpdateActive();
                PayeesLabel.SetBinding(Label.TextColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
                PayeesIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
            }

            if (pageName != "ReportsPage")
            {
                ReportsFrame.UpdateResting();
                ReportsLabel.SetBinding(Label.TextColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
                ReportsIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
            }
            else
            {
                ReportsFrame.UpdateActive();
                ReportsLabel.SetBinding(Label.TextColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
                ReportsIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
            }

            if (pageName != "SettingsPage")
            {
                SettingsFrame.UpdateResting();
                SettingsLabel.SetBinding(Label.TextColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
                SettingsIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[gray_600]", source: DynamicResourceProvider.Instance));
            }
            else
            {
                SettingsFrame.UpdateActive();
                SettingsLabel.SetBinding(Label.TextColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
                SettingsIconFont.SetBinding(FontImageSource.ColorProperty, new Binding("[brand_600]", source: DynamicResourceProvider.Instance));
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
