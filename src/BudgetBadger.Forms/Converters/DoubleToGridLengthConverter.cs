using System;
using System.Globalization;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Converters
{
    public class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OnIdiom<double> onIdiom)
            {
                var doubleValue = GetValue(onIdiom);
                return new GridLength(doubleValue);
            }

            if (value is double length)
            {
                return new GridLength(length);
            }

            throw new FormatException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        double GetValue(OnIdiom<double> idiom)
        {
            switch (Device.Idiom)
            {
                case TargetIdiom.Phone:
                    return idiom.Phone;
                case TargetIdiom.Tablet:
                    return idiom.Tablet;
                case TargetIdiom.Desktop:
                    return idiom.Desktop;
                case TargetIdiom.TV:
                    return idiom.TV;
                case TargetIdiom.Watch:
                    return idiom.Watch;
                default:
                    return idiom.Default;
            }
        }
    }
}
