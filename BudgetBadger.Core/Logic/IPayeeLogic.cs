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
        Task<Result> DeletePayeeAsync(Guid id);
        Task<Result<Payee>> GetPayeeAsync(Guid id);
        Task<Result<IEnumerable<Payee>>> GetPayeesAsync();
        Task<Result<IEnumerable<Payee>>> GetPayeesForSelectionAsync();

        IEnumerable<Payee> SearchPayees(IEnumerable<Payee> payees, string searchText);
        ILookup<string, Payee> GroupPayees(IEnumerable<Payee> payees);
    }
}
