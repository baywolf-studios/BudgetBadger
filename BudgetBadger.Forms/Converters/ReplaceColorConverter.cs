using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Converters
{
    public class ReplaceColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var PreferredColor = (Color)value;
                var ReplaceMap = new Dictionary<string, string>
                {
                    { "#ffffff", PreferredColor.GetHexString() }
                };

                return ReplaceMap;
            }

            return "#ffffff";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
