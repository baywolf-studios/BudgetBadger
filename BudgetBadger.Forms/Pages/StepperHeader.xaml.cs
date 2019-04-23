﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Pages
{
    public partial class StepperHeader : Grid
    {
        uint _animationLength = 150;

        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(StepperHeader));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public static BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(StepperHeader), defaultBindingMode: BindingMode.TwoWay);
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static BindableProperty SearchCommandProperty = BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(StepperHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public static BindableProperty ToolbarItemTextProperty = BindableProperty.Create(nameof(ToolbarItemText), typeof(string), typeof(StepperHeader), defaultBindingMode: BindingMode.TwoWay);
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

        public static BindableProperty ToolbarItemCommandProperty = BindableProperty.Create(nameof(ToolbarItemCommand), typeof(ICommand), typeof(StepperHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand ToolbarItemCommand
        {
            get => (ICommand)GetValue(ToolbarItemCommandProperty);
            set => SetValue(ToolbarItemCommandProperty, value);
        }

        public static BindableProperty NextCommandProperty = BindableProperty.Create(nameof(NextCommand), typeof(ICommand), typeof(StepperHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand NextCommand
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        public static BindableProperty PreviousCommandProperty = BindableProperty.Create(nameof(PreviousCommand), typeof(ICommand), typeof(StepperHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand PreviousCommand
        {
            get => (ICommand)GetValue(PreviousCommandProperty);
            set => SetValue(PreviousCommandProperty, value);
        }

        public Dictionary<string, string> ReplaceColor
        {
            get => new Dictionary<string, string> { { "#ffffff", "#FFFFFF" } };
        }


        public StepperHeader()
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

            SizeChanged += (sender, e) => 
            {
                EntryFrame.Margin = Height < 40 ? new Thickness(8, 0) : new Thickness(8);
            };
        }

        async void Handle_Tapped(object sender, System.EventArgs e)
        {
            var originalColor = ToolbarItemFrame.BackgroundColor;

            ToolbarItemFrame.BackgroundColor = Color.Red;

            var colorTask2 = ToolbarItemFrame.ColorTo(Color.Red, originalColor, (Color obj2) => ToolbarItemFrame.BackgroundColor = obj2, _animationLength, Easing.CubicInOut);
            if (await Task.WhenAny(colorTask2, Task.Delay((int)_animationLength + 50)) != colorTask2)
            {
                ViewExtensions.CancelAnimations(ToolbarItemFrame);
                ToolbarItemFrame.BackgroundColor = originalColor;
            }
        }

        async void SearchTapped(object sender, EventArgs e)
        {
            if (!SearchBoxFrame.IsVisible) //currently hidden
            {
                SearchImage.ReplaceStringMap = ReplaceColor;
                SearchImage.Source = "cancel.svg";

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

                SearchImage.ReplaceStringMap = ReplaceColor;
                SearchImage.Source = "search.svg";

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
