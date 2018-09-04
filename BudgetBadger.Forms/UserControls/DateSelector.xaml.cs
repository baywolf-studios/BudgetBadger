using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class DateSelector : StackLayout
    {
        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(DateSelector));
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty DateProperty =
            BindableProperty.Create(nameof(Date),
                                    typeof(DateTime),
                                    typeof(DateSelector),
                                    defaultValue: DateTime.Now,
                                    defaultBindingMode: BindingMode.TwoWay);
        public DateTime Date
        {
            get => (DateTime)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public event EventHandler<DateChangedEventArgs> DateSelected;

        public DateSelector()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            DateControl.BindingContext = this;

            DateControl.DateSelected += (sender, e) => 
            {
                DateSelected?.Invoke(this, e);
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    DateControl.IsEnabled = IsEnabled;
                }
            };
        }
    }
}

