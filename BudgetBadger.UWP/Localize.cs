using BudgetBadger.Core.LocalizedResources;
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
        CultureInfo _currentCultureInfo = null;

        public CultureInfo CurrentCultureInfo
        {
            get
            {
                if (_currentCultureInfo == null)
                {
                    _currentCultureInfo = CultureInfo.CurrentUICulture;
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = _currentCultureInfo.Name;
                    Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
                    Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
                }
                return _currentCultureInfo;
            }
            set
            {
                _currentCultureInfo = value;
                CultureInfo.CurrentCulture = value;
                CultureInfo.CurrentUICulture = value;
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = value.Name;
                Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
                Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
            }
        }

        public CultureInfo DeviceCultureInfo { get { return CultureInfo.CurrentUICulture; } }
    }
}
