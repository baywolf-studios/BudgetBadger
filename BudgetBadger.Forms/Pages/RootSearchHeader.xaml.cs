using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Pages
{
    public partial class RootSearchHeader : Grid
    {
        uint _animationLength = 150;

        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(RootSearchHeader));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public static BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(RootSearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static BindableProperty SearchCommandProperty = BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(RootSearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public static BindableProperty ToolbarItemTextProperty = BindableProperty.Create(nameof(ToolbarItemText), typeof(string), typeof(RootSearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public string ToolbarItemText
        {
            get => (string)GetValue(ToolbarItemTextProperty);
            set => SetValue(ToolbarItemTextProperty, value);
        }

        public static BindableProperty ToolbarItemIconProperty = BindableProperty.Create(nameof(ToolbarItemIcon), typeof(ImageSource), typeof(RootSearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public ImageSource ToolbarItemIcon
        {
            get => (ImageSource)GetValue(ToolbarItemIconProperty);
            set => SetValue(ToolbarItemIconProperty, value);
        }

        public static BindableProperty ToolbarItemCommandProperty = BindableProperty.Create(nameof(ToolbarItemCommand), typeof(ICommand), typeof(RootSearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand ToolbarItemCommand
        {
            get => (ICommand)GetValue(ToolbarItemCommandProperty);
            set => SetValue(ToolbarItemCommandProperty, value);
        }

        public Dictionary<string, string> ReplaceColor
        {
            get => new Dictionary<string, string> { { "#ffffff", "#FFFFFF" } };
        }

        public RootSearchHeader()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            ToolbarItemFrame.BindingContext = this;
            ToolbarItemImage.BindingContext = this;
            EntryControl.BindingContext = this;
            svgSearch.BindingContext = this;

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += SearchTapped;
            SearchButtonFrame.GestureRecognizers.Add(tapGestureRecognizer);

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

            SizeChanged += (sender, e) =>
            {
                EntryFrame.Margin = Height < 40 ? new Thickness(8, 0) : new Thickness(8);
            };
        }

        async void SearchTapped(object sender, EventArgs e)
        {
            if (!SearchBoxFrame.IsVisible) //currently hidden
            {
                svgSearch.ReplaceStringMap = ReplaceColor;
                svgSearch.Source = "cancel.svg";

                //show it
                SearchBoxFrame.IsVisible = true;
                var translationTask = SearchBoxFrame.FadeTo(1, _animationLength, Easing.CubicOut);
                if (await Task.WhenAny(translationTask, Task.Delay((int)_animationLength + 50)) != translationTask)
                {
                    ViewExtensions.CancelAnimations(SearchBoxFrame);
                    SearchBoxFrame.Opacity = 1;
                }
                EntryControl.Focus();
            }
            else //currently showing
            {
                SearchText = string.Empty;

                svgSearch.ReplaceStringMap = ReplaceColor;
                svgSearch.Source = "search.svg";

                //hide it
                var translationTask = SearchBoxFrame.FadeTo(0, _animationLength, Easing.CubicOut);
                if (await Task.WhenAny(translationTask, Task.Delay((int)_animationLength + 50)) != translationTask)
                {
                    ViewExtensions.CancelAnimations(SearchBoxFrame);
                    SearchBoxFrame.Opacity = 0;
                }
                SearchBoxFrame.IsVisible = false;

            }
        }
    }
}
