using System;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Converters
{
    public class InverseThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Thickness thickness)
            {
                var newThickness = new Thickness(-1 * thickness.Left, -1 * thickness.Top, -1 * thickness.Right, -1 * thickness.Bottom);
                return newThickness;
            }
            return new Thickness();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Thickness thickness)
            {
                var newThickness = new Thickness(-1 * thickness.Left, -1 * thickness.Top, -1 * thickness.Right, -1 * thickness.Bottom);
                return newThickness;
            }
            return new Thickness();
        }
    }
}
