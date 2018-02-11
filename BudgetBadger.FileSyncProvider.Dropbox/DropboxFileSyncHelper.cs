using System;
using Dropbox.Api;

namespace BudgetBadger.FileSyncProvider.Dropbox
{
    public static class DropboxFileSyncHelper
    {
        static string OAuth2State = Guid.NewGuid().ToString("N");

        public static Uri GetAuthorizationUri(Uri callbackUrl)
        {
            var oauth2State = Guid.NewGuid().ToString("N");
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Token,
                "clientId",
                callbackUrl,
                state: OAuth2State);

            return authorizeUri;
        }

        public static string GetAccessToken(Uri redirectedUri)
        {
            var accessTokenResponse = DropboxOAuth2Helper.ParseTokenFragment(redirectedUri);

            if (OAuth2State == accessTokenResponse.State)
            {
                return accessTokenResponse.AccessToken;
            }

            return string.Empty;
        }
    }
}
