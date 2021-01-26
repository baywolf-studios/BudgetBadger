using BudgetBadger.Forms;
using BudgetBadger.Forms.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using WebAuthenticationResult = BudgetBadger.Forms.Authentication.WebAuthenticationResult;

namespace BudgetBadger.UWP
{
    public class UwpDropboxWebAuthentication : IWebAuthentication
    {
        private readonly string _appKey;
        private readonly string _redirectUrl;

        public UwpDropboxWebAuthentication()
        {
            _appKey = AppSecrets.DropBoxAppKey;
            _redirectUrl = "budgetbadger://authorize";
        }

        public async Task<WebAuthenticationResult> AuthenticateAsync()
        {
            try
            {

                var r = await CustomWebAuthentication.AuthenticateAsync(WebAuthenticationOptions.None,
        new Uri("https://www.dropbox.com/1/oauth2/authorize?response_type=token&redirect_uri=" + _redirectUrl + "&client_id=" + _appKey));

                switch (r.ResponseStatus)
                {
                    case WebAuthenticationStatus.Success:
                        return new WebAuthenticationResult() { AccessToken = r.ResponseData };
                    case WebAuthenticationStatus.UserCancel:
                        throw new TaskCanceledException();
                    case WebAuthenticationStatus.ErrorHttp:
                         throw new HttpRequestException("Error: " + r.ResponseErrorDetail);
                    default:
                        throw new Exception("Response: " + r.ResponseData.ToString() + "\nStatus: " + r.ResponseStatus);
                }
            }
            catch (FileNotFoundException)
            {
                throw new TaskCanceledException();
            }
        }
    }
}
