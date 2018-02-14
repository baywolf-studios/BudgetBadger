using System;
using System.IO;
using System.Threading.Tasks;
using BudgetBadger.Core.Files;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public class LocalFileSyncProvider : IFileSyncProvider
    {
        readonly DirectoryInfo LocalDirectoryInfo;

        public LocalFileSyncProvider(string localDirectoryPath)
        {
            LocalDirectoryInfo = new DirectoryInfo(localDirectoryPath);
        }

        public async Task<Result> PushFilesFrom(IDirectoryInfo sourceDirectory)
        {
            var result = new Result();

            try
            {
                foreach(var file in sourceDirectory.GetFiles())
                {
                    string outputPath = Path.Combine(LocalDirectoryInfo.FullName, file.Name);
                    
                    using (var sourceStream = file.Open())
                    using (var destinationStream = File.Create(outputPath))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
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

        public async Task<Result> PullFilesTo(IDirectoryInfo destinationDirectory)
        {
            var result = new Result();

            try
            {
                foreach (var file in LocalDirectoryInfo.GetFiles())
                {
                    using (var sourceFile = File.Open(file.FullName, FileMode.Open))
                    using (var destinationFile = destinationDirectory.CreateFile(file.Name))
                    {
                        await sourceFile.CopyToAsync(destinationFile);
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
