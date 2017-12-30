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
        Task<IEnumerable<Transaction>> ReadAccountTransactionsAsync(Guid accountId);
        Task<IEnumerable<Transaction>> ReadPayeeTransactionsAsync(Guid payeeId);
        Task<IEnumerable<Transaction>> ReadEnvelopeTransactionsAsync(Guid envelopeId);
        Task<IEnumerable<Transaction>> ReadTransactionsAsync();
        Task UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransaction(Guid id);
    }
}
