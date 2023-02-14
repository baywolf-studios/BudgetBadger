using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Models;

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
        Task<Result<Account>> SoftDeleteAccountAsync(Guid id);
        Task<Result<Account>> HideAccountAsync(Guid id);
        Task<Result<Account>> UnhideAccountAsync(Guid id);

        Task<Result> ReconcileAccount(Guid accountId, DateTime dateTime, decimal amount);

        bool FilterAccount(Account account, string searchText);
        bool FilterAccount(Account account, FilterType filterType);
    }
}
