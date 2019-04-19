using System;
using System.Globalization;
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
            throw new NotImplementedException();
        }
    }
}