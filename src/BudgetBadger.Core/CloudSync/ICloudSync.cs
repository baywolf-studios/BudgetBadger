using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.CloudSync
{
    public interface ICloudSync
    {
        Task<Result> Sync();

        Task<DateTime?> GetLastSyncDateTimeAsync();

        Task<Result> EnableCloudSync(string syncMode);

        Task DisableCloudSync();
    }
}