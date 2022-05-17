using System;
using System.Threading.Tasks;

namespace BudgetBadger.Core.Settings
{
    public interface ISettings
    {
        Task<string> GetValueOrDefaultAsync(string key);
        Task AddOrUpdateValueAsync(string key, string value);
        Task RemoveAsync(string key);
        Task RemoveAllAsync();
    }
}
