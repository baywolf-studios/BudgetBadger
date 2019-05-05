using System;
using System.Data;
using System.Globalization;
using BudgetBadger.Core.LocalizedResources;
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

            if (!decimal.TryParse(value.ToString(), out decimal result))
            {
                try
                {
                    var locale = DependencyService.Get<ILocalize>().GetLocale() ?? CultureInfo.CurrentUICulture;
                    var nfi = locale.NumberFormat;
                    var symbol = nfi.CurrencySymbol;
                    var groupSeparator = nfi.CurrencyGroupSeparator;
                    var decimalSeparator = nfi.CurrencyDecimalSeparator;
                    var text = value.ToString().Replace(symbol, "").Replace(groupSeparator, "").Replace(decimalSeparator, ".").Replace("(", "-").Replace(")", "");
                    var temp = new DataTable().Compute(text, null);
                    result = Decimal.Parse(temp.ToString());
                    result = Decimal.Round(result, nfi.CurrencyDecimalDigits, MidpointRounding.AwayFromZero);
                }
                catch (Exception ex)
                {
                    result = 0;
                }
            }

            return result;
        }
    }
}