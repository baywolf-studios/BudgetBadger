using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class CurrencyCalculatorEntry : StackLayout
    {
        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(CurrencyCalculatorEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty NumberProperty = BindableProperty.Create(nameof(Number), typeof(decimal?), typeof(CurrencyCalculatorEntry), defaultBindingMode: BindingMode.TwoWay);
        public decimal? Number
        {
            get => (decimal?)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public static BindableProperty HintProperty = BindableProperty.Create(nameof(Hint), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public static BindableProperty ErrorProperty = BindableProperty.Create(nameof(Error), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public CurrencyCalculatorEntry()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
            TextControl.BindingContext = this;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    TextControl.IsEnabled = IsEnabled;
                }

                if (e.PropertyName == nameof(Hint) || e.PropertyName == nameof(Error))
                {
                    if (!String.IsNullOrEmpty(Error))
                    {
                        HintErrorControl.IsVisible = true;
                        HintErrorControl.Text = Error;
                        HintErrorControl.TextColor = (Color)Application.Current.Resources["ErrorColor"];
                    }
                    else if (!String.IsNullOrEmpty(Hint))
                    {
                        HintErrorControl.IsVisible = true;
                        HintErrorControl.Text = Hint;
                        HintErrorControl.TextColor = (Color)Application.Current.Resources["IdleColor"];
                    }
                    else
                    {
                        HintErrorControl.IsVisible = false;
                    }
                }
            };
        }
    }
}
