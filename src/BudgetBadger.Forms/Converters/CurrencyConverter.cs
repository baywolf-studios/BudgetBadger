using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using BudgetBadger.Core.Localization;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return StaticResourceContainer.Current.GetFormattedString("{0:C}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var locale = DependencyService.Get<ILocalize>().GetLocale() ?? CultureInfo.CurrentUICulture;
            var nfi = locale.NumberFormat;

            if (!decimal.TryParse(value.ToString(), NumberStyles.Currency, nfi, out decimal result))
            {
                try
                {
                    var symbol = nfi.CurrencySymbol;
                    var groupSeparator = nfi.CurrencyGroupSeparator;
                    var decimalSeparator = nfi.CurrencyDecimalSeparator;
                    var text = value.ToString().Replace(symbol, "").Replace(groupSeparator, "").Replace(decimalSeparator, ".").Replace("(", "-").Replace(")", "");
                    var temp = new DataTable().Compute(text, null);
                    result = Decimal.Parse(temp.ToString());
                    result = StaticResourceContainer.Current.GetRoundedDecimal(result);
                }
                catch
                {
                    result = 0;
                }
            }

            return result;
        }
    }
}