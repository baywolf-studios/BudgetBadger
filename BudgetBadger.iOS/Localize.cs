using System;
using System.Globalization;
using System.Threading;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.iOS;
using Foundation;
using Xamarin.Forms;

[assembly: Dependency(typeof(Localize))]
namespace BudgetBadger.iOS
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

            if (NSLocale.PreferredLanguages.Length > 0)
            {
                var pref = NSLocale.PreferredLanguages[0];
                prefLanguageOnly = pref.Substring(0, 2);

                if (prefLanguageOnly == "pt")
                {
                    if (pref == "pt")
                        pref = "pt-BR"; // Brazilian
                    else
                        pref = "pt-PT"; // Portuguese
                }

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
