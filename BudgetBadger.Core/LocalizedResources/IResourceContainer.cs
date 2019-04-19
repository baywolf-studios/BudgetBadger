using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBadger.Core.LocalizedResources
{
    public interface IResourceContainer
    {
        string GetString(string key);
    }
}
