using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace BudgetBadger.Core.Localization
{
    public class ResourceContainer : IResourceContainer
    {
        readonly ILocalize _localize;

        public ResourceContainer(ILocalize localize)
        {
            _localize = localize;
        }

        public string GetResourceString(string key)
        {
            if (key == null)
            {
                return "";
            }

            AppResources.Culture = _localize.GetLocale();
            var translation = AppResources.ResourceManager.GetString(key);

            if (translation == null)
            {

#if DEBUG
                System.Diagnostics.Debug.WriteLine(String.Format("Key '{0}' was not found in resources.", key));
                //throw new ArgumentException(
                //    String.Format("Key '{0}' was not found in resources.", key),
                //    nameof(key));
#endif
                translation = key; // returns the key, which GETS DISPLAYED TO THE USER
            }

            return translation;
        }

        public string GetFormattedString(string format, object obj)
        {
            return String.Format(_localize.GetLocale(), format, obj);
        }

        public string GetFormattedString(string format, params object[] objs)
        {
            return String.Format(_localize.GetLocale(), format, objs);
        }

        public decimal GetRoundedDecimal(decimal amount)
        {
            var locale = _localize.GetLocale() ?? CultureInfo.CurrentUICulture;
            var nfi = locale.NumberFormat;

            return Decimal.Round(amount, nfi.CurrencyDecimalDigits, MidpointRounding.AwayFromZero);
        }

        public decimal? GetRoundedDecimal(decimal? amount)
        {
            if (amount.HasValue)
            {
                return GetRoundedDecimal(amount.Value);
            }
            else
            {
                return null;
            }
        }
    }
}
