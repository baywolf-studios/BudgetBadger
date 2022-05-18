using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;

namespace BudgetBadger.Core.Logic
{
    public interface IMergeLogic
    {
        Task MergeAccountsAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess);
        Task MergePayeesAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess);
        Task MergeEnvelopeGroupsAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess);
        Task MergeEnvelopesAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess);
        Task MergeBudgetSchedulesAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess);
        Task MergeBudgetsAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess);
        Task MergeTransactionsAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess);
        Task MergeAllAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess);
    }
}