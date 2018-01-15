using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface ITransactionLogic
    {
        Task<Result<Transaction>> SaveTransactionAsync(Transaction transaction);
        Task<Result> DeleteTransactionAsync(Transaction transaction);
        Task<Result<Transaction>> GetTransactionAsync(Guid id);
        Task<Result<IEnumerable<Transaction>>> GetTransactionsAsync();
        Task<Result<IEnumerable<Transaction>>> GetAccountTransactionsAsync(Account account);
        Task<Result<IEnumerable<Transaction>>> GetEnvelopeTransactionsAsync(Envelope envelope);
        Task<Result<IEnumerable<Transaction>>> GetPayeeTransactionsAsync(Payee payee);

        IEnumerable<GroupedList<Transaction>> GroupTransactions(IEnumerable<Transaction> transactions);
        Task<Result<Transaction>> GetCorrectedTransaction(Transaction transaction);
    }
}
