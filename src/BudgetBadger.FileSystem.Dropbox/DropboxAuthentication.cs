using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Models;
using Dropbox.Api;

namespace BudgetBadger.FileSystem.Dropbox
{
    public class DropboxAuthentication : IDropboxAuthentication
    {
        private readonly Uri _redirectUrl = new Uri("budgetbadger://authorize");
        private readonly IWebAuthenticator _webAuthenticator;

        public DropboxAuthentication(IWebAuthenticator webAuthenticator)
        {
            _webAuthenticator = webAuthenticator;
        }

        public async Task<Result<string>> GetRefreshTokenAsync(string appKey)
        {
            var result = new Result<string>();

            var codeVerifier = DropboxOAuth2Helper.GeneratePKCECodeVerifier();
            var codeChallenge = DropboxOAuth2Helper.GeneratePKCECodeChallenge(codeVerifier);

            var requestUrl = new Uri("https://www.dropbox.com/oauth2/authorize?response_type=code&client_id=" +
                                     appKey +
                                     "&redirect_uri=" +
                                     _redirectUrl +
                                     "&token_access_type=offline&code_challenge_method=S256&code_challenge=" +
                                     codeChallenge);

            var authResult = await _webAuthenticator.AuthenticateAsync(requestUrl, _redirectUrl);

            if (authResult.Success && authResult.Data.TryGetValue("code", out var code))
            {
                try
                {
                    var tokenResponse = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code,
                        appKey,
                        codeVerifier: codeVerifier,
                        redirectUri: _redirectUrl.AbsoluteUri);

                    if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                    {
                        result.Success = true;
                        result.Data = tokenResponse.RefreshToken;
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = tokenResponse.Uid;
                    }
                }
                catch (Exception ex)
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
