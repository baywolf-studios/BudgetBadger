using System;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Models;
using SafariServices;

namespace BudgetBadger.iOS
{
    public class OAuth2Authenticator : IOAuth2Authenticator
    {
        SFAuthenticationSession authenticationSession { get; set; }

        public OAuth2Authenticator()
        {
        }

        public void Authenticate(string url, string callbackUrlScheme, OAuth2AthenticationHandler authenticationCompletedHandler)
        {
            

            var authorizeUrl = new Foundation.NSUrl(url);

            authenticationSession = new SFAuthenticationSession(authorizeUrl,
                                                                callbackUrlScheme,
                                                                (callbackUrl, error) =>
            {
                var result = new Result<Uri>();

                result.Success = error == null;
                if (callbackUrl != null)
                {
                    result.Data = new Uri(callbackUrl.AbsoluteString);
                }
                result.Message = error?.Description;

                authenticationCompletedHandler(result);
            });

            authenticationSession.Start();
        }
    }
}
