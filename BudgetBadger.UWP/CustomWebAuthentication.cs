using BudgetBadger.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Essentials;

namespace BudgetBadger.UWP
{
    public static class CustomWebAuthentication
    {
        private static Uri _redirectUri;
        private static TaskCompletionSource<Result<IDictionary<string, string>>> _tcsResponse;

        public static async Task<Result<IDictionary<string, string>>> AuthenticateAsync(Uri requestUri, Uri redirectUri)
        {
            if (_tcsResponse?.Task != null && !_tcsResponse.Task.IsCompleted)
                _tcsResponse.TrySetCanceled();

            _redirectUri = redirectUri;
            _tcsResponse = new TaskCompletionSource<Result<IDictionary<string, string>>>();

            try
            {
                await Browser.OpenAsync(requestUri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                _tcsResponse.TrySetException(ex);
            }

            return await _tcsResponse.Task;
        }

        internal static bool OpenUrl(Uri uri)
        {
            // If we aren't waiting on a task, don't handle the url
            if (_tcsResponse?.Task?.IsCompleted ?? true)
                return false;

            try
            {
                // If we can't handle the url, don't
                if (!CanHandleCallback(_redirectUri, uri))
                    return false;

                var result = new Result<IDictionary<string, string>>
                {
                    Success = true,
                    Data = ParseQueryString(uri.Fragment)
                };
                _tcsResponse.TrySetResult(result);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        private static bool CanHandleCallback(Uri expectedUrl, Uri callbackUrl)
        {
            if (!callbackUrl.Scheme.Equals(expectedUrl.Scheme, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrEmpty(expectedUrl.Host))
            {
                if (!callbackUrl.Host.Equals(expectedUrl.Host, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }
        private static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var nvc = HttpUtility.ParseQueryString(queryString);
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }
    }
}