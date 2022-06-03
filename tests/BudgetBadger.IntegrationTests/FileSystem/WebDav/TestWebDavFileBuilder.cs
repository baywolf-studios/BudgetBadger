using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using WebDav;

namespace BudgetBadger.IntegrationTests.FileSystem.WebDav;

public static class TestWebDavFileBuilder
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
        await WebDavClient.Mkcol(directory);
    }
    
    public static async Task<(string Path, byte[] Data)> GetExistingFile(string rootDirectory)
    {
        var existingFile = Url.Combine(rootDirectory, Path.GetRandomFileName());
        var existingFileUrl = Url.Combine(BaseAddress, existingFile);
        
        var bytes = new byte[10];
        Rnd.NextBytes(bytes);
        using var fileStream = new MemoryStream(bytes);

        var response = await WebDavClient.PutFile(existingFileUrl, fileStream);
        
        return (Path: existingFile, Data: bytes);
    }
    
    public static async Task<(string Path, byte[] Data)> GetDeletedFile(string rootDirectory)
    {
        var deletedFile = Url.Combine(rootDirectory, Path.GetRandomFileName());
        var deletedFileUrl = Url.Combine(BaseAddress, deletedFile);
        
        var bytes = new byte[10];
        Rnd.NextBytes(bytes);
        using var fileStream = new MemoryStream(bytes);
        
        await WebDavClient.PutFile(deletedFileUrl, fileStream);
        await WebDavClient.Delete(deletedFileUrl);
        
        return (Path: deletedFile, Data: bytes);
    }
    
    public static async Task<(string Path, byte[] Data)> GetNewFile(string rootDirectory)
    {
        var newFile = Url.Combine(rootDirectory, Path.GetRandomFileName());
        var bytes = new byte[10];
        Rnd.NextBytes(bytes);
        return (Path: newFile, Data: bytes);
    }

    public static async Task<(string Path, byte[] Data)> GetInvalidFile()
    {
        return (Path: new string(Path.GetInvalidPathChars()), Data: Array.Empty<byte>());
    }

    public static async Task Cleanup(string rootDirectory)
    {
        try
        {
            await WebDavClient.Delete(Url.Combine(BaseAddress, rootDirectory));
        }
        catch (Exception)
        {
            // can happen if the folder doesn't exist
        }
    }
}
