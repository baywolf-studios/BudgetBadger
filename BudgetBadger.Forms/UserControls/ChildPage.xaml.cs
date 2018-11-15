using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ChildPage : ContentPage
    {
        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(StepperPage));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public static BindableProperty ToolbarItemTextProperty = BindableProperty.Create(nameof(ToolbarItemText), typeof(string), typeof(ChildSearchPage), defaultBindingMode: BindingMode.TwoWay);
        public string ToolbarItemText
        {
            get => (string)GetValue(ToolbarItemTextProperty);
            set => SetValue(ToolbarItemTextProperty, value);
        }

        public ImageSource ToolbarItemIcon
        {
            get => ToolbarItemImage.Source;
            set { ToolbarItemImage.ReplaceStringMap = ReplaceColor; ToolbarItemImage.Source = value; }
        }

        public static BindableProperty ToolbarItemCommandProperty = BindableProperty.Create(nameof(ToolbarItemCommand), typeof(ICommand), typeof(ChildSearchPage), defaultBindingMode: BindingMode.TwoWay);
        public ICommand ToolbarItemCommand
        {
            get => (ICommand)GetValue(ToolbarItemCommandProperty);
            set => SetValue(ToolbarItemCommandProperty, value);
        }

        public static BindableProperty BackCommandProperty = BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(ChildSearchPage), defaultBindingMode: BindingMode.TwoWay);
        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        public View BodyContent
        {
            get => BodyView.Content;
            set => BodyView.Content = value;
        }

        public Dictionary<string, string> ReplaceColor
        {
            get => new Dictionary<string, string> { { "#ffffff", "#FFFFFF" } };
        }

        public ChildPage()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            ToolbarItemFrame.BindingContext = this;
            ToolbarItemImage.BindingContext = this;
            BackButtonFrame.BindingContext = this;
            BackButtonImage.BindingContext = this;

            //var backGestureRecognizer = new TapGestureRecognizer();
            //backGestureRecognizer.Tapped += BackButtonTapped;
            //BackButtonFrame.GestureRecognizers.Add(backGestureRecognizer);

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

        async void BackButtonTapped(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
