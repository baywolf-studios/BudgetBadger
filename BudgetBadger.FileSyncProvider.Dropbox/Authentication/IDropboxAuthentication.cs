using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.FileSyncProvider.Dropbox.Authentication
{
    public interface IDropboxAuthentication
    {
         Task<Result<DropboxAuthenticationResult>> AuthenticateAsync();
    }
}
