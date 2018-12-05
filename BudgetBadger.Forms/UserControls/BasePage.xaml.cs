using System;
using System.Collections.Generic;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class BasePage : ContentPage
    {
        Layout _header;
        public Layout Header
        {
            get => _header;
            set
            {
                _header = value;

                SetHeader();
            }
        }

        public View Body
        {
            get => BodyView.Content;
            set => BodyView.Content = value;
        }

        public BasePage()
        {
            InitializeComponent();

            MainGrid.SizeChanged += HeaderUpdated;
        }

        void SetHeader()
        {
            if (Device.RuntimePlatform == Device.UWP || Device.RuntimePlatform == Device.macOS)
            {
                Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);
                MainGrid.Children.Add(Header);
                Grid.SetRow(Header, 0);
            }
            else
            {
                Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, true);

                Xamarin.Forms.NavigationPage.SetTitleView(this, Header);

                Header.HeightRequest = 56;

                Header.SizeChanged += HeaderUpdated;
            }
        }

        void HeaderUpdated(object sender, EventArgs e)
        {
            var pageHeight = Height;
            var bodyHeight = Body.Height;

            var headerWidth = Header.Width + Header.Margin.Left + Header.Margin.Right;
            if (Device.RuntimePlatform == Device.Android)
            {
                var newPadding = (MainGrid.Width - headerWidth) * -1;
                Header.Padding = new Thickness(newPadding, 0, 0, 0);
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                var newPadding = ((MainGrid.Width - headerWidth) / 2) * -1;
                Header.Padding = new Thickness(newPadding, 0, newPadding, 0);
            }
        }
    }
}
