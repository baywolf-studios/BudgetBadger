using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class DateSelector : AbsoluteLayout
    {
        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(DateSelector), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty DateValueProperty = BindableProperty.Create(nameof(DateValue), typeof(DateTime), typeof(DateSelector), defaultBindingMode: BindingMode.TwoWay, defaultValue: DateTime.Now);
        public DateTime DateValue
        {
            get => (DateTime)GetValue(DateValueProperty);
            set => SetValue(DateValueProperty, value);
        }

        public DateSelector()
        {
            InitializeComponent();
            DatePickerControl.BindingContext = this;
            LabelControl.BindingContext = this;
        }
    }
}
