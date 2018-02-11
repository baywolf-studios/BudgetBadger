using System;
using System.Windows.Input;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Models;
using Dropbox.Api;
using Prism.Commands;
using PropertyChanged;

namespace BudgetBadger.Forms.Settings
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsPageViewModel
    {
        readonly IOAuth2Authenticator Authenticator;

        public ICommand DropBoxSyncCommand { get; set; }

        public SettingsPageViewModel(IOAuth2Authenticator authenticator)
        {
            Authenticator = authenticator;

            DropBoxSyncCommand = new DelegateCommand(() => ExecuteDropBoxSyncCommand());
        }

        public void ExecuteDropBoxSyncCommand()
        {
            var oauth2State = Guid.NewGuid().ToString("N");
            var callbackUrl = new Uri(@"bb://test");
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, "", callbackUrl, state: oauth2State);
            Authenticator.Authenticate(authorizeUri.AbsoluteUri, @"bb://", HandleOAuth2AthenticationHandler);
        }

        void HandleOAuth2AthenticationHandler(Result<Uri> result)
        {
            var test = result.Success;
        }
    }
}
