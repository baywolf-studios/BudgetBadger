using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class SearchHeader : AbsoluteLayout
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
            if (SearchBoxFrame.TranslationX > 0) //currently hidden
            {
                //SearchBoxFrame.InputTransparent = false;
                //show it
                await SearchBoxFrame.TranslateTo(0, 0, _animationLength, Easing.CubicOut);
                EntryControl.Focus();
            }
            else //currently showing
            {
                //SearchBoxFrame.InputTransparent = true;

                SearchText = string.Empty;

                //hide it
                await SearchBoxFrame.TranslateTo(SearchBoxFrame.Width, 0, _animationLength, Easing.CubicOut);
            }
        }

        public SearchHeader()
        {
            InitializeComponent();

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
