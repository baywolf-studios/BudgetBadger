using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Forms.Authentication
{
    public interface IDropboxAuthentication
    {
         Task<Result<string>> AuthenticateAsync();
    }
}
