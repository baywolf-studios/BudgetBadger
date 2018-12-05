using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class StepperPage : ContentPage
    {
        StepperHeader _header { get; set; }

        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(StepperPage));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public static BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(StepperPage), defaultBindingMode: BindingMode.TwoWay);
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static BindableProperty SearchCommandProperty = BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(StepperPage), defaultBindingMode: BindingMode.TwoWay);
        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public static BindableProperty ToolbarItemTextProperty = BindableProperty.Create(nameof(ToolbarItemText), typeof(string), typeof(StepperPage), defaultBindingMode: BindingMode.TwoWay);
        public string ToolbarItemText
        {
            get => (string)GetValue(ToolbarItemTextProperty);
            set => SetValue(ToolbarItemTextProperty, value);
        }

        public ImageSource ToolbarItemIcon
        {
            get => _header.ToolbarItemIcon;
            set => _header.ToolbarItemIcon = value;
        }

        public static BindableProperty ToolbarItemCommandProperty = BindableProperty.Create(nameof(ToolbarItemCommand), typeof(ICommand), typeof(StepperPage), defaultBindingMode: BindingMode.TwoWay);
        public ICommand ToolbarItemCommand
        {
            get => (ICommand)GetValue(ToolbarItemCommandProperty);
            set => SetValue(ToolbarItemCommandProperty, value);
        }

        public static BindableProperty NextCommandProperty = BindableProperty.Create(nameof(NextCommand), typeof(ICommand), typeof(StepperPage), defaultBindingMode: BindingMode.TwoWay);
        public ICommand NextCommand
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        public static BindableProperty PreviousCommandProperty = BindableProperty.Create(nameof(PreviousCommand), typeof(ICommand), typeof(StepperPage), defaultBindingMode: BindingMode.TwoWay);
        public ICommand PreviousCommand
        {
            get => (ICommand)GetValue(PreviousCommandProperty);
            set => SetValue(PreviousCommandProperty, value);
        }

        public View BodyContent
        {
            get => BodyView.Content;
            set => BodyView.Content = value;
        }

        public StepperPage()
        {
            InitializeComponent();

            _header = new StepperHeader
            {
                BindingContext = this
            };
            _header.SetBinding(StepperHeader.PreviousCommandProperty, "PreviousCommand");
            _header.SetBinding(StepperHeader.NextCommandProperty, "NextCommand");
            _header.SetBinding(StepperHeader.PageTitleProperty, "PageTitle");
            _header.SetBinding(StepperHeader.ToolbarItemCommandProperty, "ToolbarItemCommand");
            _header.SetBinding(StepperHeader.ToolbarItemTextProperty, "ToolberItemText");
            _header.SetBinding(StepperHeader.SearchTextProperty, "SearchText");
            _header.SetBinding(StepperHeader.SearchCommandProperty, "SearchCommand");

            if (Device.RuntimePlatform == Device.UWP || Device.RuntimePlatform == Device.macOS)
            {
                NavigationPage.SetHasNavigationBar(this, false);
                MainGrid.Children.Add(_header);
                Grid.SetRow(_header, 0);
            }
            else
            {
                NavigationPage.SetHasNavigationBar(this, true);
                NavigationPage.SetTitleView(this, _header);

                MainGrid.SizeChanged += Header_SizeChanged;
                _header.SizeChanged += Header_SizeChanged;
            }
        }

        void Header_SizeChanged(object sender, EventArgs e)
        {
            var headerWidth = _header.Width + _header.Margin.Left + _header.Margin.Right;

            if (Device.RuntimePlatform == Device.Android)
            {
                var newPadding = (MainGrid.Width - headerWidth) * -1;
                _header.Padding = new Thickness(newPadding, 0, 0, 0);
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                var newPadding = ((MainGrid.Width - headerWidth) / 2) * -1;
                _header.Padding = new Thickness(newPadding, 0, newPadding, 0);
            }
        }
    }
}

