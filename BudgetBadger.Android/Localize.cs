using System;
using System.Globalization;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(Localize))]
namespace BudgetBadger.Droid
{
    public class Localize : ILocalize
    {
        static CultureInfo _currentCulture;

        public CultureInfo GetLocale()
        {
            return _currentCulture;
        }

        public void SetLocale(CultureInfo cultureInfo)
        {
            _currentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }

        public CultureInfo GetDeviceCultureInfo()
        {
            var netLanguage = "en";
            var prefLanguageOnly = "en";

            var androidLocale = Java.Util.Locale.Default;
            var pref = androidLocale.ToString();
            if (!String.IsNullOrEmpty(pref))
            {
                prefLanguageOnly = pref.Substring(0, 2);
                netLanguage = pref.Replace("_", "-");
            }

            CultureInfo cultureInfo;
            try
            {
                cultureInfo = new CultureInfo(netLanguage);
            }
            catch
            {
                // Fallback to first two characters, e.g. "en"
                cultureInfo = new CultureInfo(prefLanguageOnly);
            }

            return cultureInfo;
        }
    }
}
