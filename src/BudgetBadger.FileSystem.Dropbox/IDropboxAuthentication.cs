using System.Threading.Tasks;
using BudgetBadger.Core.Models;

namespace BudgetBadger.FileSystem.Dropbox
{
    public interface IDropboxAuthentication
    {
        Task<Result<string>> GetRefreshTokenAsync(string appKey);
    }
}
