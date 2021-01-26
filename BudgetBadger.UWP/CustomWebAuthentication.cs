using Dropbox.Api;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Essentials;

namespace BudgetBadger.UWP
{
    public static class CustomWebAuthentication
    {
        private static TaskCompletionSource<CustomWebAuthenticationResult> tcs1;

        public static Task<CustomWebAuthenticationResult> AuthenticateAsync(WebAuthenticationOptions options, Uri requestUri)
        {
            return AuthenticateAsync(options, requestUri, Windows.Security.Authentication.Web.WebAuthenticationBroker.GetCurrentApplicationCallbackUri());
        }

        public static async Task<CustomWebAuthenticationResult> AuthenticateAsync(WebAuthenticationOptions options, Uri requestUri, Uri callbackUri)
        {
            if (options != WebAuthenticationOptions.None)
                throw new ArgumentException("WebAuthenticationBroker currently only supports WebAuthenticationOptions.None", "options");

            tcs1 = new TaskCompletionSource<CustomWebAuthenticationResult>();

            try
            {
                await Browser.OpenAsync(requestUri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                tcs1.SetException(ex);
            }

            return await tcs1.Task;
        }

        public static void HandleAuthRedirect(Uri url)
        {
            if (url == null || url.Fragment == null || url.Fragment.Length < 1)
            {
                tcs1.SetResult(new CustomWebAuthenticationResult("", 0, WebAuthenticationStatus.UserCancel));
                return;
            }

            try
            {
                var code = DropboxOAuth2Helper.ParseTokenFragment(url);
                tcs1.SetResult(new CustomWebAuthenticationResult(code.AccessToken, 0, string.IsNullOrEmpty(code.AccessToken) ? WebAuthenticationStatus.UserCancel : WebAuthenticationStatus.Success));
            }
            catch (Exception ex)
            {
                tcs1.SetResult(new CustomWebAuthenticationResult("", 0, WebAuthenticationStatus.UserCancel));
            }
            
        }
    }

    public sealed class CustomWebAuthenticationResult
    {
        internal CustomWebAuthenticationResult(string response, uint errorDetail, WebAuthenticationStatus responseStatus)
        {
            ResponseData = response;
            ResponseErrorDetail = errorDetail;
            ResponseStatus = responseStatus;
        }

        public string ResponseData { get; private set; }

        public uint ResponseErrorDetail { get; private set; }

        public WebAuthenticationStatus ResponseStatus { get; private set; }
    }
}