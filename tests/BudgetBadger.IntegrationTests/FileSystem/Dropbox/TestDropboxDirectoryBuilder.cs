using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.IntegrationTests;
using Dropbox.Api;

namespace BudgetBadger.IntegrationTests.FileSystem.Dropbox;

public static class TestDropboxDirectoryBuilder
{
    private static readonly Random _rnd = new Random();

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_rnd.Next(s.Length)]).ToArray());
    }
    
    public static async Task<string> GetExistingDirectory(string rootDirectory)
    {
        using var dbx = new DropboxClient(IntegrationTestSecrets.DropBoxRefreshToken,
            IntegrationTestSecrets.DropBoxAppKey, IntegrationTestSecrets.DropBoxAppSecret);

        var existingDirectory = Path.Combine(rootDirectory, RandomString(10));

        await dbx.Files.CreateFolderV2Async(existingDirectory);
        return existingDirectory;
    }
    
    public static async Task<string> GetDeletedDirectory(string rootDirectory)
    {
        using var dbx = new DropboxClient(IntegrationTestSecrets.DropBoxRefreshToken,
            IntegrationTestSecrets.DropBoxAppKey, IntegrationTestSecrets.DropBoxAppSecret);

        var deletedDirectory = Path.Combine(rootDirectory, RandomString(10));
        
        await dbx.Files.CreateFolderV2Async(deletedDirectory);
        await dbx.Files.DeleteV2Async(deletedDirectory);
        return deletedDirectory;
    }
    
    public static async Task<string> GetNewDirectory(string rootDirectory)
    {
        var newDirectory = Path.Combine(rootDirectory, RandomString(10));
        return newDirectory;
    }

    public static async Task<string> GetInvalidDirectory()
    {
        return new string(Path.GetInvalidPathChars());
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