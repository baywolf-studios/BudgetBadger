using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public interface IFileSyncProvider
    {
        Task<Result> DownloadSyncFolder();
        Task<Result> UploadSyncFolder();
        //Task<Result> CleanSyncFolder();
    }
}
