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
            set => HeaderView.Content = value;
        }

        public ContentView BodyContentView
        {
            get => BodyView;
        }

        public View Body
        {
            get => BodyView.Content;
            set => BodyView.Content = value;
        }

        public BasePage()
        {
            InitializeComponent();

            SizeChanged += BasePage_SizeChanged;
        }

        void BasePage_SizeChanged(object sender, EventArgs e)
        {
            if (Height > Width)
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
