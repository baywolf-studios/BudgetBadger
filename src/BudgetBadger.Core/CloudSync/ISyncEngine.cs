using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.FileSystem;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.CloudSync
{
    public interface ISyncEngine
    {
        Task<Result> ImportAsync(IDataAccess importDataAccess, IDataAccess appDataAccess);
        Task<Result> ExportAsync(IDataAccess appDataAccess, IDataAccess exportDataAccess);

        Task<Result> FileBasedImportAsync(IFileSystem importFileSystem,
            string importFile,
            bool compression,
            IFileSystem tempFileSystem,
            string tempFile,
            IDataAccess tempDataAccess,
            IDataAccess appDataAccess);

        Task<Result> FileBasedExportAsync(IDataAccess appDataAccess,
            IDataAccess tempDataAccess,
            IFileSystem tempFileSystem,
            string tempFile,
            bool compression,
            string exportFile,
            IFileSystem exportFileSystem);

        Task<Result> SyncAsync(IDataAccess appDataAccess, IDataAccess syncDataAccess);

        Task<Result> FileBasedSyncAsync(bool compression,
            IFileSystem syncFileSystem,
            string syncFile,
            IFileSystem tempFileSystem,
            string tempFile,
            IDataAccess tempDataAccess,
            IDataAccess appDataAccess);
    }
}
