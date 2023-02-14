using System;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.FileSystem;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.CloudSync
{
    public class SyncEngine : ISyncEngine
    {
        private const string _compressExt = ".gz";
        private static readonly SemaphoreSlim FileBasedSyncLock = new SemaphoreSlim(1, 1);
        private readonly IMergeLogic _mergeLogic;

        public SyncEngine(IMergeLogic mergeLogic)
        {
            _mergeLogic = mergeLogic;
        }

        public SyncEngine() : this(new MergeLogic())
        {
        }

        /// <summary>
        ///     Merges all the data from the importDataAccess into the appDataAccess.
        ///     Will never delete data
        /// </summary>
        /// <param name="importDataAccess">The DataAccess going to be imported</param>
        /// <param name="appDataAccess">The DataAccess that will get imported to</param>
        /// <returns></returns>
        public async Task<Result> ImportAsync(IDataAccess importDataAccess, IDataAccess appDataAccess)
        {
            var result = new Result();

            try
            {
                await importDataAccess.Init();
                await appDataAccess.Init();

                await _mergeLogic.MergeAllAsync(importDataAccess, appDataAccess);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        /// <summary>
        ///     Merges all data from the appDataAccess into the exportDataAccess.
        ///     Will never delete data.
        /// </summary>
        /// <param name="appDataAccess"></param>
        /// <param name="exportDataAccess"></param>
        /// <returns></returns>
        public async Task<Result> ExportAsync(IDataAccess appDataAccess, IDataAccess exportDataAccess)
        {
            Result result;

            try
            {
                await appDataAccess.Init();
                await exportDataAccess.Init();

                await _mergeLogic.MergeAllAsync(appDataAccess, exportDataAccess);

                result = Result.Ok();
            }
            catch (Exception ex)
            {
                result = Result.Fail(ex.Message);
            }

            return result;
        }

        /// <summary>
        ///     Reads the importFile from the importFileSystem
        ///     Decompresses importFile if needed
        ///     Copies the importFile from the importFileSystem to the tempFile on the tempFileSystem.
        ///     Merges all data from the tempDataAccess into the appDataAccess.
        ///     Will not delete data.
        /// </summary>
        /// <param name="importFileSystem">IFileSystem where the importFile exists</param>
        /// <param name="importFile">The file that has the data to import</param>
        /// <param name="compression">Determines if import file needs decompressed</param>
        /// <param name="tempFileSystem">IFileSystem where tempFile will be saved to</param>
        /// <param name="tempFile">The file that tempDataAccess uses</param>
        /// <param name="tempDataAccess">IDataAccess backed by tempFile</param>
        /// <param name="appDataAccess">IDataAccess that is imported to</param>
        /// <returns></returns>
        public async Task<Result> FileBasedImportAsync(IFileSystem importFileSystem,
            string importFile,
            bool compression,
            IFileSystem tempFileSystem,
            string tempFile,
            IDataAccess tempDataAccess,
            IDataAccess appDataAccess)
        {
            Result result;

            try
            {
                if (compression)
                {
                    importFile += _compressExt;
                }

                if (await importFileSystem.File.ExistsAsync(importFile))
                {
                    var importFileBytes = await importFileSystem.File.ReadAllBytesAsync(importFile);
                    if (compression)
                    {
                        importFileBytes = importFileBytes.Decompress();
                    }

                    await tempFileSystem.File.WriteAllBytesAsync(tempFile, importFileBytes);

                    result = await ImportAsync(tempDataAccess, appDataAccess);
                }
                else
                {
                    result = Result.Fail($"{nameof(importFile)} does not exist");
                }
            }
            catch (Exception ex)
            {
                result = Result.Fail(ex.Message);
            }

            return result;
        }

        /// <summary>
        ///     Merges all data from the appDataAccess into the tempDataAccess.
        ///     Reads the tempFile from the tempFileSystem
        ///     Compresses tempFile if needed
        ///     Copies the tempFile from the tempFileSystem to the exportFile on the exportFileSystem.
        ///     Will not delete data.
        /// </summary>
        /// <param name="appDataAccess"></param>
        /// <param name="tempDataAccess"></param>
        /// <param name="tempFileSystem"></param>
        /// <param name="tempFile"></param>
        /// <param name="compression"></param>
        /// <param name="exportFileSystem"></param>
        /// <param name="exportFile"></param>
        /// <returns></returns>
        public async Task<Result> FileBasedExportAsync(IDataAccess appDataAccess,
            IDataAccess tempDataAccess,
            IFileSystem tempFileSystem,
            string tempFile,
            bool compression,
            string exportFile,
            IFileSystem exportFileSystem)
        {
            Result result;

            try
            {
                var exportResult = await ExportAsync(appDataAccess, tempDataAccess);

                if (exportResult.Success)
                {
                    if (await tempFileSystem.File.ExistsAsync(tempFile))
                    {
                        var tempFileBytes = await tempFileSystem.File.ReadAllBytesAsync(tempFile);
                        if (compression)
                        {
                            tempFileBytes = tempFileBytes.Compress();
                            exportFile += _compressExt;
                        }

                        await exportFileSystem.File.WriteAllBytesAsync(exportFile, tempFileBytes);

                        result = Result.Ok();
                    }
                    else
                    {
                        result = Result.Fail($"{nameof(tempFile)} does not exist");
                    }
                }
                else
                {
                    result = exportResult;
                }
            }
            catch (Exception ex)
            {
                result = Result.Fail(ex.Message);
            }

            return result;
        }

        public async Task<Result> SyncAsync(IDataAccess appDataAccess, IDataAccess syncDataAccess)
        {
            var importResult = await ImportAsync(syncDataAccess, appDataAccess);

            if (importResult.Success)
            {
                return await ExportAsync(appDataAccess, syncDataAccess);
            }

            return importResult;
        }

        public async Task<Result> FileBasedSyncAsync(bool compression,
            IFileSystem syncFileSystem,
            string syncFile,
            IFileSystem tempFileSystem,
            string tempFile,
            IDataAccess tempDataAccess,
            IDataAccess appDataAccess)
        {
            using (await FileBasedSyncLock.UseWaitAsync())
            {
                Result result;
                var syncFileNameToCheck = syncFile;
                if (compression)
                {
                    syncFileNameToCheck += _compressExt;
                }

                if (await syncFileSystem.File.ExistsAsync(syncFileNameToCheck))
                {
                    result = await FileBasedImportAsync(syncFileSystem,
                        syncFile,
                        compression,
                        tempFileSystem,
                        tempFile,
                        tempDataAccess,
                        appDataAccess);
                }
                else
                {
                    result = Result.Ok();
                }

                if (result.Success)
                {
                    result = await FileBasedExportAsync(appDataAccess,
                        tempDataAccess,
                        tempFileSystem,
                        tempFile,
                        compression,
                        syncFile,
                        syncFileSystem);
                }

                return result;
            }
        }
    }
}
