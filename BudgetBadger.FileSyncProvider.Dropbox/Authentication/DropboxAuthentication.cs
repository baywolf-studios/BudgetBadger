using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Models;
using Dropbox.Api;

namespace BudgetBadger.FileSyncProvider.Dropbox.Authentication
{
    public class DropboxAuthentication : IDropboxAuthentication
    {
        private readonly IWebAuthenticator _webAuthenticator;
        private readonly Uri _redirectUrl = new Uri("budgetbadger://authorize");
        private readonly string _appKey;

        public DropboxAuthentication(IWebAuthenticator webAuthenticator, string appKey)
        {
            _webAuthenticator = webAuthenticator;
            _appKey = appKey;
            
            //_requestUrl = new Uri("https://www.dropbox.com/1/oauth2/authorize?response_type=code&code_challenge=lSEB3zK2TM-X38Baht80CvC4E_a5DnpCG52y5a7dQyk&code_challenge_method=S256&redirect_uri=" + _redirectUrl + "&client_id=" + appKey);
        }

        public async Task<Result<DropboxAuthenticationResult>> AuthenticateAsync()
        {
            var result = new Result<DropboxAuthenticationResult>();

            var codeVerifier = DropboxOAuth2Helper.GeneratePKCECodeVerifier();
            var codeChallenge = DropboxOAuth2Helper.GeneratePKCECodeChallenge(codeVerifier);
            //var requestUrl = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, _appKey, _redirectUrl.AbsoluteUri, codeChallenge: codeChallenge, tokenAccessType: TokenAccessType.Offline);

            var requestUrl = new Uri("https://www.dropbox.com/oauth2/authorize?response_type=code&client_id=" + _appKey + "&redirect_uri=" + _redirectUrl + "&token_access_type=offline&code_challenge_method=S256&code_challenge=" + codeChallenge);

            var authResult = await _webAuthenticator.AuthenticateAsync(requestUrl, _redirectUrl);

            if (authResult.Success
                && authResult.Data.TryGetValue("code", out string code))
            {
                try
                {
                    var tokenRespone = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, _appKey, codeVerifier: codeVerifier, redirectUri: _redirectUrl.AbsoluteUri);

                    if (!string.IsNullOrEmpty(tokenRespone.RefreshToken))
                    {
                        result.Success = true;
                        result.Data = new DropboxAuthenticationResult()
                        {
                            RefreshToken = tokenRespone.RefreshToken
                        };
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = tokenRespone.Uid;
                    }
                }
                catch(Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
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
