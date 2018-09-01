using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ChildPage : ContentPage
    {
        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(StepperPage));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
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
            get => new Dictionary<string, string> { { "currentColor", "#FFFFFF" } };
        }

        public ChildPage()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            ToolbarItemFrame.BindingContext = this;
            ToolbarItemImage.BindingContext = this;
            BackButtonFrame.BindingContext = this;
            BackButtonImage.BindingContext = this;

            //var backGestureRecognizer = new TapGestureRecognizer();
            //backGestureRecognizer.Tapped += BackButtonTapped;
            //BackButtonFrame.GestureRecognizers.Add(backGestureRecognizer);
        }

        async void BackButtonTapped(object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
