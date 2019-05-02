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
        Task<Result> DeleteTransactionAsync(Guid transactionId);
        Task<Result<Transaction>> GetTransactionAsync(Guid transactionId);
        Task<Result<IReadOnlyList<Transaction>>> GetTransactionsAsync();
        Task<Result<IReadOnlyList<Transaction>>> GetAccountTransactionsAsync(Account account);
        Task<Result<IReadOnlyList<Transaction>>> GetEnvelopeTransactionsAsync(Envelope envelope);
        Task<Result<IReadOnlyList<Transaction>>> GetPayeeTransactionsAsync(Payee payee);
        Task<Result<int>> GetTransactionsCountAsync();

        Task<Result<IReadOnlyList<Transaction>>> GetTransactionsFromSplitAsync(Guid splitId);
        Task<Result> SaveSplitTransactionAsync(IEnumerable<Transaction> transactions);
        Task<Result> UpdateSplitTransactionPostedAsync(Guid splitID, bool posted);

        bool FilterTransaction(Transaction transaction, string searchText);

        Task<Result<Transaction>> GetCorrectedTransaction(Transaction transaction);
    }
}
