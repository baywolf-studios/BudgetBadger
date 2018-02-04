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
                result = await SyncLogic.Sync();
            }

            if (result.Success)
            {
                result = await FileProvider.UploadSyncFolder();
            }

            return result;
        }

        public async Task<Result> Pull()
        {
            var result = await FileProvider.DownloadSyncFolder();

            if (result.Success)
            {
                result = await SyncLogic.SyncToLocal();
            }

            return result;
        }

        public async Task<Result> Push()
        {
            //clear sync folder

            var result = await SyncLogic.SyncToRemote();

            if (result.Success)
            {
                result = await FileProvider.UploadSyncFolder();
            }

            return result;
        }
    }
}
