using System;
using System.Globalization;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Converters
{
	public class TranslationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return StaticResourceContainer.Current.GetResourceString(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

