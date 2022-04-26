using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Settings;
using Prism.AppModel;

namespace BudgetBadger.Forms.Settings
{
    public class AppStoreSettings : ISettings
    {
        readonly IApplicationStore AppStore;

        public AppStoreSettings(IApplicationStore appStore)
        {
            AppStore = appStore;
        }

        public async Task AddOrUpdateValueAsync(string key, string value)
        {
            if (AppStore.Properties.ContainsKey(key))
            {
                AppStore.Properties[key] = value;
            }
            else
            {
                AppStore.Properties.Add(key, value);
            }

            await AppStore.SavePropertiesAsync();
        }

        public string GetValueOrDefault(string key)
        {
            if (AppStore.Properties.ContainsKey(key))
            {
                return AppStore.Properties[key].ToString();
            }
            return string.Empty;
        }
    }
}
