using System;
using System.Globalization;
using System.Threading;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.macOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(Localize))]
namespace BudgetBadger.macOS
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
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public CultureInfo GetDeviceCultureInfo()
        {
            return CultureInfo.InstalledUICulture;
        } 
    }
}
