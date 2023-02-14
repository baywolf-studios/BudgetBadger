using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BudgetBadger.Core.Localization
{
    public interface IResourceContainer
    {
        string GetResourceString(string key);
        string GetFormattedString(string format, object obj);
        string GetFormattedString(string format, params object[] objs);
        decimal? GetRoundedDecimal(decimal? amount);
        decimal GetRoundedDecimal(decimal amount);
    }
}
