using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.DataAccess
{
    public interface IAccountDataAccess
    {
        Task CreateAccountAsync(Account account);
        Task<Account> ReadAccountAsync(Guid id);
        Task<IReadOnlyList<Account>> ReadAccountsAsync();
        Task UpdateAccountAsync(Account account);
        Task SoftDeleteAccountAsync(Guid id);
        Task HideAccountAsync(Guid id);
        Task UnhideAccountAsync(Guid id);
        Task PurgeAccountsAsync(DateTime deletedBefore);
        Task<int> GetAccountsCountAsync();
    }
}
