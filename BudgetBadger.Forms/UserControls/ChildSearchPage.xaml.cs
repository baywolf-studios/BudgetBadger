using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ChildSearchPage : ContentPage
    {
        uint _animationLength = 150;

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

        public View BodyContent
        {
            get => BodyView.Content;
            set => BodyView.Content = value;
        }

        public Dictionary<string, string> ReplaceColor
        {
            get => new Dictionary<string, string> { { "currentColor", "#FFFFFF" } };
        }

        public ChildSearchPage()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            ToolbarItemFrame.BindingContext = this;
            ToolbarItemImage.BindingContext = this;
            EntryControl.BindingContext = this;
            BackButtonImage.BindingContext = this;
            svgSearch.BindingContext = this;

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += SearchTapped;
            SearchButtonFrame.GestureRecognizers.Add(tapGestureRecognizer);

            var backGestureRecognizer = new TapGestureRecognizer();
            backGestureRecognizer.Tapped += BackButtonTapped;
            BackButtonFrame.GestureRecognizers.Add(backGestureRecognizer);

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
                //show it
                SearchBoxFrame.IsVisible = true;
                await SearchBoxFrame.TranslateTo(0, 0, _animationLength, Easing.CubicOut);
                EntryControl.Focus();
            }
            else //currently showing
            {
                SearchText = string.Empty;

                //hide it
                await SearchBoxFrame.TranslateTo(SearchBoxFrame.Width, 0, _animationLength, Easing.CubicOut);
                SearchBoxFrame.IsVisible = false;
            }
        }

        async void BackButtonTapped(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
