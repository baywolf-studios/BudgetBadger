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
            if (key == null)
            {
                return "";
            }

            Language.AppResources.Culture = _localize.GetLocale();
            var translation = Language.AppResources.ResourceManager.GetString(key);

            if (translation == null)
            {

#if DEBUG
                throw new ArgumentException(
                    String.Format("Key '{0}' was not found in resources.", key),
                    nameof(key));
#else
                translation = key; // returns the key, which GETS DISPLAYED TO THE USER
#endif
            }

            return translation;
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
