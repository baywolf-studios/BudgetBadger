using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace BudgetBadger.Forms.Authentication
{
    public class DropboxWebAuthentication : IWebAuthentication
    {
        private readonly string _appKey;
        private readonly string _redirectUrl;

        public DropboxWebAuthentication(string appKey, string redirectUrl)
        {
            _appKey = appKey;
            _redirectUrl = redirectUrl;
        }

        public async Task<WebAuthenticationResult> AuthenticateAsync()
        {
            var result = new WebAuthenticationResult();
            var authResult = await WebAuthenticator.AuthenticateAsync(
        new Uri("https://www.dropbox.com/1/oauth2/authorize?response_type=token&redirect_uri=" + _redirectUrl + "&client_id=" + _appKey),
        new Uri(_redirectUrl));

            result.AccessToken = authResult?.AccessToken;

            return result;
        }
    }
}
