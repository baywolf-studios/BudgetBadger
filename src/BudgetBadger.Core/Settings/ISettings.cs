using System;
using System.Threading.Tasks;

namespace BudgetBadger.Core.Settings
{
    public interface ISettings
    {
        string GetValueOrDefault(string key);
        Task AddOrUpdateValueAsync(string key, string value);
    }
}
