using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BudgetBadger.Core.LocalizedResources
{
    public interface IResourceContainer
    {
        string GetResourceString(string key);
        string GetFormattedString(string format, object obj);
    }
}
