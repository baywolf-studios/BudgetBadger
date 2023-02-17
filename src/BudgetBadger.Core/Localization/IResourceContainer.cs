using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BudgetBadger.Core.Localization
{
    public interface IResourceContainer
    {
        /// <summary>
        ///  Gets the localized string for the given key.
        ///  If the translated string does not exist, the key will be returned.
        /// </summary>
        string GetResourceString(string key);
        string GetFormattedString(string format, object obj);
        string GetFormattedString(string format, params object[] objs);
        decimal? GetRoundedDecimal(decimal? amount);
        decimal GetRoundedDecimal(decimal amount);
    }
}
