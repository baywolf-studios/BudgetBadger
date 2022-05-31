using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.FileSystem;
using Dropbox.Api;

namespace BudgetBadger.FileSystem.Dropbox
{
    public class DropboxDirectory : IDirectory
    {
        private string _appKey;
        private string _appSecret;
        private string _refreshToken;

        public void SetAuthentication(IReadOnlyDictionary<string, string> keys)
        {
            keys.TryGetValue(DropboxSettings.RefreshToken, out _refreshToken);
            keys.TryGetValue(DropboxSettings.AppKey, out _appKey);
            keys.TryGetValue(DropboxSettings.AppSecret, out _appSecret);
        }

        public async Task CreateDirectoryAsync(string path)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    await dbx.Files.CreateFolderV2Async(path);
                }
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public async Task<bool> ExistsAsync(string path)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    var dropboxResponse = await dbx.Files.GetMetadataAsync(path);
                    return dropboxResponse.IsFolder && !dropboxResponse.IsDeleted;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IReadOnlyList<string>> GetFilesAsync(string path)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    var result = new List<string>();
                    var contents = await dbx.Files.ListFolderAsync(path);
                    result.AddRange(contents.Entries.Where(e => e.IsFile && !e.IsDeleted)
                        .Select(e2 => e2.AsFile.PathDisplay));
                    var hasMore = contents.HasMore;
                    var cursor = contents.Cursor;
                    while (hasMore)
                    {
                        var contents2 = await dbx.Files.ListFolderContinueAsync(cursor);
                        result.AddRange(contents2.Entries.Where(e => e.IsFile && !e.IsDeleted)
                            .Select(e2 => e2.AsFile.PathDisplay));
                        hasMore = contents2.HasMore;
                        cursor = contents2.Cursor;
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public async Task<IReadOnlyList<string>> GetDirectoriesAsync(string path)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    var result = new List<string>();
                    var contents = await dbx.Files.ListFolderAsync(path);
                    result.AddRange(contents.Entries.Where(e => e.IsFolder && !e.IsDeleted)
                        .Select(e2 => e2.AsFolder.PathDisplay));
                    var hasMore = contents.HasMore;
                    var cursor = contents.Cursor;
                    while (hasMore)
                    {
                        var contents2 = await dbx.Files.ListFolderContinueAsync(cursor);
                        result.AddRange(contents2.Entries.Where(e => e.IsFolder && !e.IsDeleted)
                            .Select(e2 => e2.AsFolder.PathDisplay));
                        hasMore = contents2.HasMore;
                        cursor = contents2.Cursor;
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public async Task DeleteAsync(string path, bool recursive = false)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    if (!recursive)
                    {
                        var contents = await dbx.Files.ListFolderAsync(path);
                        if (contents.Entries.Any())
                        {
                            throw new IOException("Non-empty directory");
                        }
                    }

                    await dbx.Files.DeleteV2Async(path);
                }
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public async Task MoveAsync(string sourceDirName, string destDirName)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    await dbx.Files.MoveV2Async(sourceDirName, destDirName);
                }
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        private DropboxClient GetDropboxClient()
        {
            if (string.IsNullOrEmpty(_refreshToken))
            {
                throw new UnauthorizedAccessException();
            }

            var dropBoxConfig = new DropboxClientConfig
            {
                HttpClient = DropboxNetwork.StaticHttpClient,
                LongPollHttpClient = DropboxNetwork.StaticLongPollHttpClient
            };
            return string.IsNullOrEmpty(_appSecret)
                ? new DropboxClient(_refreshToken, _appKey, dropBoxConfig)
                : new DropboxClient(_refreshToken, _appKey, _appSecret, dropBoxConfig);
        }
    }
}
