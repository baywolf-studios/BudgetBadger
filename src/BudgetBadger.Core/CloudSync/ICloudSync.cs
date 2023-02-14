using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.CloudSync
{
    public interface ICloudSync
    {
        Task<Result> Sync();

        Task<DateTime?> GetLastSyncDateTimeAsync();

        Task DisableCloudSync();
    }
}
