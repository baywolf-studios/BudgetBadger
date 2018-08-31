using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class RootPage : ContentPage
    {
        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(StepperPage));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public View BodyContent
        {
            get => BodyView.Content;
            set => BodyView.Content = value;
        }

        public RootPage()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
        }
    }
}
