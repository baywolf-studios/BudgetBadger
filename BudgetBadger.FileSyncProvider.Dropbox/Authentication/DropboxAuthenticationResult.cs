using System;
namespace BudgetBadger.FileSyncProvider.Dropbox.Authentication
{
    public class DropboxAuthenticationResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
