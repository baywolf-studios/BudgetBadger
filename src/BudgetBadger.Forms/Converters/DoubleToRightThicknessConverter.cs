﻿
using System;
using System.Globalization;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Converters
{
    public class DoubleToRightThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double leftRight)
            {
                return new Thickness(0, 0, leftRight, 0);
            }

            throw new FormatException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
