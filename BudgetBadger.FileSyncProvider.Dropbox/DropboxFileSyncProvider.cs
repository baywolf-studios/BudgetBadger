using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Core.Sync;
using BudgetBadger.Models;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace BudgetBadger.FileSyncProvider.Dropbox
{
    public class DropboxFileSyncProvider : IFileSyncProvider
    {
        public DropboxFileSyncProvider()
        {
        }

        public async Task<Result> PullFilesTo(IDirectoryInfo destinationDirectory)
        {
            var result = new Result();

            try
            {
                using (var dbx = new DropboxClient("accesskey"))
                {
                    var folderArgs = new ListFolderArg("", recursive:true);

                    var folderList = await dbx.Files.ListFolderAsync(folderArgs);

                    foreach (var file in folderList.Entries.Where(i => i.IsFile))
                    {
                        var downloadArg = new DownloadArg("/" + file.Name);
                        using (var dropboxResponse = await dbx.Files.DownloadAsync(downloadArg))
                        using (var fileStream = await dropboxResponse.GetContentAsStreamAsync())
                        using (var destinationFile = destinationDirectory.CreateFile(file.Name))
                        {
                            await fileStream.CopyToAsync(destinationFile);
                        }
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
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
                    using (var dbx = new DropboxClient("accesskey"))
                    {
                        var commitInfo = new CommitInfo("/" + file.Name, mode: WriteMode.Overwrite.Instance);
                        var dropBoxResponse = await dbx.Files.UploadAsync(commitInfo, fileStream);
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
