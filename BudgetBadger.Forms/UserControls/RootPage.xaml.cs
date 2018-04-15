using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class RootPage : ContentPage
    {
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
