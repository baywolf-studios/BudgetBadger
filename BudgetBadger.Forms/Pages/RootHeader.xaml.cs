using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Pages
{
    public partial class RootHeader : Grid
    {
        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(RootHeader));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public RootHeader()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
        }
    }
}
