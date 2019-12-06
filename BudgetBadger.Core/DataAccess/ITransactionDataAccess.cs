using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.DataAccess
{
    public interface ITransactionDataAccess
    {
        Task CreateTransactionAsync(Transaction transaction);
        Task<Transaction> ReadTransactionAsync(Guid id);
        Task<IReadOnlyList<Transaction>> ReadAccountTransactionsAsync(Guid accountId);
        Task<IReadOnlyList<Transaction>> ReadPayeeTransactionsAsync(Guid payeeId);
        Task<IReadOnlyList<Transaction>> ReadEnvelopeTransactionsAsync(Guid envelopeId);
        Task<IReadOnlyList<Transaction>> ReadSplitTransactionsAsync(Guid splitId);
        Task<IReadOnlyList<Transaction>> ReadTransactionsAsync();
        Task UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(Guid id);
        Task<int> GetTransactionsCountAsync();
    }
}
