using System;
using System.IO;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public class LocalFileSyncProvider : IFileSyncProvider
    {
        readonly string LocalDirectory;
        readonly string SyncDirectory;

        public LocalFileSyncProvider(string localDirectory, string syncDirectory)
        {
            LocalDirectory = localDirectory;
            SyncDirectory = syncDirectory;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public async Task<Result> DownloadSyncFolder()
        {
            var result = new Result();

            try
            {
                await Task.Run(() => DirectoryCopy(LocalDirectory, SyncDirectory, true));
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> UploadSyncFolder()
        {
            var result = new Result();

            try
            {
                await Task.Run(() => DirectoryCopy(SyncDirectory, LocalDirectory, true));
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
