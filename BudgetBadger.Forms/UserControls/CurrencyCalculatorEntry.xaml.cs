using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class CurrencyCalculatorEntry : StackLayout
    {
        public static BindableProperty PrefixProperty = BindableProperty.Create(nameof(Prefix), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Prefix
        {
            get => (string)GetValue(PrefixProperty);
            set => SetValue(PrefixProperty, value);
        }

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

        public CurrencyCalculatorEntry()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;

            TextControl.Unfocused += UpdateCalculation;

            TextControl.Completed += UpdateCalculation;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    TextControl.IsEnabled = IsEnabled;
                }

                if (e.PropertyName == nameof(Number))
                {
                    TextControl.Text = Number.HasValue ? Number.Value.ToString("c") : string.Empty;
                }
            };
        }

        void UpdateCalculation(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TextControl.Text))
            {
                Number = null;
            }
            else
            {
                if (!decimal.TryParse(TextControl.Text, out decimal result))
                {
                    try
                    {
                        var nfi = CultureInfo.CurrentCulture.NumberFormat;
                        var symbol = nfi.CurrencySymbol;
                        var groupSeparator = nfi.CurrencyGroupSeparator;
                        var decimalSeparator = nfi.CurrencyDecimalSeparator;
                        var text = TextControl.Text.Replace(symbol, "").Replace(groupSeparator, "").Replace(decimalSeparator, ".").Replace("(", "-").Replace(")", "");
                        var temp = new DataTable().Compute(text, null);
                        result = Convert.ToDecimal(temp);
                    }
                    catch (Exception ex)
                    {
                        result = 0;
                    }
                }
                Number = Convert.ToDecimal(result);
            }

            OnPropertyChanged(nameof(Number));
        }
    }
}
