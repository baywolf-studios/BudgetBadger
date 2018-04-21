using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class StepperPage : ContentPage
    {
        uint _animationLength = 150;

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
            get => ToolbarItemImage.Source;
            set  { ToolbarItemImage.ReplaceStringMap = ReplaceColor; ToolbarItemImage.Source = value; }
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

        public Dictionary<string, string> ReplaceColor
        {
            get => new Dictionary<string, string> { { "currentColor", "#FFFFFF" } };
        }

        public StepperPage()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            ToolbarItemFrame.BindingContext = this;
            ToolbarItemImage.BindingContext = this;
            PreviousFrame.BindingContext = this;
            PreviousImage.BindingContext = this;
            NextFrame.BindingContext = this;
            NextImage.BindingContext = this;
            EntryControl.BindingContext = this;
            SearchImage.BindingContext = this;

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
    }
}

