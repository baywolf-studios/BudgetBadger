using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Forms.Authentication
{
    public class WebAuthenticator : IWebAuthenticator
    {
        public WebAuthenticator()
        {
        }

        public async Task<Result<IDictionary<string, string>>> AuthenticateAsync(Uri requestUri, Uri callbackUri)
        {
            var result = new Result<IDictionary<string, string>>();

            try
            {
                var authResult = await Xamarin.Essentials.WebAuthenticator.AuthenticateAsync(requestUri, callbackUri);
                result.Success = true;
                result.Data = authResult.Properties;
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
