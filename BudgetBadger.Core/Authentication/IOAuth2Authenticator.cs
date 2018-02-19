using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Authentication
{
    public delegate void OAuth2AthenticationHandler(Result<Uri> result);

    public interface IOAuth2Authenticator
    {
        Task<Uri> AuthenticateAsync(string url, string callbackUrlScheme);
    }
}
