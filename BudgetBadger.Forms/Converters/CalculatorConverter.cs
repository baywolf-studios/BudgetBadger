using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Converters
{

    public class CalculatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            return Decimal.Parse(value.ToString()).ToString("c");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return null;
            }
            else
            {
                if (!decimal.TryParse(value.ToString(), out decimal result))
                {
                    try
                    {
                        var nfi = CultureInfo.CurrentCulture.NumberFormat;
                        var groupSeparator = nfi.CurrencyGroupSeparator;
                        var decimalSeparator = nfi.CurrencyDecimalSeparator;
                        var text = value.ToString().Replace(groupSeparator, "").Replace(decimalSeparator, ".").Replace("(", "-").Replace(")", "");
                        var temp = new DataTable().Compute(text, null);
                        result = System.Convert.ToDecimal(temp);
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
}
