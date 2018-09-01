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

        async Task<Result> ValidateDeleteTransactionAsync(Guid transactionId)
        {
            var errors = new List<string>();

            var tempTransaction = await TransactionDataAccess.ReadTransactionAsync(transactionId);
            var transaction = await GetPopulatedTransaction(tempTransaction);

            if (transaction.IsTransfer && transaction.Payee.IsDeleted)
            {
                errors.Add("Cannot delete transfer transaction with deleted payee");
            }

            if (transaction.Account.IsDeleted)
            {
                errors.Add("Cannot delete transaction with deleted account");
            }

            if (transaction.Envelope.IsDeleted)
            {
                errors.Add("Cannot delete transaction with deleted envelope");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }

        public async Task<Result> DeleteTransactionAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var validationResult = await ValidateDeleteTransactionAsync(id);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                var transactionToDelete = await TransactionDataAccess.ReadTransactionAsync(id);

                await CleanupRelatedSplitTransactions(transactionToDelete);

                transactionToDelete.ModifiedDateTime = DateTime.Now;
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

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            result.Success = true;
			result.Data = OrderTransactions(CombineSplitTransactions(await Task.WhenAll(tasks)));

            return result;
        }

        public async Task<Result<IReadOnlyList<Transaction>>> GetEnvelopeTransactionsAsync(Envelope envelope)
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await TransactionDataAccess.ReadEnvelopeTransactionsAsync(envelope.Id);

			var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            result.Success = true;
			result.Data = OrderTransactions(CombineSplitTransactions(await Task.WhenAll(tasks)));

            return result;
        }

        public async Task<Result<IReadOnlyList<Transaction>>> GetPayeeTransactionsAsync(Payee payee)
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await TransactionDataAccess.ReadPayeeTransactionsAsync(payee.Id);

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);
                     
            result.Success = true;
			result.Data = OrderTransactions(CombineSplitTransactions(await Task.WhenAll(tasks)));

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

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            result.Success = true;
			result.Data = OrderTransactions(CombineSplitTransactions(await Task.WhenAll(tasks)));

            return result;
        }

        public bool FilterTransaction(Transaction transaction, string searchText)
        {
            if (transaction != null)
            {
                return transaction.Envelope.Description.ToLower().Contains(searchText.ToLower())
                    || transaction.Payee.Description.ToLower().Contains(searchText.ToLower())
                    || transaction.Account.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        public IReadOnlyList<Transaction> SearchTransactions(IEnumerable<Transaction> transactions, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return transactions.ToList();
            }

            var searchResults = transactions.Where(t => FilterTransaction(t, searchText));

			return OrderTransactions(searchResults);
        }

		public IReadOnlyList<Transaction> OrderTransactions(IEnumerable<Transaction> transactions)
        {
			return transactions.OrderByDescending(a => a.ServiceDate).ToList();
        }

        public IReadOnlyList<IGrouping<string, Transaction>> GroupTransactions(IEnumerable<Transaction> transactions)
        {
			var groupedTransactions = OrderTransactions(transactions).GroupBy(t => String.Format("{0:d}", t.ServiceDate));

			var groupedAndOrderedTransactions = groupedTransactions.OrderByDescending(g => g.FirstOrDefault().ServiceDate).ToList();

			return groupedAndOrderedTransactions;
        }

        public async Task<Result> ValidateTransactionAsync(Transaction transaction)
        {
            if (!transaction.IsValid())
            {
                return transaction.Validate();
            }

            var errors = new List<string>();

            var transactionPayee = await PayeeDataAccess.ReadPayeeAsync(transaction.Payee.Id);
            // check for existance of payee
            if (transactionPayee.IsNew)
            {
                errors.Add("Payee does not exist");
            }

            // check for existance of account
            var transactionAccount = await AccountDataAccess.ReadAccountAsync(transaction.Account.Id);
            if (transactionAccount.IsNew)
            {
                errors.Add("Account does not exist");
            }

            // check for existance of envelope
            var transactionEnvelope = await EnvelopeDataAccess.ReadEnvelopeAsync(transaction.Envelope.Id);
            if (!transaction.Envelope.IsGenericDebtEnvelope && transactionEnvelope.IsNew)
            {
                errors.Add("Envelope does not exist");
            }

            var tempTransaction = await TransactionDataAccess.ReadTransactionAsync(transaction.Id);
            var existingTransaction = await GetPopulatedTransaction(tempTransaction);

            if (existingTransaction.IsActive) // already exists need to compare
            {
                if (transaction.Account.Id != existingTransaction.Account.Id)
                {
                    if (transactionAccount.IsDeleted)
                    {
                        errors.Add("Cannot use a deleted Account");
                    }

                    if (existingTransaction.Account.IsDeleted)
                    {
                        if (transaction.Amount != existingTransaction.Amount)
                        {
                            errors.Add("Cannot edit the Amount on a transaction with a deleted Account");
                        }

                        if (transaction.ServiceDate != existingTransaction.ServiceDate)
                        {
                            errors.Add("Cannot edit the Service Date on a transaction with a deleted Account");
                        }

                        if (transaction.Account.Id != existingTransaction.Account.Id)
                        {
                            errors.Add("Cannot edit the Account on a transaction with a deleted Account");
                        }
                    }
                }

                if (transaction.Payee.Id != existingTransaction.Payee.Id)
                {
                    if (transactionPayee.IsDeleted)
                    {
                        errors.Add("Cannot use a deleted Payee");
                    }

                    if (existingTransaction.IsTransfer && existingTransaction.Payee.IsDeleted)
                    {
                        if (transaction.Amount != existingTransaction.Amount)
                        {
                            errors.Add("Cannot edit the Amount on a transaction with a deleted Payee");
                        }

                        if (transaction.ServiceDate != existingTransaction.ServiceDate)
                        {
                            errors.Add("Cannot edit the Service Date on a transaction with a deleted Payee");
                        }

                        if (transaction.Payee.Id != existingTransaction.Payee.Id)
                        {
                            errors.Add("Cannot edit the Payee on a transaction with a deleted Payee");
                        }
                    }
                }

                if (transaction.Envelope.Id != existingTransaction.Envelope.Id)
                {
                    if (transactionEnvelope.IsDeleted)
                    {
                        errors.Add("Cannot use a deleted Envelope");
                    }

                    if (existingTransaction.Envelope.IsDeleted)
                    {
                        if (transaction.Amount != existingTransaction.Amount)
                        {
                            errors.Add("Cannot edit the Amount on a transaction with a deleted Envelope");
                        }

                        if (transaction.ServiceDate != existingTransaction.ServiceDate)
                        {
                            errors.Add("Cannot edit the Service Date on a transaction with a deleted Envelope");
                        }

                        if (transaction.Envelope.Id != existingTransaction.Envelope.Id)
                        {
                            errors.Add("Cannot edit the Envelope on a transaction with a deleted Envelope");
                        }
                    }
                }
            }
            else
            {
                if (transactionPayee.IsDeleted)
                {
                    errors.Add( "Payee is deleted");
                }

                if (transactionAccount.IsDeleted)
                {
                    errors.Add("Account is deleted");
                }

                if (!transaction.Envelope.IsGenericDebtEnvelope && transactionEnvelope.IsDeleted)
                {
                    errors.Add("Envelope is deleted");
                }
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
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

            // removes the transaction from being reconciled if changed
            if (transactionToUpsert.Reconciled)
            {
                transactionToUpsert.ReconciledDateTime = null;
            }

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

            if (transactionToUpsert.Envelope.IsGenericDebtEnvelope)
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
			result.Data = OrderTransactions(await Task.WhenAll(tasks));

            return result;
        }

        public async Task<Result> SaveSplitTransactionAsync(IEnumerable<Transaction> transactions)
        {
            var result = new Result();

            if (!transactions.Any())
            {
                result.Success = false;
                result.Message = "No split transactions to save";
                return result;
            }

            if (transactions.Count() == 1)
            {
                var transaction = transactions.Single();
                transaction.SplitId = null;
                return await SaveTransactionAsync(transaction);
            }

            var splitId = Guid.NewGuid();
            try
            {
                // extra validation so we don't save one and then fail validation later
                var validationResults = await Task.WhenAll(transactions.Select(ValidateTransactionAsync));
                if (validationResults.Any(v => !v.Success))
                {
                    return new Result { Success = false, Message = string.Join(Environment.NewLine, validationResults.Select(m => m.Message)) };
                }

                foreach (var transaction in transactions)
                {
                    await CleanupRelatedSplitTransactions(transaction);

                    transaction.SplitId = splitId;

                    var saveResult = await SaveTransactionAsync(transaction);
                    if (!saveResult.Success)
                    {
                        return saveResult;
                    }
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

        public async Task<Result> UpdateSplitTransactionPostedAsync(Guid splitId, bool posted)
        {
            var result = new Result();

            try
            {
                var transactions = await TransactionDataAccess.ReadSplitTransactionsAsync(splitId);

                foreach (var transaction in transactions)
                {
                    transaction.Posted = posted;
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

        async Task CleanupRelatedSplitTransactions(Transaction transaction)
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

            if (envelopeNotNeeded && !transactionToPopulate.Envelope.IsSystem)
            {
                transactionToPopulate.Envelope = Constants.IgnoredEnvelope;
            }
            else if (!envelopeNotNeeded && transactionToPopulate.Envelope.IsSystem)
            {
                transactionToPopulate.Envelope = new Envelope();
            }

            // handle logic to set envelope to the generic debt envelope
            if (transactionToPopulate.Envelope.Group.IsDebt && !transactionToPopulate.Envelope.IsGenericDebtEnvelope)
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
                if (transactionGroup.Count() > 1)
                {
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

                    if (transactionGroup.All(t => t.Posted))
                    {
                        combinedTransaction.Posted = true;
                    }
                    else
                    {
                        combinedTransaction.Posted = false;
                    }
                }

                combinedTransactions.Add(combinedTransaction);
            }

            return combinedTransactions;
        }
    }
}
