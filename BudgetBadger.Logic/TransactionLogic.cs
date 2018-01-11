using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Extensions;
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
            var dateTimeNow = DateTime.Now;

            if (newTransaction.CreatedDateTime == null)
            {
                newTransaction.Id = Guid.NewGuid();
                newTransaction.CreatedDateTime = dateTimeNow;
                newTransaction.ModifiedDateTime = dateTimeNow;

                if (await IsTransferTransaction(newTransaction))
                {
                    var accountPayee = await AccountDataAccess.ReadAccountAsync(newTransaction.Payee.Id);
                    var linkedTransferTransaction = newTransaction.DeepCopy();
                    linkedTransferTransaction.Id = Guid.NewGuid();
                    linkedTransferTransaction.Amount = -1 * linkedTransferTransaction.Amount;
                    linkedTransferTransaction.Payee = new Payee { Id = newTransaction.Account.Id };
                    linkedTransferTransaction.Account = accountPayee;
                    await TransactionDataAccess.CreateTransactionAsync(linkedTransferTransaction);
                }

                await TransactionDataAccess.CreateTransactionAsync(newTransaction);
            }
            else
            {
                newTransaction.ModifiedDateTime = dateTimeNow;
                await TransactionDataAccess.UpdateTransactionAsync(newTransaction);
            }

            return new Result<Transaction> { Success = true, Data = newTransaction };
        }

        public async Task<Result<Transaction>> GetPopulatedTransaction(Transaction transaction, Account account = null, Envelope envelope = null, Payee payee = null)
        {
            var newTransaction = transaction.DeepCopy();

            if (account != null)
            {
                newTransaction.Account = account.DeepCopy();
            }

            if (envelope != null)
            {
                newTransaction.Envelope = envelope.DeepCopy();
            }

            if (payee != null)
            {
                newTransaction.Payee = payee.DeepCopy();

                if (await IsTransferTransaction(newTransaction))
                {
                    var payeeAccount = await AccountDataAccess.ReadAccountAsync(payee.Id);
                    // determine if both accounts are on budget or not on budget
                    if ((newTransaction.Account.OnBudget && payeeAccount.OnBudget)
                       || (!newTransaction.Account.OnBudget && !payeeAccount.OnBudget))
                    {
                        newTransaction.Envelope = Constants.TransferEnvelope;
                    }
                }


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

        public async Task<bool> IsTransferTransaction(Transaction transaction)
        {
            //wrap any call to this in a trycatch
            //or refactor into a result<bool> for reuse
            var result = false;
            if (transaction.Envelope.IsTransfer())
            {
                result = true;
            }
            else if (transaction.Payee != null)
            {
                var payeeAccount = await AccountDataAccess.ReadAccountAsync(transaction.Payee.Id);
                if (payeeAccount.Id != Guid.Empty && payeeAccount.CreatedDateTime != null)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}
