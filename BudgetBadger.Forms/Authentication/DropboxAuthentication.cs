using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BudgetBadger.Models;
using Xamarin.Essentials;

namespace BudgetBadger.Forms.Authentication
{
    public class DropboxAuthentication : IDropboxAuthentication
    {
        private readonly IWebAuthenticator _webAuthenticator;
        private readonly Uri _requestUrl;
        private readonly Uri _redirectUrl = new Uri("budgetbadger://authorize");

        public DropboxAuthentication(IWebAuthenticator webAuthenticator, string appKey)
        {
            _webAuthenticator = webAuthenticator;
            _requestUrl = new Uri("https://www.dropbox.com/1/oauth2/authorize?response_type=token&redirect_uri=" + _redirectUrl + "&client_id=" + appKey);
        }

        public async Task<Result<DropboxAuthenticationResult>> AuthenticateAsync()
        {
            var result = new Result<DropboxAuthenticationResult>();
            var authResult = await _webAuthenticator.AuthenticateAsync(_requestUrl, _redirectUrl);

            if (authResult.Success
                && authResult.Data.TryGetValue("access_token", out string accessToken)
                && authResult.Data.TryGetValue("refresh_token", out string refreshToken))
            {
                result.Success = true;
                result.Data = new DropboxAuthenticationResult()
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            else
            {
                result.Success = false;
                result.Message = authResult.Message;
            }

            return result;
        }
    }
}
