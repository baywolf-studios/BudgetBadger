using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using BudgetBadger.Core.LocalizedResources;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class CurrencyCalculator : Entry
    {
        readonly IResourceContainer _resourceContainer;
        readonly ILocalize _localize;

        public static BindableProperty NumberProperty = BindableProperty.Create(nameof(Number), typeof(decimal?), typeof(CurrencyCalculatorEntry), defaultBindingMode: BindingMode.TwoWay);
        public decimal? Number
        {
            get => (decimal?)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public CurrencyCalculator()
        {
            InitializeComponent();

            _resourceContainer = StaticResourceContainer.Current;
            _localize = DependencyService.Get<ILocalize>();

            Unfocused += UpdateCalculation;

            Completed += UpdateCalculation;

            Focused += (sender, e) =>
            {
                if (Text != null)
                {
                    var locale = _localize.GetLocale() ?? CultureInfo.CurrentUICulture;
                    var nfi = locale.NumberFormat;
                    Text = Text.Replace(nfi.CurrencySymbol, "");
                }

                if (!Number.HasValue)
                {
                    CursorPosition = 0;
                    SelectionLength = 0;
                }
                else if (Number.Value == 0)
                {
                    CursorPosition = 0;
                    SelectionLength = Text.Length;
                }
                else
                {
                    CursorPosition = Text.Length;
                    SelectionLength = 0;
                }
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Number))
                {
                    Text = Number.HasValue ? _resourceContainer.GetFormattedString("{0:C}", Number.Value) : string.Empty;
                }
            };
        }

        void UpdateCalculation(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                Number = null;
            }
            else
            {
                var locale = _localize.GetLocale() ?? CultureInfo.CurrentUICulture;
                var nfi = locale.NumberFormat;

                if (!decimal.TryParse(Text, NumberStyles.Currency, nfi, out decimal result))
                {
                    try
                    {
                        var symbol = nfi.CurrencySymbol;
                        var groupSeparator = nfi.CurrencyGroupSeparator;
                        var decimalSeparator = nfi.CurrencyDecimalSeparator;
                        var text = Text.Replace(symbol, "").Replace(groupSeparator, "").Replace(decimalSeparator, ".").Replace("(", "-").Replace(")", "");
                        var temp = new System.Data.DataTable().Compute(text, null);
                        result = Convert.ToDecimal(temp);
                        result = _resourceContainer.GetRoundedDecimal(result);
                    }
                    catch
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
