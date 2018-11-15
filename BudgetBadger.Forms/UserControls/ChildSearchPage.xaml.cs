using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Navigation;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ChildSearchPage : ContentPage
    {
        uint _animationLength = 150;

        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(StepperPage));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public static BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(ChildSearchPage), defaultBindingMode: BindingMode.TwoWay);
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static BindableProperty SearchCommandProperty = BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(ChildSearchPage), defaultBindingMode: BindingMode.TwoWay);
        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
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

        public ChildSearchPage()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            ToolbarItemFrame.BindingContext = this;
            ToolbarItemImage.BindingContext = this;
            EntryControl.BindingContext = this;
            BackButtonImage.BindingContext = this;
            BackButtonFrame.BindingContext = this;
            svgSearch.BindingContext = this;

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += SearchTapped;
            SearchButtonFrame.GestureRecognizers.Add(tapGestureRecognizer);

            //var backGestureRecognizer = new TapGestureRecognizer();
            //backGestureRecognizer.Tapped += BackButtonTapped;
            //BackButtonFrame.GestureRecognizers.Add(backGestureRecognizer);

            EntryControl.TextChanged += (sender, e) =>
            {
                if (e.OldTextValue != e.NewTextValue)
                {
                    if (SearchCommand?.CanExecute(SearchText) != false)
                    {
                        SearchCommand?.Execute(SearchText);
                    }
                }
            };

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

        async void SearchTapped(object sender, EventArgs e)
        {
            if (!SearchBoxFrame.IsVisible) //currently hidden
            {
                svgSearch.ReplaceStringMap = ReplaceColor;
                svgSearch.Source = "cancel.svg";

                //show it
                SearchBoxFrame.IsVisible = true;
                var translationTask = SearchBoxFrame.TranslateTo(0, 0, _animationLength, Easing.CubicOut);
                if (await Task.WhenAny(translationTask, Task.Delay((int)_animationLength + 50)) != translationTask)
                {
                    ViewExtensions.CancelAnimations(SearchBoxFrame);
                    SearchBoxFrame.TranslationX = SearchBoxFrame.Width;
                }
                EntryControl.Focus();
            }
            else //currently showing
            {
                SearchText = string.Empty;

                svgSearch.ReplaceStringMap = ReplaceColor;
                svgSearch.Source = "search.svg";

                //hide it
                var translationTask = SearchBoxFrame.TranslateTo(SearchBoxFrame.Width, 0, _animationLength, Easing.CubicOut);
                if (await Task.WhenAny(translationTask, Task.Delay((int)_animationLength + 50)) != translationTask)
                {
                    ViewExtensions.CancelAnimations(SearchBoxFrame);
                    SearchBoxFrame.TranslationX = SearchBoxFrame.Width;
                }
                SearchBoxFrame.IsVisible = false;
            }
        }

        async void BackButtonTapped(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
