using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface ITransactionLogic
    {
        Task<Result> ValidateTransactionAsync(Transaction transaction);
        Task<Result<Transaction>> SaveTransactionAsync(Transaction transaction);
        Task<Result> DeleteTransactionAsync(Guid id);
        Task<Result<Transaction>> GetTransactionAsync(Guid id);
        Task<Result<IEnumerable<Transaction>>> GetTransactionsAsync();
        Task<Result<IEnumerable<Transaction>>> GetAccountTransactionsAsync(Account account);
        Task<Result<IEnumerable<Transaction>>> GetEnvelopeTransactionsAsync(Envelope envelope);
        Task<Result<IEnumerable<Transaction>>> GetPayeeTransactionsAsync(Payee payee);

        ILookup<string, Transaction> GroupTransactions(IEnumerable<Transaction> transactions);
        Task<Result<Transaction>> GetCorrectedTransaction(Transaction transaction);
    }
}
