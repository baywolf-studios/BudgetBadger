using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BudgetBadger.Core.FileSystem;
using BudgetBadger.Core.Utilities;
using Flurl;
using WebDav;

namespace BudgetBadger.FileSystem.WebDav
{
    public class WebDavDirectory : IDirectory
    {
        private static HttpClientHandler HttpClientHandler = new HttpClientHandler();
        private static HttpClient HttpClient = new HttpClient(HttpClientHandler);
        private static IWebDavClient WebDavClient = new WebDavClient(HttpClient);
        private static string BaseAddress;

        public void SetAuthentication(IReadOnlyDictionary<string, string> keys)
        {
            keys.TryGetValue(WebDavSettings.Server, out var server);
            keys.TryGetValue(WebDavSettings.AcceptInvalidCertificate, out var acceptInvalidCertificateString);
            keys.TryGetValue(WebDavSettings.Directory, out var directory);
            keys.TryGetValue(WebDavSettings.Username, out var username);
            keys.TryGetValue(WebDavSettings.Password, out var password);

            BaseAddress = Url.Combine(server, directory);
            if (bool.TryParse(acceptInvalidCertificateString, out var acceptInvalidCertificate))
            {
                switch (acceptInvalidCertificate)
                {
                    case true when HttpClientHandler?.ServerCertificateCustomValidationCallback == null:
                        HttpClientHandler = new HttpClientHandler();
                        HttpClientHandler.ServerCertificateCustomValidationCallback =
                            (sender, cert, chain, sslPolicyErrors) => true;
                        HttpClient = new HttpClient(HttpClientHandler);
                        WebDavClient = new WebDavClient(HttpClient);
                        break;
                    case false when HttpClientHandler.ServerCertificateCustomValidationCallback != null:
                        HttpClientHandler = new HttpClientHandler();
                        HttpClient = new HttpClient(HttpClientHandler);
                        WebDavClient = new WebDavClient(HttpClient);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
            {
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
            }
            else
            {
                HttpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task CreateDirectoryAsync(string path)
        {
            try
            {
                var url = WebDavHelper.GetUrlFromPath(BaseAddress, path);
                var response = await WebDavClient.Mkcol(url);
                WebDavHelper.ValidateResponse(response);
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
                var url = Url.Combine(BaseAddress, path);
                if (url.IsInvalidPath())
                {
                    return false;
                }

                var response = await WebDavClient.Propfind(url);
                WebDavHelper.ValidateResponse(response);

                var trimmedPath = path.TrimEnd('/');
                return response.Resources.Any(r => r.IsCollection && r.Uri.TrimEnd('/').EndsWith(trimmedPath));
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
                var url = WebDavHelper.GetUrlFromPath(BaseAddress, path);

                var response = await WebDavClient.Propfind(url);
                WebDavHelper.ValidateResponse(response);

                var trimmedUrl = url.TrimEnd('/');

                return response.Resources.Where(r => !r.IsCollection && !trimmedUrl.EndsWith(r.Uri.Trim('/')))
                    .Select(r => r.Uri.Substring(r.Uri.IndexOf(path)))
                    .ToList();
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
                var url = WebDavHelper.GetUrlFromPath(BaseAddress, path);

                var response = await WebDavClient.Propfind(url);
                WebDavHelper.ValidateResponse(response);

                var trimmedUrl = url.TrimEnd('/');

                return response.Resources.Where(r => r.IsCollection && !trimmedUrl.EndsWith(r.Uri.Trim('/')))
                    .Select(r => r.Uri.Substring(r.Uri.IndexOf(path)))
                    .ToList();
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
                var url = WebDavHelper.GetUrlFromPath(BaseAddress, path);

                if (!recursive)
                {
                    var contents = await WebDavClient.Propfind(url);
                    WebDavHelper.ValidateResponse(contents);
                    var trimmedUrl = url.TrimEnd('/');
                    if (contents.Resources.Any(r => !trimmedUrl.EndsWith(r.Uri.Trim('/'))))
                    {
                        throw new IOException("Non-empty directory");
                    }
                }

                var response = await WebDavClient.Delete(url);
                WebDavHelper.ValidateResponse(response);
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
                var sourceUrl = WebDavHelper.GetUrlFromPath(BaseAddress, sourceDirName);
                var destUrl = WebDavHelper.GetUrlFromPath(BaseAddress, destDirName);

                if (await ExistsAsync(destDirName))
                {
                    throw new IOException($"{nameof(destDirName)} already exists");
                }

                var response = await WebDavClient.Move(sourceUrl, destUrl);
                WebDavHelper.ValidateResponse(response);
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }
    }
}
