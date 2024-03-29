﻿using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.UWP;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(Localize))]
namespace BudgetBadger.UWP
{
    public class Localize : ILocalize
    {
        static CultureInfo _currentCulture;

        public CultureInfo GetLocale()
        {
            var test = Windows.System.UserProfile.GlobalizationPreferences.Languages[0];
            return _currentCulture;
        }

        public void SetLocale(CultureInfo cultureInfo)
        {
            _currentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = cultureInfo.Name;
            Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
            Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
        }

        public CultureInfo GetDeviceCultureInfo()
        {
            return new CultureInfo(Windows.System.UserProfile.GlobalizationPreferences.Languages[0].ToString());
        }
    }
}
