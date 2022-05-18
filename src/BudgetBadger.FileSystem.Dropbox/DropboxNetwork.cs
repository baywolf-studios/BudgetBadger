using System;
using System.Net.Http;

namespace BudgetBadger.FileSystem.Dropbox
{
    public class DropboxNetwork
    {
        public static readonly HttpClient StaticHttpClient = new HttpClient();
        public static readonly HttpClient StaticLongPollHttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(480) };
    }
}
