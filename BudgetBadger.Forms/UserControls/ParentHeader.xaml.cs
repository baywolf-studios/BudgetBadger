using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ParentHeader : ContentView
    {
        uint _animationLength = 150;

        public static BindableProperty SearchTextProperty = BindableProperty.Create(nameof(SearchText), typeof(string), typeof(SearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static BindableProperty SearchCommandProperty = BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(SearchHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
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

        public static BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(ParentHeader), defaultBindingMode: BindingMode.TwoWay);
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static BindableProperty ToolbarItemProperty = BindableProperty.Create(nameof(ToolbarItem), typeof(ToolbarItem), typeof(ParentHeader), defaultBindingMode: BindingMode.TwoWay);
        public ToolbarItem ToolbarItem
        {
            get => (ToolbarItem)GetValue(ToolbarItemProperty);
            set => SetValue(ToolbarItemProperty, value);
        }

        public View ChildContent
        {
            get => ChildView.Content;
            set => ChildView.Content = value;
        }

        public ParentHeader()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            ToolbarItemFrame.BindingContext = this;
            ToolbarItemImage.BindingContext = this;
            EntryControl.BindingContext = this;

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
    }
}
