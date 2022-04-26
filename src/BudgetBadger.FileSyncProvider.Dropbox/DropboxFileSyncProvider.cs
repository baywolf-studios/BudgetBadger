using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.Models;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace BudgetBadger.FileSyncProvider.Dropbox
{
    public class DropboxFileSyncProvider : IFileSyncProvider
    {
        readonly ISettings _settings;
        string _refreshToken { get => _settings.GetValueOrDefault(DropboxSettings.RefreshToken); }
        readonly string _appKey;

        public DropboxFileSyncProvider(ISettings settings, string appKey)
        {
            _settings = settings;
            _appKey = appKey;
        }

        static Stream Compress(Stream decompressed)
        {
            var compressed = new MemoryStream();
            using (var zip = new GZipStream(compressed, CompressionLevel.Fastest, true))
            {
                decompressed.CopyTo(zip);
            }

            compressed.Seek(0, SeekOrigin.Begin);
            return compressed;
        }

        static Stream Decompress(Stream compressed)
        {
            var decompressed = new MemoryStream();
            using (var zip = new GZipStream(compressed, CompressionMode.Decompress, true))
            {
                zip.CopyTo(decompressed);
            }

            decompressed.Seek(0, SeekOrigin.Begin);
            return decompressed;
        }

        public async Task<Result> PullFilesTo(IDirectoryInfo destinationDirectory)
        {
            var result = new Result();

            try
            {
                using (var dbx = new DropboxClient(_refreshToken, _appKey))
                {
                    var folderArgs = new ListFolderArg("", recursive: true);

                    var folderList = await dbx.Files.ListFolderAsync(folderArgs);

                    foreach (var file in folderList.Entries.Where(i => i.IsFile))
                    {
                        var fileName = file.Name;

                        var compressed = file.Name.EndsWith(".gz");
                        if (compressed)
                        {
                            fileName = fileName.Substring(0, fileName.LastIndexOf(".gz"));
                        }

                        var downloadArg = new DownloadArg("/" + file.Name);
                        using (var dropboxResponse = await dbx.Files.DownloadAsync(downloadArg))
                        using (var fileStream = await dropboxResponse.GetContentAsStreamAsync())
                        using (var destinationFile = destinationDirectory.CreateFile(fileName))
                        {
                            if (compressed)
                            {
                                using (var uncompressedFilestream = Decompress(fileStream))
                                {
                                    await uncompressedFilestream.CopyToAsync(destinationFile);
                                }
                            }
                            else
                            {
                                await fileStream.CopyToAsync(destinationFile);
                            }
                        }
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> PushFilesFrom(IDirectoryInfo sourceDirectory)
        {
            var result = new Result();

            try
            {
                var files = sourceDirectory.GetFiles();

                foreach (var file in files)
                {
                    using (var fileStream = file.Open())
                    using (var compressedFileStream = Compress(fileStream))    
                    using (var dbx = new DropboxClient(_refreshToken, _appKey))
                    {
                        if (file.Name.EndsWith(".gz"))
                        {
                            var commitInfo = new CommitInfo("/" + file.Name, mode: WriteMode.Overwrite.Instance);
                            var dropBoxResponse = await dbx.Files.UploadAsync(commitInfo, fileStream);
                        }
                        else
                        {
                            var commitInfo = new CommitInfo("/" + file.Name + ".gz", mode: WriteMode.Overwrite.Instance);
                            var dropBoxResponse = await dbx.Files.UploadAsync(commitInfo, compressedFileStream);
                        }
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
