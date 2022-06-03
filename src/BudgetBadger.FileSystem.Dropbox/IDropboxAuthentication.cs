using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.FileSystem.Dropbox
{
    public interface IDropboxAuthentication
    {
        Task<Result<string>> GetRefreshTokenAsync(string appKey);
    }
}
