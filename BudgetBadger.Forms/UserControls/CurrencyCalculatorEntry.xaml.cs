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
        readonly IResourceContainer _resourceContainer;
        readonly ILocalize _localize;

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

            _resourceContainer = StaticResourceContainer.Current;
            _localize = DependencyService.Get<ILocalize>();

            LabelControl.BindingContext = this;

            TextControl.Unfocused += UpdateCalculation;

            TextControl.Completed += UpdateCalculation;

            TextControl.Focused += (sender, e) =>
            {
                if (!Number.HasValue)
                {
                    TextControl.CursorPosition = 0;
                    TextControl.SelectionLength = 0;
                }
                else if (Number.Value == 0)
                {
                    TextControl.CursorPosition = 0;
                    TextControl.SelectionLength = TextControl.Text.Length;
                }
                else
                {
                    TextControl.CursorPosition = TextControl.Text.Length;
                    TextControl.SelectionLength = 0;
                }
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    TextControl.IsEnabled = IsEnabled;
                }

                if (e.PropertyName == nameof(Number))
                {
                    TextControl.Text = Number.HasValue ? _resourceContainer.GetFormattedString("{0:C}", Number.Value) : string.Empty;
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

        void UpdateCalculation(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TextControl.Text))
            {
                Number = null;
            }
            else
            {
                var locale = DependencyService.Get<ILocalize>().GetLocale() ?? CultureInfo.CurrentUICulture;
                var nfi = locale.NumberFormat;

                if (!decimal.TryParse(TextControl.Text, NumberStyles.Currency, nfi, out decimal result))
                {
                    try
                    {
                        var symbol = nfi.CurrencySymbol;
                        var groupSeparator = nfi.CurrencyGroupSeparator;
                        var decimalSeparator = nfi.CurrencyDecimalSeparator;
                        var text = TextControl.Text.Replace(symbol, "").Replace(groupSeparator, "").Replace(decimalSeparator, ".").Replace("(", "-").Replace(")", "");
                        var temp = new DataTable().Compute(text, null);
                        result = Convert.ToDecimal(temp);
                        result = Decimal.Round(result, nfi.CurrencyDecimalDigits, MidpointRounding.AwayFromZero);
                    }
                    catch (Exception ex)
                    {
                        result = 0;
                    }
                }
                Number = result;
            }

            OnPropertyChanged(nameof(Number));
        }
    }
}
