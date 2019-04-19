using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BudgetBadger.Core.LocalizedResources
{
    public interface ILocalize
    {
        CultureInfo GetDeviceCultureInfo();
        void SetLocale(CultureInfo cultureInfo);
        CultureInfo GetLocale();
    }
}
