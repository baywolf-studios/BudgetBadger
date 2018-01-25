using System;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public class FileSync : ISync
    {
        readonly IFileSyncProvider FileProvider;

        public FileSync(IFileSyncProvider fileProvider)
        {
            FileProvider = fileProvider;
        }

        public Result Sync()
        {
            FileProvider.DownloadSyncFolder();

            // do some syncing

            FileProvider.UploadSyncFolder();

            return new Result();
        }

        public Result SyncDown()
        {
            throw new NotImplementedException();
        }

        public Result SyncUp()
        {
            throw new NotImplementedException();
        }
    }
}
