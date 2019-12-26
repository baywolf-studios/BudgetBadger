using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class HeaderColumn : ContentView
    {
        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(HeaderColumn), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public TextAlignment HorizontalTextAlignment
        {
            get => TextControl.HorizontalTextAlignment;
            set => TextControl.HorizontalTextAlignment = value;
        }

        public HeaderColumn() : this(false) { }

        public HeaderColumn(bool dense)
        {
            InitializeComponent();
            if (dense)
            {
                if (Application.Current.Resources.TryGetValue("DenseDataTableColumnHeaderLabelStyle", out object resource))
                {
                    TextControl.Style = (Xamarin.Forms.Style)resource;
                }
            }
            else
            {
                if (Application.Current.Resources.TryGetValue("DataTableColumnHeaderLabelStyle", out object resource))
                {
                    TextControl.Style = (Xamarin.Forms.Style)resource;
                }
            }
            TextControl.BindingContext = this;
        }
    }
}
