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
        Task DeleteAccountAsync(Guid id);
    }
}
