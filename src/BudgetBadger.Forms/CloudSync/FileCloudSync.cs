using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.CloudSync;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Settings;
using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.FileSystem.Dropbox;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;

namespace BudgetBadger.Forms.CloudSync
{
    public class FileCloudSync : ICloudSync
    {
        private readonly ISettings _settings;
        private readonly ISyncEngine _syncEngine;
        private readonly IDataAccess _appDataAccess;
        private readonly ITempSqliteDataAccessFactory _tempSqliteDataAccessFactory;
        private readonly IFileSystem _localFileSystem;
        private readonly KeyValuePair<string, IFileSystem>[] _fileSystems;
        private readonly KeyValuePair<string, IFileSystemAuthentication>[] _fileSystemAuthenticators;
        private const string DefaultCloudBudgetFileName = "/default.bb";

        public FileCloudSync(ISettings settings,
            ISyncEngine syncEngine,
            IDataAccess appDataAccess,
            ITempSqliteDataAccessFactory tempSqliteDataAccessFactory,
            KeyValuePair<string, IFileSystem>[] fileSystems,
            KeyValuePair<string, IFileSystemAuthentication>[] fileSystemAuthenticators)
        {
            _settings = settings;
            _syncEngine = syncEngine;
            _appDataAccess = appDataAccess;
            _tempSqliteDataAccessFactory = tempSqliteDataAccessFactory;
            _fileSystems = fileSystems;
            _fileSystemAuthenticators = fileSystemAuthenticators;
            _localFileSystem = fileSystems.FirstOrDefault(f => f.Key == Enums.FileSystem.Local).Value;
        }
        
        public async Task<Result> Sync()
        {
            var syncMode = await _settings.GetValueOrDefaultAsync(AppSettings.SyncMode);
            Result result;
            switch (syncMode)
            {
                case SyncMode.Dropbox:
                    var refreshToken =  await _settings.GetValueOrDefaultAsync(DropboxSettings.RefreshToken);
                    var dropBoxFileSystem = _fileSystems.FirstOrDefault(f => f.Key == Enums.FileSystem.Dropbox).Value;
                    dropBoxFileSystem.SetAuthentication(new Dictionary<string, string>
                    {
                        {DropboxSettings.RefreshToken, refreshToken},
                        {DropboxSettings.AppKey, AppSecrets.DropBoxAppKey}
                    });

                    var temp = _tempSqliteDataAccessFactory.Create();

                    result = await _syncEngine.FileBasedSyncAsync(true,
                        dropBoxFileSystem,
                        DefaultCloudBudgetFileName,
                        _localFileSystem,
                        temp.path,
                        temp.sqliteDataAccess,
                        _appDataAccess);
                    if (result.Success)
                    {
                        await SetLastSyncDateTime(DateTime.Now);
                    }
                    await _localFileSystem.File.DeleteAsync(temp.path);
                    break;
                default:
                    result = Result.Ok();
                    break;
            }
            
            return result;
        }

        private async Task SetLastSyncDateTime(DateTime dateTime)
        {
            await _settings.AddOrUpdateValueAsync(AppSettings.LastSyncDateTime, dateTime.ToString());
        }

        public async Task<DateTime?> GetLastSyncDateTimeAsync()
        {
            var lastSyncString = await _settings.GetValueOrDefaultAsync(AppSettings.LastSyncDateTime);
            if (DateTime.TryParse(lastSyncString, out var dateTime))
            {
                return dateTime;
            }

            return null;
        }

        public async Task<Result> EnableCloudSync(string syncMode)
        {
            switch (syncMode)
            {
                case SyncMode.Dropbox:
                    return await EnableDropboxCloudSync();
                default:
                    return Result.Ok();
            }
        }

        public async Task DisableCloudSync()
        {
            await _settings.RemoveAsync(AppSettings.SyncMode);
            await _settings.RemoveAsync(AppSettings.LastSyncDateTime);
        }
        
        private async Task<Result> EnableDropboxCloudSync()
        {
            Result result;

            try
            {
                var dropboxAuthentication = _fileSystemAuthenticators.FirstOrDefault(f => f.Key == Enums.FileSystem.Dropbox).Value;
                var dropboxResult = await dropboxAuthentication.Authenticate(
                    new Dictionary<string, string> { { DropboxSettings.AppKey, AppSecrets.DropBoxAppKey } });

                if (dropboxResult.Success)
                {
                    await _settings.AddOrUpdateValueAsync(AppSettings.SyncMode, SyncMode.Dropbox);
                    foreach (var item in dropboxResult.Data)
                    {
                        await _settings.AddOrUpdateValueAsync(item.Key, item.Value);
                    }
                    result = Result.Ok();
                }
                else
                {
                    result = dropboxResult;
                }
            }
            catch (Exception ex)
            {
                result = Result.Fail(ex.Message);
            }

            return result;
        }
    }
}
