using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Pages
{
    public partial class ChildSearchHeader : Grid
    {
        uint _animationLength = 150;

        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(ChildSearchHeader));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public static BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(ChildSearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static BindableProperty SearchCommandProperty = BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(ChildSearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public static BindableProperty ToolbarItemTextProperty = BindableProperty.Create(nameof(ToolbarItemText), typeof(string), typeof(ChildSearchHeader), defaultBindingMode: BindingMode.TwoWay);
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

        public static BindableProperty ToolbarItemCommandProperty = BindableProperty.Create(nameof(ToolbarItemCommand), typeof(ICommand), typeof(ChildSearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand ToolbarItemCommand
        {
            get => (ICommand)GetValue(ToolbarItemCommandProperty);
            set => SetValue(ToolbarItemCommandProperty, value);
        }

        public static BindableProperty BackCommandProperty = BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(ChildSearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        public Dictionary<string, string> ReplaceColor
        {
            get => new Dictionary<string, string> { { "#ffffff", "#FFFFFF" } };
        }

        public ChildSearchHeader()
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
