using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.DataAccess
{
    public interface IPayeeDataAccess
    {
        Task CreatePayeeAsync(Payee payee);
        Task<Payee> ReadPayeeAsync(Guid id);
        Task<IReadOnlyList<Payee>> ReadPayeesAsync();
        Task UpdatePayeeAsync(Payee payee);
        Task DeletePayeeAsync(Guid id);
    }
}
