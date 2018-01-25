using System;
namespace BudgetBadger.Core.Sync
{
    public interface IFileSyncProvider
    {
        bool DownloadSyncFolder();
        bool UploadSyncFolder();
    }
}
