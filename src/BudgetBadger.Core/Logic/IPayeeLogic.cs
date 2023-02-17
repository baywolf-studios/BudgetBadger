using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IPayeeLogic
    {
        Task<Result<Payee>> SavePayeeAsync(Payee payee);
        Task<Result<Guid>> SavePayeeAsync(PayeeEditModel payee);
        Task<Result<int>> GetHiddenPayeesCountAsync();
        Task<Result<Payee>> GetPayeeAsync(Guid id);
        Task<Result<IReadOnlyList<Payee>>> GetPayeesAsync();
        Task<Result<IReadOnlyList<PayeeEditModel>>> GetPayees2Async(IEnumerable<Guid> payeeIds = null);
        Task<Result<IReadOnlyList<Payee>>> GetPayeesForSelectionAsync();
        Task<Result<IReadOnlyList<PayeeModel>>> GetPayeesForSelection2Async(IEnumerable<Guid> payeeIds = null);
        Task<Result<IReadOnlyList<Payee>>> GetPayeesForReportAsync();
        Task<Result<IReadOnlyList<PayeeModel>>> GetPayeesForReport2Async(IEnumerable<Guid> payeeIds = null);
        Task<Result<IReadOnlyList<Payee>>> GetHiddenPayeesAsync();
        Task<Result<IReadOnlyList<PayeeEditModel>>> GetHiddenPayees2Async(IEnumerable<Guid> payeeIds = null);
        Task<Result<Payee>> SoftDeletePayeeAsync(Guid id);
        Task<Result> SoftDeletePayee2Async(Guid payeeId);
        Task<Result<Payee>> HidePayeeAsync(Guid id);
        Task<Result> HidePayee2Async(Guid payeeId);
        Task<Result<Payee>> UnhidePayeeAsync(Guid id);
        Task<Result> UnhidePayee2Async(Guid payeeId);

        bool FilterPayee(Payee payee, string searchText);
        bool FilterPayee(Payee payee, FilterType filterType);
    }
}
