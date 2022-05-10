using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class BusyIndicator : ContentView
    {
        public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(
            "IsBusy",
            typeof(bool),
            typeof(ContentView), 
            false); 
        
        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly BindableProperty BusyTextProperty = BindableProperty.Create(
            "BusyText",
            typeof(string),
            typeof(ContentView),
            string.Empty);
        
        public string BusyText
        {
            get { return (string)GetValue(BusyTextProperty); }
            set { SetValue(BusyTextProperty, value); }
        }

        public BusyIndicator()
        {
            InitializeComponent();
        }
    }
}
