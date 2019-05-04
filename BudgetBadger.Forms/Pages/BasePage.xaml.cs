using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace BudgetBadger.Forms.Pages
{
    public partial class BasePage : ContentPage
    {
        double _appBarPortraitHeight
        {
            get
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        return 48;
                    case Device.Android:
                        return 56;
                    default:
                        return 48;
                }
            }
        }

        double _appBarLandscapeHeight
        {
            get
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        return 32;
                    case Device.Android:
                        return 48;
                    default:
                        return 48;
                }
            }
        }

        public ContentView HeaderContentView 
        {
            get => HeaderView;
        }

        public View Header
        {
            get => HeaderView.Content;
            set
            {
                if (value != null)
                {
                    value.BackgroundColor = (Color)Application.Current.Resources["AppBarColor"];
                }
                HeaderView.Content = value;
            }
        }

        public ContentView BodyContentView
        {
            get => BodyView;
        }

        public View Body
        {
            get => BodyView.Body;
            set
            {
                if (value != null)
                {
                    value.BackgroundColor = (Color)Application.Current.Resources["BackgroundColor"];
                }
                BodyView.Body = value;
            }
        }

        public BasePage()
        {
            InitializeComponent();

            if (Device.Idiom == TargetIdiom.Phone)
            {
                HeaderView.Elevation = 0;
                BodyView.Elevation = 0;
                BodyView.Margin = new Thickness(0);
            }
            else
            {
                HeaderView.Elevation = 4;
                BodyView.Elevation = 1;
                BodyView.HorizontalOptions = LayoutOptions.Center;
                BodyView.WidthRequest = (double)Application.Current.Resources["MaxWidth"];
                BodyView.Margin = new Thickness(32);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (height > width)
            {
                // portrait
                HeaderView.HeightRequest = _appBarPortraitHeight;
            }
            else
            {
                // landscape
                HeaderView.HeightRequest = _appBarLandscapeHeight;
            }

            if (width > 0
                && Device.RuntimePlatform != Device.macOS
                && Device.Idiom != TargetIdiom.Phone)
            {
                if (width > (double)Application.Current.Resources["MaxWidth"] && BodyView.Margin.Top < 32)
                {
                    BodyView.Margin = new Thickness(32);
                    HeaderView.Elevation = 4;
                    BodyView.Elevation = 1;
                }
                else if (width <= (double)Application.Current.Resources["MaxWidth"] && BodyView.Margin.Top > 0)
                {
                    BodyView.Margin = new Thickness(0);
                    HeaderView.Elevation = 0;
                    BodyView.Elevation = 0;
                }
            }
        }
    }
}
