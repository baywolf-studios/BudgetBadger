using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ChildPage : ContentPage
    {
        uint _animationLength = 150;

        public static BindableProperty ToolbarItemTextProperty = BindableProperty.Create(nameof(ToolbarItemText), typeof(string), typeof(ChildSearchPage), defaultBindingMode: BindingMode.TwoWay);
        public string ToolbarItemText
        {
            get => (string)GetValue(ToolbarItemTextProperty);
            set => SetValue(ToolbarItemTextProperty, value);
        }

        public static BindableProperty ToolbarItemIconProperty = BindableProperty.Create(nameof(ToolbarItemIcon), typeof(ImageSource), typeof(ChildSearchPage), defaultBindingMode: BindingMode.TwoWay);
        public ImageSource ToolbarItemIcon
        {
            get => (ImageSource)GetValue(ToolbarItemIconProperty);
            set => SetValue(ToolbarItemIconProperty, value);
        }

        public static BindableProperty ToolbarItemCommandProperty = BindableProperty.Create(nameof(ToolbarItemCommand), typeof(ICommand), typeof(ChildSearchPage), defaultBindingMode: BindingMode.TwoWay);
        public ICommand ToolbarItemCommand
        {
            get => (ICommand)GetValue(ToolbarItemCommandProperty);
            set => SetValue(ToolbarItemCommandProperty, value);
        }

        public static BindableProperty BackButtonIconProperty = BindableProperty.Create(nameof(BackButtonIcon), typeof(ImageSource), typeof(ChildSearchPage), defaultBindingMode: BindingMode.TwoWay);
        public ImageSource BackButtonIcon
        {
            get => (ImageSource)GetValue(BackButtonIconProperty);
            set => SetValue(BackButtonIconProperty, value);
        }

        public View BodyContent
        {
            get => BodyView.Content;
            set => BodyView.Content = value;
        }

        public ChildPage()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            ToolbarItemFrame.BindingContext = this;
            ToolbarItemImage.BindingContext = this;

            var backGestureRecognizer = new TapGestureRecognizer();
            backGestureRecognizer.Tapped += BackButtonTapped;
            BackButtonFrame.GestureRecognizers.Add(backGestureRecognizer);
        }

        async void BackButtonTapped(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
