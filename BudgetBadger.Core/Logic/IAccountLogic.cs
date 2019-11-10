using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IAccountLogic
    {
        Task<Result<Account>> SaveAccountAsync(Account account);
        Task<Result<int>> GetAccountsCountAsync();
        Task<Result<Account>> GetAccountAsync(Guid id);
        Task<Result<IReadOnlyList<Account>>> GetAccountsAsync();
        Task<Result<IReadOnlyList<Account>>> GetAccountsForSelectionAsync();
        Task<Result<IReadOnlyList<Account>>> GetHiddenAccountsAsync();
        Task<Result> SoftDeleteAccountAsync(Guid id);
        Task<Result> HideAccountAsync(Guid id);
        Task<Result> UnhideAccountAsync(Guid id);

        Task<Result> ReconcileAccount(Guid accountId, DateTime dateTime, decimal amount);

        bool FilterAccount(Account account, string searchText);

        // removing these
        Task<Result> DeleteAccountAsync(Guid id);
        Task<Result> UndoDeleteAccountAsync(Guid id);
        Task<Result<IReadOnlyList<Account>>> GetDeletedAccountsAsync();
    }
}
