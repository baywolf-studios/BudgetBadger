using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.CloudSync;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.FileSystem;
using BudgetBadger.Core.Settings;
using BudgetBadger.DataAccess.Sqlite;
using BudgetBadger.FileSystem.Dropbox;
using BudgetBadger.FileSystem.WebDav;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Forms.CloudSync
{
    public class FileCloudSync : ICloudSync
    {
        private const string DefaultCloudBudgetFileName = "/default.bb";
        private readonly IDataAccess _appDataAccess;
        private readonly KeyValuePair<string, IFileSystem>[] _fileSystems;
        private readonly IFileSystem _localFileSystem;
        private readonly ISettings _settings;
        private readonly ISyncEngine _syncEngine;
        private readonly ITempSqliteDataAccessFactory _tempSqliteDataAccessFactory;

        public FileCloudSync(ISettings settings,
            ISyncEngine syncEngine,
            IDataAccess appDataAccess,
            ITempSqliteDataAccessFactory tempSqliteDataAccessFactory,
            KeyValuePair<string, IFileSystem>[] fileSystems)
        {
            _settings = settings;
            _syncEngine = syncEngine;
            _appDataAccess = appDataAccess;
            _tempSqliteDataAccessFactory = tempSqliteDataAccessFactory;
            _fileSystems = fileSystems;
            _localFileSystem = fileSystems.FirstOrDefault(f => f.Key == Enums.FileSystem.Local).Value;
        }

        public async Task<Result> Sync()
        {
            var syncMode = await _settings.GetValueOrDefaultAsync(AppSettings.SyncMode);
            Result result;
            switch (syncMode)
            {
                case SyncMode.Dropbox:
                    var refreshToken = await _settings.GetValueOrDefaultAsync(DropboxSettings.RefreshToken);
                    var dropBoxFileSystem = _fileSystems.FirstOrDefault(f => f.Key == Enums.FileSystem.Dropbox).Value;
                    dropBoxFileSystem.SetAuthentication(new Dictionary<string, string>
                    {
                        { DropboxSettings.RefreshToken, refreshToken },
                        { DropboxSettings.AppKey, AppSecrets.DropBoxAppKey }
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

                    try
                    {
                        await _localFileSystem.File.DeleteAsync(temp.path);
                    }
                    catch (Exception)
                    {
                        // it's a temp file, ignore
                    }
                    break;
                case SyncMode.WebDav:
                    var webDavServer = await _settings.GetValueOrDefaultAsync(WebDavSettings.Server);
                    var webDavAcceptInvalidCertificate = await _settings.GetValueOrDefaultAsync(WebDavSettings.AcceptInvalidCertificate);
                    var webDavDirectory = await _settings.GetValueOrDefaultAsync(WebDavSettings.Directory);
                    var webDavUsername = await _settings.GetValueOrDefaultAsync(WebDavSettings.Username);
                    var webDavPassword = await _settings.GetValueOrDefaultAsync(WebDavSettings.Password);
                    var webDavFileSystem = _fileSystems.FirstOrDefault(f => f.Key == Enums.FileSystem.WebDav).Value;
                    webDavFileSystem.SetAuthentication(new Dictionary<string, string>
                    {
                        { WebDavSettings.Server, webDavServer },
                        { WebDavSettings.AcceptInvalidCertificate, webDavAcceptInvalidCertificate },
                        { WebDavSettings.Directory, webDavDirectory },
                        { WebDavSettings.Username, webDavUsername },
                        { WebDavSettings.Password, webDavPassword }
                    });

                    var temp2 = _tempSqliteDataAccessFactory.Create();

                    result = await _syncEngine.FileBasedSyncAsync(true,
                        webDavFileSystem,
                        DefaultCloudBudgetFileName,
                        _localFileSystem,
                        temp2.path,
                        temp2.sqliteDataAccess,
                        _appDataAccess);
                    if (result.Success)
                    {
                        await SetLastSyncDateTime(DateTime.Now);
                    }

                    try
                    {
                        await _localFileSystem.File.DeleteAsync(temp2.path);
                    }
                    catch (Exception)
                    {
                        // it's a temp file, ignore
                    }
                    break;
                default:
                    result = Result.Ok();
                    break;
            }

            return result;
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

        public async Task DisableCloudSync()
        {
            var syncMode = await _settings.GetValueOrDefaultAsync(AppSettings.SyncMode);
            await _settings.RemoveAsync(AppSettings.SyncMode);
            await _settings.RemoveAsync(AppSettings.LastSyncDateTime);

            switch (syncMode)
            {
                case SyncMode.Dropbox:
                    await _settings.RemoveAsync(DropboxSettings.RefreshToken);
                    break;
            }
        }

        private async Task SetLastSyncDateTime(DateTime dateTime)
        {
            await _settings.AddOrUpdateValueAsync(AppSettings.LastSyncDateTime, dateTime.ToString());
        }
    }
}
