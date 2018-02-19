using System;
namespace BudgetBadger.Core.Authentication
{
    public static class AuthMessenger
    {
        public delegate void AuthEvent(object sender, Uri authUrl);

        public static event AuthEvent Subscribe;

        public static void SendAuthInfo(Uri authUrl)
        {
            Subscribe?.Invoke(null, authUrl);
        }
    }
}
