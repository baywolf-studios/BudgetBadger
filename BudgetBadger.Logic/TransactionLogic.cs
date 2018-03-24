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

        public async Task<Result> DeleteTransactionAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var transactionToDelete = await TransactionDataAccess.ReadTransactionAsync(id);
                transactionToDelete.DeletedDateTime = DateTime.Now;
                await TransactionDataAccess.UpdateTransactionAsync(transactionToDelete);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Transaction>>> GetAccountTransactionsAsync(Account account)
        {
            var result = new Result<IReadOnlyList<Transaction>>();
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

            var tasks = transactions.Where(t => t.IsActive).Select(t => GetPopulatedTransaction(t));

            var completedTasks = await Task.WhenAll(tasks);

            result.Success = true;
            result.Data = CombineSplitTransactions(completedTasks);

            return result;
        }

        public async Task<Result<IReadOnlyList<Transaction>>> GetEnvelopeTransactionsAsync(Envelope envelope)
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await TransactionDataAccess.ReadEnvelopeTransactionsAsync(envelope.Id);

            var tasks = transactions.Where(t => t.IsActive).Select(t => GetPopulatedTransaction(t));

            var completedTasks = await Task.WhenAll(tasks);

            result.Success = true;
            result.Data = CombineSplitTransactions(completedTasks);

            return result;
        }

        public async Task<Result<IReadOnlyList<Transaction>>> GetPayeeTransactionsAsync(Payee payee)
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await TransactionDataAccess.ReadPayeeTransactionsAsync(payee.Id);

            var tasks = transactions.Where(t => t.IsActive).Select(t => GetPopulatedTransaction(t));

            var completedTasks = await Task.WhenAll(tasks);

            result.Success = true;
            result.Data = CombineSplitTransactions(completedTasks);

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

        public async Task<Result<IReadOnlyList<Transaction>>> GetTransactionsAsync()
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await TransactionDataAccess.ReadTransactionsAsync();

            var tasks = transactions.Where(t => t.IsActive).Select(t => GetPopulatedTransaction(t));

            var completedTasks = await Task.WhenAll(tasks);

            result.Success = true;
            result.Data = CombineSplitTransactions(completedTasks);

            return result;
        }

        public IReadOnlyList<Transaction> SearchTransactions(IEnumerable<Transaction> transactions, string searchText)
        {
            //eventually look it up based on envelope description, account description, and payee description
            return transactions.ToList();
        }

        public IReadOnlyList<IGrouping<string, Transaction>> GroupTransactions(IEnumerable<Transaction> transactions)
        {
            var groupedTransactions = transactions.GroupBy(t => String.Format("{0:d}", t.ServiceDate)).ToList();

            return groupedTransactions;
        }

        public async Task<Result> ValidateTransactionAsync(Transaction transaction)
        {
            if (!transaction.IsValid())
            {
                return transaction.Validate();
            }

            // check for existance of payee
            var transactionPayee = await PayeeDataAccess.ReadPayeeAsync(transaction.Payee.Id);
            if (!transactionPayee.IsActive)
            {
                return new Result { Success = false, Message = "Payee does not exist" };
            }

            // check for existance of account
            var transactionAccount = await AccountDataAccess.ReadAccountAsync(transaction.Account.Id);
            if (!transactionAccount.IsActive)
            {
                return new Result { Success = false, Message = "Account does not exist" };
            }

            // check for existance of envelope
            var transactionEnvelope = await EnvelopeDataAccess.ReadEnvelopeAsync(transaction.Envelope.Id);
            if (!transaction.Envelope.IsGenericDebtEnvelope() && !transactionEnvelope.IsActive)
            {
                return new Result { Success = false, Message = "Envelope does not exist" };
            }

            return new Result { Success = true };
        }

        public async Task<Result<Transaction>> SaveTransactionAsync(Transaction transaction)
        {
            var validationResult = await ValidateTransactionAsync(transaction);
            if (!validationResult.Success)
            {
                return validationResult.ToResult<Transaction>();
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

        public async Task<Result<IReadOnlyList<Transaction>>> GetTransactionsFromSplitAsync(Guid splitId)
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await TransactionDataAccess.ReadSplitTransactionsAsync(splitId);

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result> SaveSplitTransactionAsync(IEnumerable<Guid> transactionIds)
        {
            var result = new Result();

            if (!transactionIds.Any())
            {
                result.Success = false;
                result.Message = "No transactions to split";
                return result;
            }

            if (transactionIds.Count() == 1)
            {
                return await RemoveTransactionFromSplitAsync(transactionIds.FirstOrDefault());
            }

            var splitId = Guid.NewGuid();
            var transactions = new List<Transaction>();
            try
            {
                foreach (var transactionId in transactionIds)
                {
                    var transaction = await TransactionDataAccess.ReadTransactionAsync(transactionId);
                    if (transaction.IsActive)
                    {
                        transactions.Add(transaction);
                    }
                }

                foreach (var transaction in transactions)
                {
                    if (transaction.IsSplit)
                    {
                        var relatedTransactions = await TransactionDataAccess.ReadSplitTransactionsAsync(transaction.SplitId.Value);
                        if (relatedTransactions.Count() == 2) //need to unsplit, need better logic here!!!
                        {
                            var relatedTransaction = relatedTransactions.FirstOrDefault(t => t.Id != transaction.Id);
                            relatedTransaction.SplitId = null;
                            relatedTransaction.ModifiedDateTime = DateTime.Now;
                            await TransactionDataAccess.UpdateTransactionAsync(relatedTransaction);
                        }
                    }

                    transaction.SplitId = splitId;
                    transaction.ModifiedDateTime = DateTime.Now;
                    await TransactionDataAccess.UpdateTransactionAsync(transaction);
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> RemoveTransactionFromSplitAsync(Guid transactionId)
        {
            var result = new Result();

            try
            {
                var transaction = await TransactionDataAccess.ReadTransactionAsync(transactionId);
                if (transaction.IsActive)
                {
                    transaction.SplitId = null;
                    transaction.ModifiedDateTime = DateTime.Now;
                    await TransactionDataAccess.UpdateTransactionAsync(transaction);
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = "Transaction does not exist";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }


            return result;
        }

        // handle logic to get transaction into usable state
        public async Task<Result<Transaction>> GetCorrectedTransaction(Transaction transaction)
        {
            var transactionToPopulate = transaction.DeepCopy();

            // transfer logic
            bool envelopeNotNeeded = false;
            if (transactionToPopulate.Account.IsActive)
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
            transaction.Payee.IsAccount = payeeAccount.IsActive;

            transaction.Envelope = await EnvelopeDataAccess.ReadEnvelopeAsync(transaction.Envelope.Id);

            transaction.Account = await AccountDataAccess.ReadAccountAsync(transaction.Account.Id);

            return transaction;
        }

        public IReadOnlyList<Transaction> CombineSplitTransactions(IEnumerable<Transaction> transactions)
        {
            var combinedTransactions = new List<Transaction>(transactions.Where(t => !t.IsSplit));

            var transactionGroups = transactions.Where(t => t.IsSplit).GroupBy(t2 => t2.SplitId);

            foreach (var transactionGroup in transactionGroups)
            {
                var combinedTransaction = transactionGroup.FirstOrDefault();
                combinedTransaction.Id = combinedTransaction.SplitId.Value;
                combinedTransaction.Amount = transactionGroup.Sum(t => t.Amount);

                if (!transactionGroup.All(t => t.Account.Id == combinedTransaction.Account.Id))
                {
                    combinedTransaction.Account = new Account() { Description = "Split" };
                }

                if (!transactionGroup.All(t => t.Envelope.Id == combinedTransaction.Envelope.Id))
                {
                    combinedTransaction.Envelope = new Envelope() { Description = "Split" };
                }

                if (!transactionGroup.All(t => t.Payee.Id == combinedTransaction.Payee.Id))
                {
                    combinedTransaction.Payee = new Payee() { Description = "Split" };
                }

                combinedTransactions.Add(combinedTransaction);
            }

            return combinedTransactions;
        }
    }
}
