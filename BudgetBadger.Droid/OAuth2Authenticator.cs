using System;
using BudgetBadger.Core.Authentication;
using Android.App;
using Android.Support.CustomTabs;
using System.Threading.Tasks;
using Android.OS;
using SimpleAuth.Providers;

namespace BudgetBadger.Droid
{
    public class OAuth2Authenticator : IOAuth2Authenticator
    {
        public async Task<Uri> AuthenticateAsync(string url, string callbackUrlScheme)
        {
            var api = new DropBoxApi("dropbox", 
                                     "afmrpojfmcilidv",
                                     "",
                                     "budgetbadger://authorize");

            var account = await api.Authenticate();

            return new Uri(account.Identifier);
        }

    }
}
