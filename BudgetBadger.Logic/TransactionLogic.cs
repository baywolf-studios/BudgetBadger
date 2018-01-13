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

        public async Task<Result> DeleteTransactionAsync(Transaction transaction)
        {
            var result = new Result();
            var transactionToDelete = transaction.DeepCopy();
            transactionToDelete.DeletedDateTime = DateTime.Now;

            try
            {
                await TransactionDataAccess.UpdateTransactionAsync(transactionToDelete);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IEnumerable<Transaction>>> GetAccountTransactionsAsync(Account account)
        {
            var result = new Result<IEnumerable<Transaction>>();
            var transactions = new List<Transaction>();

            transactions.AddRange(await TransactionDataAccess.ReadAccountTransactionsAsync(account.Id));

            var payeeTransactions = await TransactionDataAccess.ReadPayeeTransactionsAsync(account.Id);

            foreach (var transaction in payeeTransactions)
            {
                transaction.Payee.Id = transaction.Account.Id;
                transaction.Account.Id = account.Id;
                transaction.Amount = -1 * transaction.Amount;
                transactions.Add(transaction);
            }

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
            var result = new Result<Transaction>();
            var transactionToUpsert = transaction.DeepCopy();
            var dateTimeNow = DateTime.Now;

            if(await IsTransferTransaction(transactionToUpsert))
            {
                // business rule forces account to be the on budget one
                try
                {
                    var payeeAccount = await AccountDataAccess.ReadAccountAsync(transactionToUpsert.Payee.Id);
                    if (!transactionToUpsert.Account.OnBudget && payeeAccount.OnBudget)
                    {
                        transactionToUpsert.Payee = await PayeeDataAccess.ReadPayeeAsync(transactionToUpsert.Account.Id);
                        transactionToUpsert.Account = payeeAccount;
                        transactionToUpsert.Amount = -1 * transactionToUpsert.Amount;
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                    return result;
                }
            }

            if (transactionToUpsert.CreatedDateTime == null)
            {
                transactionToUpsert.Id = Guid.NewGuid();
                transactionToUpsert.CreatedDateTime = dateTimeNow;
                transactionToUpsert.ModifiedDateTime = dateTimeNow;

                try
                {
                    await TransactionDataAccess.CreateTransactionAsync(transactionToUpsert);
                    result.Success = true;
                    result.Data = transactionToUpsert;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
            }
            else
            {
                transactionToUpsert.ModifiedDateTime = dateTimeNow;

                try
                {
                    await TransactionDataAccess.UpdateTransactionAsync(transactionToUpsert);
                    result.Success = true;
                    result.Data = transactionToUpsert;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
            }

            return result;
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
