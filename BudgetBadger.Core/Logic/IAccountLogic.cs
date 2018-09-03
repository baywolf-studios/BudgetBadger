using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IAccountLogic
    {
        Task<Result> ValidateAccountAsync(Account account);
        Task<Result<Account>> SaveAccountAsync(Account account);
        Task<Result> DeleteAccountAsync(Guid id);
        Task<Result> UndoDeleteAccountAsync(Guid id);
        Task<Result<Account>> GetAccountAsync(Guid id);
        Task<Result<IReadOnlyList<Account>>> GetAccountsAsync();
        Task<Result<IReadOnlyList<Account>>> GetAccountsForSelectionAsync();
        Task<Result<IReadOnlyList<Account>>> GetDeletedAccountsAsync();

        Task<Result> ReconcileAccount(Guid accountId, DateTime dateTime, decimal amount);

        IReadOnlyList<string> GetAccountTypes();

        bool FilterAccount(Account account, string searchText);
    }
}
