using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Flurl;
using WebDav;

namespace BudgetBadger.IntegrationTests.FileSystem.WebDav;

public class TestWebDavDirectoryBuilder
{
    private static readonly Random Rnd = new Random();
    private static readonly HttpClient HttpClient = new HttpClient();
    private static readonly IWebDavClient WebDavClient = new WebDavClient(HttpClient);
    private static readonly string BaseAddress = Url.Combine(IntegrationTestSecrets.WebDavServer, IntegrationTestSecrets.WebDavDirectory);

    public static async Task Setup(string rootDirectory)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", 
            Convert.ToBase64String(
                ASCIIEncoding.ASCII.GetBytes($"{IntegrationTestSecrets.WebDavUsername}:{IntegrationTestSecrets.WebDavPassword}")));
        var directory = Url.Combine(BaseAddress, rootDirectory);
        var response = await WebDavClient.Mkcol(directory);
        Console.WriteLine(response.StatusCode);
    }
    
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Rnd.Next(s.Length)]).ToArray());
    }
    
    public static async Task<string> GetExistingDirectory(string rootDirectory)
    {
        var existingDirectory = Url.Combine(rootDirectory, RandomString(10));
        var existingDirectoryUrl = Url.Combine(BaseAddress, existingDirectory);
        
        var response = await WebDavClient.Mkcol(existingDirectoryUrl);
        
        return existingDirectory;
    }
    
    public static async Task<string> GetDeletedDirectory(string rootDirectory)
    {
        var deletedDirectory = Url.Combine(rootDirectory, RandomString(10));
        var deletedDirectoryUrl = Url.Combine(BaseAddress, deletedDirectory);
        
        var response = await WebDavClient.Mkcol(deletedDirectoryUrl);
        var response2 = await WebDavClient.Delete(deletedDirectoryUrl);
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
            var response = await WebDavClient.Delete(Url.Combine(BaseAddress, rootDirectory));
            Console.WriteLine(response.StatusCode);
        }
        catch (Exception)
        {
            // can happen if the folder doesn't exist
        }
    }
}
