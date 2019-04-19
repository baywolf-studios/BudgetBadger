using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BudgetBadger.Core.LocalizedResources
{
    public interface ILocalize
    {
        CultureInfo CurrentCultureInfo { get; set; }
        CultureInfo DeviceCultureInfo { get; }
    }
}
