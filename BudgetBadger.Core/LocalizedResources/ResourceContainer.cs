using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBadger.Core.LocalizedResources
{
    public class ResourceContainer : IResourceContainer
    {
        readonly ILocalize _localize;

        public ResourceContainer(ILocalize localize)
        {
            _localize = localize;
        }

        public string GetString(string key)
        {
            Language.AppResources.Culture = _localize.CurrentCultureInfo;
            return Language.AppResources.ResourceManager.GetString(key);
        }
    }
}
