using System;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace BudgetBadger.IntegrationTests.FileSystem.Dropbox;

public static class TestDropboxFileBuilder
{
    private static readonly Random _rnd = new Random();

    public static async Task<(string Path, byte[] Data)> GetExistingFile(string rootDirectory)
    {
        using var dbx = new DropboxClient(IntegrationTestSecrets.DropBoxRefreshToken,
            IntegrationTestSecrets.DropBoxAppKey, IntegrationTestSecrets.DropBoxAppSecret);

        var existingFile = Path.Combine(rootDirectory, Path.GetRandomFileName());
        
        var bytes = new byte[10];
        _rnd.NextBytes(bytes);
        using var fileStream = new MemoryStream(bytes);
        var commitInfo = new CommitInfo(existingFile, mode: WriteMode.Overwrite.Instance);
        await dbx.Files.UploadAsync(commitInfo, fileStream);
        return (Path: existingFile, Data: bytes);
    }
    
    public static async Task<(string Path, byte[] Data)> GetDeletedFile(string rootDirectory)
    {
        using var dbx = new DropboxClient(IntegrationTestSecrets.DropBoxRefreshToken,
            IntegrationTestSecrets.DropBoxAppKey, IntegrationTestSecrets.DropBoxAppSecret);

        var deletedFile = Path.Combine(rootDirectory, Path.GetRandomFileName());
        
        var bytes = new byte[10];
        _rnd.NextBytes(bytes);
        using var fileStream = new MemoryStream(bytes);
        var commitInfo = new CommitInfo(deletedFile, mode: WriteMode.Overwrite.Instance);
        await dbx.Files.UploadAsync(commitInfo, fileStream);
        await dbx.Files.DeleteV2Async(deletedFile);
        return (Path: deletedFile, Data: bytes);
    }
    
    public static async Task<(string Path, byte[] Data)> GetNewFile(string rootDirectory)
    {
        var newFile = Path.Combine(rootDirectory, Path.GetRandomFileName());
        var bytes = new byte[10];
        _rnd.NextBytes(bytes);
        return (Path: newFile, Data: bytes);
    }

    public static async Task<(string Path, byte[] Data)> GetInvalidFile()
    {
        return (Path: "#$%$%@(#@)#@$*#@##$", Data: Array.Empty<byte>());
    }
    
    public static async Task Cleanup(string rootDirectory)
    {
        try
        {
            using var dbx = new DropboxClient(IntegrationTestSecrets.DropBoxRefreshToken,
                IntegrationTestSecrets.DropBoxAppKey, IntegrationTestSecrets.DropBoxAppSecret);
            await dbx.Files.DeleteV2Async(rootDirectory);
        }
        catch (Exception)
        {
            // can happen if the folder doesn't exist
        }
    }
}