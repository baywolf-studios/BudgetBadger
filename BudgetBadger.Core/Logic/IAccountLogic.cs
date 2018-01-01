using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IAccountLogic
    {
        Task<Result<Account>> UpsertAccountAsync(Account account);
        Task<Result> DeleteAccountAsync(Account account);
        Task<Result<Account>> GetAccountAsync(Guid id);
        Task<Result<IEnumerable<Account>>> GetAccountsAsync();

        Task<Result<IEnumerable<AccountType>>> GetAccountTypesAsync();

        IEnumerable<Account> SearchAccounts(IEnumerable<Account> accounts, string searchText);
        IEnumerable<GroupedList<Account>> GroupAccounts(IEnumerable<Account> accounts);
    }
}
