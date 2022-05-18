using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BudgetBadger.Core.CloudSync;
using BudgetBadger.Core.Utilities;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace BudgetBadger.FileSystem.Dropbox
{
    public class DropboxFile : IFile
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

        public async Task CopyAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    if (overwrite)
                    {
                        var exists = await ExistsAsync(destFileName);
                        if (exists)
                        {
                            await DeleteAsync(destFileName);
                        }
                    }

                    await dbx.Files.CopyV2Async(sourceFileName, destFileName);
                }
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public async Task DeleteAsync(string path)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    await dbx.Files.DeleteV2Async(path);
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
                    return dropboxResponse.IsFile && !dropboxResponse.IsDeleted;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task MoveAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    if (overwrite)
                    {
                        var exists = await ExistsAsync(destFileName);
                        if (exists)
                        {
                            await DeleteAsync(destFileName);
                        }
                    }

                    await dbx.Files.MoveV2Async(sourceFileName, destFileName);
                }
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            try
            {
                using (var dbx = GetDropboxClient())
                {
                    var downloadArg = new DownloadArg(path);
                    using (var dropboxResponse = await dbx.Files.DownloadAsync(downloadArg))
                    {
                        return await dropboxResponse.GetContentAsByteArrayAsync();
                    }
                }
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public async Task WriteAllBytesAsync(string path, byte[] data)
        {
            try
            {
                using (var fileStream = new MemoryStream(data))
                using (var dbx = GetDropboxClient())
                {
                    var commitInfo = new CommitInfo(path, WriteMode.Overwrite.Instance);
                    await dbx.Files.UploadAsync(commitInfo, fileStream);
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