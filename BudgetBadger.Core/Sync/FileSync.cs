using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public class FileSync : ISync
    {
        readonly IFileSyncProvider FileProvider;
        readonly ISyncLogic SyncLogic;

        public FileSync(IFileSyncProvider fileProvider, ISyncLogic syncLogic)
        {
            FileProvider = fileProvider;
            SyncLogic = syncLogic;
        }

        public async Task<Result> Sync()
        {
            var result = await FileProvider.DownloadSyncFolder();

            if (result.Success)
            {
                result = await SyncLogic.SyncToRemote();
            }

            if (result.Success)
            {
                result = await FileProvider.UploadSyncFolder();
            }

            // copy to the local folder?

            return result;
        }

        public Task<Result> SyncDown()
        {
            throw new NotImplementedException();
        }

        public Task<Result> SyncUp()
        {
            throw new NotImplementedException();
        }
    }
}
