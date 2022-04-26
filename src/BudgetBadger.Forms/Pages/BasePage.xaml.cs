using System;
using System.Collections.Generic;
using BudgetBadger.Forms.Style;
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
                    value.SetBinding(View.BackgroundColorProperty, new Binding("[app_bar_color]", source: DynamicResourceProvider.Instance));
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
            get => BodyView.Content;
            set
            {
                if (value != null)
                {
                    value.SetBinding(View.BackgroundColorProperty, new Binding("[gray_50]", source: DynamicResourceProvider.Instance));
                }
                BodyView.Content = value;
            }
        }

        public BasePage()
        {
            InitializeComponent();

            if (Device.Idiom != TargetIdiom.Phone)
            {
                BodyView.HorizontalOptions = LayoutOptions.Center;
                BodyView.WidthRequest = (double)Application.Current.Resources["MaxWidth"];
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
        }
    }
}
