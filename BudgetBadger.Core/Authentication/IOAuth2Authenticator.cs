using System;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Authentication
{
    public delegate void OAuth2AthenticationHandler(Result<Uri> result);

    public interface IOAuth2Authenticator
    {
        void Authenticate(string url, string callbackUrlScheme, OAuth2AthenticationHandler authenticationCompletedHandler);
    }
}
