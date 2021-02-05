using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IScheduledTransactionLogic
    {
        Task<Result<ScheduledTransaction>> SaveScheduledTransactionAsync(ScheduledTransaction scheduledTransaction);
        Task<Result<int>> GetScheduledTransactionsCountAsync();
        Task<Result<IReadOnlyList<ScheduledTransaction>>> GetScheduledTransactions();
        Task<Result<IReadOnlyList<ScheduledTransaction>>> GetPendingTransactionsAsync();
        Task<Result<ScheduledTransaction>> SoftDeleteScheduledTransactionAsync(Guid transactionId);

        bool FilterScheduledTransaction(ScheduledTransaction scheduledTransaction, string searchText);
    }
}
