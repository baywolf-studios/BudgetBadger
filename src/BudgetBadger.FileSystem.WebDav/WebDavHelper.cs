using System;
using BudgetBadger.Core.Utilities;
using Flurl;
using WebDav;

namespace BudgetBadger.FileSystem.WebDav
{
    public static class WebDavHelper
    {
        public static string GetUrlFromPath(string baseAddress, string path)
        {
            var url = Url.Combine(baseAddress, path);
            if (url.IsInvalidPath())
            {
                throw new ArgumentOutOfRangeException(nameof(path));
            }

            return url;
        }

        public static void ValidateResponse(WebDavResponse response)
        {
            if (!response.IsSuccessful)
            {
                throw new Exception(response.ToString());
            }
        }
    }
}
