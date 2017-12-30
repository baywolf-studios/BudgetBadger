using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public class TransactionLogic : ITransactionLogic
    {
        readonly ITransactionDataAccess TransactionDataAccess;
        readonly IAccountDataAccess AccountDataAccess;
        readonly IPayeeDataAccess PayeeDataAccess;
        readonly IEnvelopeDataAccess EnvelopeDataAccess;

        public TransactionLogic(ITransactionDataAccess transactionDataAccess, IAccountDataAccess accountDataAccess, IPayeeDataAccess payeeDataAccess, IEnvelopeDataAccess envelopeDataAccess)
        {
            TransactionDataAccess = transactionDataAccess;
            AccountDataAccess = accountDataAccess;
            PayeeDataAccess = payeeDataAccess;
            EnvelopeDataAccess = envelopeDataAccess;
        }

        public Task<Result> DeleteTransactionAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<IEnumerable<Transaction>>> GetAccountTransactionsAsync(Account account)
        {
            var result = new Result<IEnumerable<Transaction>>();

            var transactions = await TransactionDataAccess.ReadAccountTransactionsAsync(account.Id);

            var tasks = transactions.Select(t => FillTransaction(t));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result<IEnumerable<Transaction>>> GetEnvelopeTransactionsAsync(Envelope envelope)
        {
            var result = new Result<IEnumerable<Transaction>>();

            var transactions = await TransactionDataAccess.ReadEnvelopeTransactionsAsync(envelope.Id);

            var tasks = transactions.Select(t => FillTransaction(t));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result<IEnumerable<Transaction>>> GetPayeeTransactionsAsync(Payee payee)
        {
            var result = new Result<IEnumerable<Transaction>>();

            var transactions = await TransactionDataAccess.ReadPayeeTransactionsAsync(payee.Id);

            var tasks = transactions.Select(t => FillTransaction(t));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public Task<Result<Transaction>> GetTransactionAsync(Guid id)
        {
            return Task.FromResult(new Result<Transaction>());
        }

        public Task<Result<IEnumerable<Transaction>>> GetTransactionsAsync()
        {
            return Task.FromResult(new Result<IEnumerable<Transaction>>());
        }

        public IEnumerable<GroupedList<Transaction>> GroupTransactions(IEnumerable<Transaction> transactions)
        {
            var groupedTransactions = new List<GroupedList<Transaction>>();
            var tempGroups = transactions.GroupBy(t => String.Format("{0:d}", t.ServiceDate));

            foreach (var tempGroup in tempGroups)
            {
                var newTemp = new GroupedList<Transaction>(tempGroup.Key, tempGroup.Key);
                newTemp.AddRange(tempGroup);
                groupedTransactions.Add(newTemp);
            }

            return groupedTransactions;
        }

        public async Task<Result<Transaction>> UpsertTransactionAsync(Transaction transaction)
        {
            var newTransaction = transaction.DeepCopy();

            if (newTransaction.CreatedDateTime == null)
            {
                newTransaction.CreatedDateTime = DateTime.Now;
                newTransaction.ModifiedDateTime = DateTime.Now;
                await TransactionDataAccess.CreateTransactionAsync(newTransaction);
            }
            else
            {
                newTransaction.ModifiedDateTime = DateTime.Now;
                await TransactionDataAccess.UpdateTransactionAsync(newTransaction);
            }

            return new Result<Transaction> { Success = true, Data = newTransaction };
        }

        public async Task<Transaction> FillTransaction(Transaction transaction)
        {
            transaction.Payee = await PayeeDataAccess.ReadPayeeAsync(transaction.Payee.Id);
            transaction.Envelope = await EnvelopeDataAccess.ReadEnvelopeAsync(transaction.Envelope.Id);
            transaction.Account = await AccountDataAccess.ReadAccountAsync(transaction.Account.Id);

            return transaction;
        }
    }
}
