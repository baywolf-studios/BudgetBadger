using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Settings;
using Xamarin.Essentials;

namespace BudgetBadger.Forms.Settings
{
	public class SecureStorageSettings : ISettings
	{
		public SecureStorageSettings()
		{
		}

        public async Task AddOrUpdateValueAsync(string key, string value)
        {
            try
            {
                await SecureStorage.SetAsync(key, value);
            }
            catch (Exception)
            {
            }
        }

        public async Task<string> GetValueOrDefaultAsync(string key)
        {
            string value = null;

            try
            {
                value = await SecureStorage.GetAsync(key);
            }
            catch (Exception)
            {
                // set value to null to clear out setting in case of corruption
                await SecureStorage.SetAsync(key, value);
            }

            return value;
        }

        public Task RemoveAllAsync()
        {
            SecureStorage.RemoveAll();
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            SecureStorage.Remove(key);
            return Task.CompletedTask;
        }
    }
}

