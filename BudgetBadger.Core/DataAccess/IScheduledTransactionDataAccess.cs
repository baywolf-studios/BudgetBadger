using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.DataAccess
{
    public interface IScheduledTransactionDataAccess
    {
        Task Init();

        Task CreateScheduledTransactionAsync(ScheduledTransaction scheduledTransaction);
        Task UpdateScheduledTransaction(ScheduledTransaction scheduledTransaction);
        Task<ScheduledTransaction> ReadScheduledTransactionsAsync();
        Task DeleteScheduledTransactionAsync(Guid id);
        Task<int> GetScheduledTransactionsCountAsync();
    }
}
