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
    public class WebDavFile : IFile
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

        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            try
            {
                var url = WebDavHelper.GetUrlFromPath(BaseAddress, path);

                using (var response = await WebDavClient.GetRawFile(url))
                {
                    WebDavHelper.ValidateResponse(response);

                    var bytes = response.Stream.ReadAllBytes();

                    return bytes;
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
                var url = WebDavHelper.GetUrlFromPath(BaseAddress, path);
                using (var byteStream = new MemoryStream(data))
                {
                    var response = await WebDavClient.PutFile(url, byteStream);
                    WebDavHelper.ValidateResponse(response);
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
                var url = WebDavHelper.GetUrlFromPath(BaseAddress, path);
                var response = await WebDavClient.Delete(url);
                WebDavHelper.ValidateResponse(response);
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public async Task CopyAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            try
            {
                var sourceUrl = WebDavHelper.GetUrlFromPath(BaseAddress, sourceFileName);
                var destUrl = WebDavHelper.GetUrlFromPath(BaseAddress, destFileName);

                var response =
                    await WebDavClient.Copy(sourceUrl, destUrl, new CopyParameters { Overwrite = overwrite });
                WebDavHelper.ValidateResponse(response);
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        public async Task MoveAsync(string sourceFileName, string destFileName, bool overwrite = false)
        {
            try
            {
                var sourceUrl = WebDavHelper.GetUrlFromPath(BaseAddress, sourceFileName);
                var destUrl = WebDavHelper.GetUrlFromPath(BaseAddress, destFileName);

                var response =
                    await WebDavClient.Move(sourceUrl, destUrl, new MoveParameters { Overwrite = overwrite });
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
                var url = WebDavHelper.GetUrlFromPath(BaseAddress, path);

                var response = await WebDavClient.Propfind(url);
                WebDavHelper.ValidateResponse(response);

                return response.Resources.Any(r => !r.IsCollection && r.Uri.EndsWith(path));
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
