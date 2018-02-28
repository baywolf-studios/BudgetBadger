using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

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

            var tasks = transactions.Select(t => GetPopulatedTransaction(t));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result<IEnumerable<Transaction>>> GetEnvelopeTransactionsAsync(Envelope envelope)
        {
            var result = new Result<IEnumerable<Transaction>>();

            var transactions = await TransactionDataAccess.ReadEnvelopeTransactionsAsync(envelope.Id);

            var tasks = transactions.Select(t => GetPopulatedTransaction(t));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result<IEnumerable<Transaction>>> GetPayeeTransactionsAsync(Payee payee)
        {
            var result = new Result<IEnumerable<Transaction>>();

            var transactions = await TransactionDataAccess.ReadPayeeTransactionsAsync(payee.Id);

            var tasks = transactions.Select(t => GetPopulatedTransaction(t));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result<Transaction>> GetTransactionAsync(Guid id)
        {
            var result = new Result<Transaction>();

            try
            {
                var transaction = await TransactionDataAccess.ReadTransactionAsync(id);
                var populatedTransaction = await GetPopulatedTransaction(transaction);

                result.Success = true;
                result.Data = populatedTransaction;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IEnumerable<Transaction>>> GetTransactionsAsync()
        {
            var result = new Result<IEnumerable<Transaction>>();

            var transactions = await TransactionDataAccess.ReadTransactionsAsync();

            var tasks = transactions.Select(t => GetPopulatedTransaction(t));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public ILookup<string, Transaction> GroupTransactions(IEnumerable<Transaction> transactions)
        {
            var groupedTransactions = transactions.ToLookup(t => String.Format("{0:d}", t.ServiceDate));

            return groupedTransactions;
        }

        public async Task<Result<Transaction>> SaveTransactionAsync(Transaction transaction)
        {
            if (!transaction.IsValid())
            {
                return transaction.Validate().ToResult<Transaction>();
            }

            // check for existance of payee
            var transactionPayee = await PayeeDataAccess.ReadPayeeAsync(transaction.Payee.Id);
            if (!transactionPayee.Exists)
            {
                return new Result<Transaction> { Success = false, Message = "Payee does not exist" };
            }

            // check for existance of account
            var transactionAccount = await AccountDataAccess.ReadAccountAsync(transaction.Account.Id);
            if (!transactionAccount.Exists)
            {
                return new Result<Transaction> { Success = false, Message = "Account does not exist" };
            }

            // check for existance of envelope
            var transactionEnvelope = await EnvelopeDataAccess.ReadEnvelopeAsync(transaction.Envelope.Id);
            if (!transaction.Envelope.IsGenericDebtEnvelope() && !transactionEnvelope.Exists)
            {
                return new Result<Transaction> { Success = false, Message = "Envelope does not exist" };
            }

            var result = new Result<Transaction>();
            var transactionToUpsert = transaction.DeepCopy();
            var dateTimeNow = DateTime.Now;

            if(transactionToUpsert.IsTransfer)
            {
                // business rule forces account to be the on budget one
                try
                {
                    var payeeAccount = await AccountDataAccess.ReadAccountAsync(transactionToUpsert.Payee.Id);
                    if (transactionToUpsert.Account.OffBudget && payeeAccount.OnBudget)
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

            if (transactionToUpsert.Envelope.IsGenericDebtEnvelope())
            {
                try
                {
                    var accountDebtEnvelope = await EnvelopeDataAccess.ReadEnvelopeAsync(transactionToUpsert.Account.Id);
                    transactionToUpsert.Envelope = accountDebtEnvelope;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                    return result;
                }
            }

            if (transactionToUpsert.IsNew)
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

        // handle logic to get transaction into usable state
        public async Task<Result<Transaction>> GetCorrectedTransaction(Transaction transaction)
        {
            var transactionToPopulate = transaction.DeepCopy();

            // transfer logic
            bool envelopeNotNeeded = false;
            if (transactionToPopulate.Account.Exists)
            {
                // if it is a transfer (aka account to account)
                if (transactionToPopulate.IsTransfer)
                {
                    var payeeAccount = await AccountDataAccess.ReadAccountAsync(transactionToPopulate.Payee.Id);
                    // determine if both accounts are the same budget type
                    if (transactionToPopulate.Account.OnBudget == payeeAccount.OnBudget)
                    {
                        envelopeNotNeeded = true;
                    }
                }
                else if (transactionToPopulate.Account.OffBudget)
                {
                    // if account is offbudget and it's not a transfer then envelope is not needed
                    envelopeNotNeeded = true;
                }
            }

            if (envelopeNotNeeded && !transactionToPopulate.Envelope.IsSystem())
            {
                transactionToPopulate.Envelope = Constants.IgnoredEnvelope;
            }
            else if (!envelopeNotNeeded && transactionToPopulate.Envelope.IsSystem())
            {
                transactionToPopulate.Envelope = new Envelope();
            }

            // handle logic to set envelope to the generic debt envelope
            if (transactionToPopulate.Envelope.Group.IsDebt() && !transactionToPopulate.Envelope.IsGenericDebtEnvelope())
            {
                var debtAccount = await AccountDataAccess.ReadAccountAsync(transactionToPopulate.Envelope.Id);
                transactionToPopulate.Account = debtAccount;
                transactionToPopulate.Envelope = Constants.GenericDebtEnvelope;
            }

            return new Result<Transaction> { Success = true, Data = transactionToPopulate };
        }

        public async Task<Transaction> GetPopulatedTransaction(Transaction transaction)
        {
            transaction.Payee = await PayeeDataAccess.ReadPayeeAsync(transaction.Payee.Id);
            var payeeAccount = await AccountDataAccess.ReadAccountAsync(transaction.Payee.Id);
            transaction.Payee.IsAccount = payeeAccount.Exists;

            transaction.Envelope = await EnvelopeDataAccess.ReadEnvelopeAsync(transaction.Envelope.Id);

            transaction.Account = await AccountDataAccess.ReadAccountAsync(transaction.Account.Id);

            return transaction;
        }
    }
}
