using BudgetBadger.Forms;
using BudgetBadger.Forms.Authentication;
using BudgetBadger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace BudgetBadger.UWP
{
    public class UwpWebAuthenticator : IWebAuthenticator
    {
        public async Task<Result<IDictionary<string, string>>> AuthenticateAsync(Uri requestUri, Uri callbackUri)
        {
            return await CustomWebAuthentication.AuthenticateAsync(requestUri, callbackUri);
        }
    }
}
