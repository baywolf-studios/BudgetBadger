using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.Logic
{
    public interface ITransactionLogic
    {
        Task<Result<Transaction>> SaveTransactionAsync(Transaction transaction);
        Task<Result<Transaction>> GetTransactionAsync(Guid transactionId);
        Task<Result<IReadOnlyList<Transaction>>> GetTransactionsAsync();
        Task<Result<IReadOnlyList<Transaction>>> GetAccountTransactionsAsync(Account account);
        Task<Result<IReadOnlyList<Transaction>>> GetEnvelopeTransactionsAsync(Envelope envelope);
        Task<Result<IReadOnlyList<Transaction>>> GetPayeeTransactionsAsync(Payee payee);
        Task<Result<Transaction>> SoftDeleteTransactionAsync(Guid transactionId);

        Task<Result> SaveSplitTransactionAsync(IEnumerable<Transaction> transactions);
        Task<Result<IReadOnlyList<Transaction>>> GetTransactionsFromSplitAsync(Guid splitId);
        Task<Result> UpdateSplitTransactionPostedAsync(Guid splitID, bool posted);

        bool FilterTransaction(Transaction transaction, string searchText);

        Task<Result<Transaction>> GetCorrectedTransaction(Transaction transaction);
    }
}
