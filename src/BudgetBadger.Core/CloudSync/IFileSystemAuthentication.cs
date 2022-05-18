using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.CloudSync
{
    public interface IFileSystemAuthentication
    {
        Task<Result<IReadOnlyDictionary<string, string>>> Authenticate(IReadOnlyDictionary<string, string> parameters);
    }
}
