using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IPayeeLogic
    {
        Task<Result> ValidatePayeeAsync(Payee payee);
        Task<Result<Payee>> SavePayeeAsync(Payee payee);
        Task<Result> DeletePayeeAsync(Guid id);
        Task<Result<Payee>> GetPayeeAsync(Guid id);
        Task<Result<IReadOnlyList<Payee>>> GetPayeesAsync();
        Task<Result<IReadOnlyList<Payee>>> GetPayeesForSelectionAsync();

        IReadOnlyList<Payee> SearchPayees(IEnumerable<Payee> payees, string searchText);
        ILookup<string, Payee> GroupPayees(IEnumerable<Payee> payees);
    }
}
