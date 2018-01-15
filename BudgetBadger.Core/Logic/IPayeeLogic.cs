using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IPayeeLogic
    {
        Task<Result<Payee>> SavePayeeAsync(Payee payee);
        Task<Result> DeletePayeeAsync(Payee payee);
        Task<Result<Payee>> GetPayeeAsync(Guid id);
        Task<Result<IEnumerable<Payee>>> GetPayeesAsync();

        IEnumerable<Payee> SearchPayees(IEnumerable<Payee> payees, string searchText);
        IEnumerable<GroupedList<Payee>> GroupPayees(IEnumerable<Payee> payees);
    }
}
