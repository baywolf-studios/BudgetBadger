using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Sync;
using BudgetBadger.Models;

namespace BudgetBadger.Forms
{
    public interface ISyncFactory
    {
        ISync GetSyncService();

        Task SetLastSyncDateTime(DateTime dateTime);
        string GetLastSyncDateTime();

        Task<Result> EnableDropboxCloudSync();
        Task DisableDropboxCloudSync();
    }
}
