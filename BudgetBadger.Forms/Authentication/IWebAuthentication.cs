using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BudgetBadger.Forms.Authentication
{
    public interface IWebAuthentication
    {
         Task<WebAuthenticationResult> AuthenticateAsync();
    }
}
