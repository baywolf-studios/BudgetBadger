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

        public Task<string> GetValueOrDefaultAsync(string key)
        {
            if (AppStore.Properties.ContainsKey(key))
            {
                return Task.FromResult(AppStore.Properties[key].ToString());
            }
            return Task.FromResult(string.Empty);
        }

        public async Task RemoveAllAsync()
        {
            AppStore.Properties.Clear();

            await AppStore.SavePropertiesAsync();
        }

        public async Task RemoveAsync(string key)
        {
            if (AppStore.Properties.ContainsKey(key))
            {
                AppStore.Properties.Remove(key);

                await AppStore.SavePropertiesAsync();
            }
        }
    }
}
