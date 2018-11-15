using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class RootPage : ContentPage
    {
        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(StepperPage));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public View BodyContent
        {
            get => BodyView.Content;
            set => BodyView.Content = value;
        }

        public RootPage()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;

            DeviceDisplay.ScreenMetricsChanged += DeviceDisplay_ScreenMetricsChanged;
            DeviceDisplay_ScreenMetricsChanged(null, null);
        }

        void DeviceDisplay_ScreenMetricsChanged(object sender, ScreenMetricsChangedEventArgs e)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                var version = DeviceInfo.Version;
                if (version.Major < 11)
                {
                    var metrics = DeviceDisplay.ScreenMetrics;
                    var orientation = metrics.Orientation;

                    if (orientation == ScreenOrientation.Portrait || Device.Idiom == TargetIdiom.Tablet)
                    {
                        Padding = new Thickness(0, 20, 0, 0);
                    }
                    else
                    {
                        Padding = new Thickness();
                    }
                }
            }
        }
    }
}
