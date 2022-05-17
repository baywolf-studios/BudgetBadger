using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Sync;
using BudgetBadger.Models;

namespace BudgetBadger.Forms
{
    public interface ISyncFactory
    {
        Task<ISync> GetSyncServiceAsync();

        Task SetLastSyncDateTime(DateTime dateTime);
        Task<string> GetLastSyncDateTimeAsync();

        Task<Result> EnableDropboxCloudSync();
        Task DisableDropboxCloudSync();
    }
}
