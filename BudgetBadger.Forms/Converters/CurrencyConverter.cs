using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Converters
{

    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Decimal.Parse(value.ToString()).ToString("C", culture.NumberFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string valueFromString = Regex.Replace(value.ToString(), @"\D", "");

            if (valueFromString.Length <= 0)
            {
                return 0m;
            }

            if (!long.TryParse(valueFromString, out long valueLong))
            {
                return 0m;
            }

            if (valueLong <= 0)
            {
                return 0m;
            }

            return valueLong / 100m;
        }
    }
}
