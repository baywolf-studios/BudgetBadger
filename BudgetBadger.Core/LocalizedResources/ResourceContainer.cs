using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace BudgetBadger.Core.LocalizedResources
{
    public class ResourceContainer : IResourceContainer
    {
        readonly ILocalize _localize;

        public ResourceContainer(ILocalize localize)
        {
            _localize = localize;
        }

        public string GetResourceString(string key)
        {
            Language.AppResources.Culture = _localize.GetLocale();
            return Language.AppResources.ResourceManager.GetString(key);
        }

        public string GetFormattedString(string format, object obj)
        {
            return String.Format(_localize.GetLocale(), format, obj);
        }

        public string GetFormattedString(string format, params object[] objs)
        {
            return String.Format(_localize.GetLocale(), format, objs);
        }
    }
}
