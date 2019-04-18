using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Checkbox : StackLayout
    {
        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Checkbox));
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty CaptionProperty = BindableProperty.Create(nameof(Caption), typeof(string), typeof(Checkbox));
        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(Checkbox), false, BindingMode.TwoWay);
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
        public Checkbox()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            CaptionControl.BindingContext = this;
            SwitchControl.BindingContext = this;

            PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    SwitchControl.IsEnabled = IsEnabled;
                }
            };
        }
    }
}
