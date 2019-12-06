using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IPayeeLogic
    {
        Task<Result<Payee>> SavePayeeAsync(Payee payee);
        Task<Result<int>> GetPayeesCountAsync();
        Task<Result<Payee>> GetPayeeAsync(Guid id);
        Task<Result<IReadOnlyList<Payee>>> GetPayeesAsync();
        Task<Result<IReadOnlyList<Payee>>> GetPayeesForSelectionAsync();
        Task<Result<IReadOnlyList<Payee>>> GetPayeesForReportAsync();
        Task<Result<IReadOnlyList<Payee>>> GetHiddenPayeesAsync();
        Task<Result> SoftDeletePayeeAsync(Guid id);
        Task<Result> HidePayeeAsync(Guid id);
        Task<Result> UnhidePayeeAsync(Guid id);

        bool FilterPayee(Payee payee, string searchText);
    }
}
