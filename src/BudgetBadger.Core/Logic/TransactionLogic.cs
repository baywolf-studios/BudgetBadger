using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.Core.Logic
{
    public class TransactionLogic : ITransactionLogic
    {
        readonly IDataAccess _dataAccess;
        readonly IResourceContainer _resourceContainer;

        public TransactionLogic(IDataAccess dataAccess,
            IResourceContainer resourceContainer)
        {
            _dataAccess = dataAccess;
            _resourceContainer = resourceContainer;
        }

        public async Task<Result<Transaction>> SaveTransactionAsync(Transaction transaction)
        {
            var validationResult = await ValidateTransactionAsync(transaction).ConfigureAwait(false);
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

            if (transactionToUpsert.IsTransfer)
            {
                // business rule forces account to be the on budget one
                try
                {
                    var payeeAccount = await _dataAccess.ReadAccountAsync(transactionToUpsert.Payee.Id).ConfigureAwait(false);
                    if (transactionToUpsert.Account.OffBudget && payeeAccount.OnBudget)
                    {
                        transactionToUpsert.Payee = await _dataAccess.ReadPayeeAsync(transactionToUpsert.Account.Id).ConfigureAwait(false);
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
                    var accountDebtEnvelope = await _dataAccess.ReadEnvelopeAsync(transactionToUpsert.Account.Id).ConfigureAwait(false);
                    transactionToUpsert.Envelope = accountDebtEnvelope;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                    return result;
                }
            }

            transactionToUpsert.Amount = _resourceContainer.GetRoundedDecimal(transactionToUpsert.Amount);

            if (transactionToUpsert.IsNew)
            {
                transactionToUpsert.Id = Guid.NewGuid();
                transactionToUpsert.CreatedDateTime = dateTimeNow;
                transactionToUpsert.ModifiedDateTime = dateTimeNow;

                try
                {
                    await _dataAccess.CreateTransactionAsync(transactionToUpsert).ConfigureAwait(false);
                    result.Success = true;
                    result.Data = await GetPopulatedTransaction(transactionToUpsert);
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
                    await _dataAccess.UpdateTransactionAsync(transactionToUpsert).ConfigureAwait(false);
                    result.Success = true;
                    result.Data = await GetPopulatedTransaction(transactionToUpsert);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
            }

            return result;
        }

        public async Task<Result<int>> GetTransactionsCountAsync()
        {
            var result = new Result<int>();

            try
            {
                var count = await _dataAccess.GetTransactionsCountAsync();
                result.Success = true;
                result.Data = count;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Transaction>> GetTransactionAsync(Guid id)
        {
            var result = new Result<Transaction>();

            try
            {
                var transaction = await _dataAccess.ReadTransactionAsync(id).ConfigureAwait(false);
                var populatedTransaction = await GetPopulatedTransaction(transaction).ConfigureAwait(false);

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

        public async Task<Result<IReadOnlyList<Transaction>>> GetAccountTransactionsAsync(Account account)
        {
            var result = new Result<IReadOnlyList<Transaction>>();
            var transactions = new List<Transaction>();

            transactions.AddRange(await _dataAccess.ReadAccountTransactionsAsync(account.Id).ConfigureAwait(false));

            var payeeTransactions = await _dataAccess.ReadPayeeTransactionsAsync(account.Id).ConfigureAwait(false);

            foreach (var transaction in payeeTransactions)
            {
                transaction.Payee.Id = transaction.Account.Id;
                transaction.Account.Id = account.Id;
                transaction.Amount = -1 * transaction.Amount;
                transactions.Add(transaction);
            }

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            result.Success = true;
			result.Data = CombineAndSortSplitTransactions(await Task.WhenAll(tasks));

            return result;
        }

        public async Task<Result<IReadOnlyList<Transaction>>> GetEnvelopeTransactionsAsync(Envelope envelope)
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await _dataAccess.ReadEnvelopeTransactionsAsync(envelope.Id).ConfigureAwait(false);

			var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            result.Success = true;
			result.Data = CombineAndSortSplitTransactions(await Task.WhenAll(tasks));

            return result;
        }

        public async Task<Result<IReadOnlyList<Transaction>>> GetPayeeTransactionsAsync(Payee payee)
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await _dataAccess.ReadPayeeTransactionsAsync(payee.Id).ConfigureAwait(false);

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);
                     
            result.Success = true;
			result.Data = CombineAndSortSplitTransactions(await Task.WhenAll(tasks));

            return result;
        }

        public async Task<Result<IReadOnlyList<Transaction>>> GetTransactionsAsync()
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await _dataAccess.ReadTransactionsAsync().ConfigureAwait(false);

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            result.Success = true;
			result.Data = CombineAndSortSplitTransactions(await Task.WhenAll(tasks));

            return result;
        }

        public async Task<Result<Transaction>> SoftDeleteTransactionAsync(Guid id)
        {
            var result = new Result<Transaction>();

            try
            {
                var transactionToDelete = await _dataAccess.ReadTransactionAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (transactionToDelete.IsNew || transactionToDelete.IsDeleted)
                {
                    errors.Add(_resourceContainer.GetResourceString("TransactionDeleteInactiveError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                if (transactionToDelete.IsSplit)
                {
                    var relatedTransactions = await _dataAccess.ReadSplitTransactionsAsync(transactionToDelete.SplitId.Value).ConfigureAwait(false);
                    if (relatedTransactions.Count() == 2) //need to unsplit, need better logic here!!!
                    {
                        var relatedTransaction = relatedTransactions.FirstOrDefault(t => t.Id != transactionToDelete.Id);
                        relatedTransaction.SplitId = null;
                        relatedTransaction.ModifiedDateTime = DateTime.Now;
                        await _dataAccess.UpdateTransactionAsync(relatedTransaction).ConfigureAwait(false);
                    }
                }

                transactionToDelete.ModifiedDateTime = DateTime.Now;
                transactionToDelete.DeletedDateTime = DateTime.Now;
                transactionToDelete.SplitId = null;
                await _dataAccess.UpdateTransactionAsync(transactionToDelete).ConfigureAwait(false);
                result.Success = true;
                result.Data = await GetPopulatedTransaction(transactionToDelete).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> SaveSplitTransactionAsync(IEnumerable<Transaction> transactions)
        {
            var result = new Result();

            if (!transactions.Any())
            {
                result.Success = false;
                result.Message = _resourceContainer.GetResourceString("SplitTransactionValidTransactionsError");
                return result;
            }

            if (transactions.Count() == 1)
            {
                var transaction = transactions.Single();
                transaction.SplitId = null;
                return await SaveTransactionAsync(transaction).ConfigureAwait(false);
            }

            var splitId = Guid.NewGuid();
            try
            {
                // extra validation so we don't save one and then fail validation later
                var validationResults = await Task.WhenAll(transactions.Select(ValidateTransactionAsync)).ConfigureAwait(false);
                if (validationResults.Any(v => !v.Success))
                {
                    return new Result { Success = false, Message = string.Join(Environment.NewLine, validationResults.Select(m => m.Message)) };
                }

                foreach (var transaction in transactions)
                {
                    await CleanupRelatedSplitTransactions(transaction).ConfigureAwait(false);

                    transaction.SplitId = splitId;

                    var saveResult = await SaveTransactionAsync(transaction).ConfigureAwait(false);
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

        public async Task<Result<IReadOnlyList<Transaction>>> GetTransactionsFromSplitAsync(Guid splitId)
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await _dataAccess.ReadSplitTransactionsAsync(splitId).ConfigureAwait(false);

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            var transactionsToReturn = (await Task.WhenAll(tasks)).ToList();
            transactionsToReturn.Sort();

            result.Success = true;
            result.Data = transactionsToReturn;

            return result;
        }

        public async Task<Result> UpdateSplitTransactionPostedAsync(Guid splitId, bool posted)
        {
            var result = new Result();

            try
            {
                var transactions = await _dataAccess.ReadSplitTransactionsAsync(splitId).ConfigureAwait(false);

                foreach (var transaction in transactions)
                {
                    transaction.Posted = posted;
                    transaction.ModifiedDateTime = DateTime.Now;
                    await _dataAccess.UpdateTransactionAsync(transaction).ConfigureAwait(false);
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

        public bool FilterTransaction(Transaction transaction, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }

            if (transaction != null)
            {
                return transaction.Envelope.Description.ToLower().Contains(searchText.ToLower())
                    || transaction.Payee.Description.ToLower().Contains(searchText.ToLower())
                    || transaction.Account.Description.ToLower().Contains(searchText.ToLower())
                    || transaction.Amount.ToString().ToLower().Contains(searchText.ToLower())
                    || transaction.ServiceDate.ToString().ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

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
                    var payeeAccount = await _dataAccess.ReadAccountAsync(transactionToPopulate.Payee.Id).ConfigureAwait(false);
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
                transactionToPopulate.Envelope = await _dataAccess.ReadEnvelopeAsync(Constants.IgnoredEnvelope.Id);
                transactionToPopulate.Envelope.TranslateEnvelope(_resourceContainer);
            }
            else if (!envelopeNotNeeded && transactionToPopulate.Envelope.IsSystem)
            {
                transactionToPopulate.Envelope = new Envelope();
            }

            // handle logic to set envelope to the generic debt envelope
            if (transactionToPopulate.Envelope.Group.IsDebt && !transactionToPopulate.Envelope.IsGenericDebtEnvelope)
            {
                var debtAccount = await _dataAccess.ReadAccountAsync(transactionToPopulate.Envelope.Id).ConfigureAwait(false);
                transactionToPopulate.Account = debtAccount;
                transactionToPopulate.Envelope = await _dataAccess.ReadEnvelopeAsync(Constants.GenericDebtEnvelope.Id);
            }

            return new Result<Transaction> { Success = true, Data = transactionToPopulate };
        }


        async Task CleanupRelatedSplitTransactions(Transaction transaction)
        {
            if (transaction.IsSplit)
            {
                var relatedTransactions = await _dataAccess.ReadSplitTransactionsAsync(transaction.SplitId.Value).ConfigureAwait(false);
                if (relatedTransactions.Count() == 2) //need to unsplit, need better logic here!!!
                {
                    var relatedTransaction = relatedTransactions.FirstOrDefault(t => t.Id != transaction.Id);
                    relatedTransaction.SplitId = null;
                    relatedTransaction.ModifiedDateTime = DateTime.Now;
                    await _dataAccess.UpdateTransactionAsync(relatedTransaction).ConfigureAwait(false);
                }
            }
        }

        async Task<Transaction> GetPopulatedTransaction(Transaction transaction)
        {
            transaction.Payee = await _dataAccess.ReadPayeeAsync(transaction.Payee.Id).ConfigureAwait(false);
            var payeeAccount = await _dataAccess.ReadAccountAsync(transaction.Payee.Id).ConfigureAwait(false);
            transaction.Payee.IsAccount = !payeeAccount.IsNew;
            transaction.Payee.TranslatePayee(_resourceContainer);

            transaction.Envelope = await _dataAccess.ReadEnvelopeAsync(transaction.Envelope.Id).ConfigureAwait(false);
            transaction.Envelope.TranslateEnvelope(_resourceContainer);

            transaction.Account = await _dataAccess.ReadAccountAsync(transaction.Account.Id).ConfigureAwait(false);
            transaction.Account.TranslateAccount(_resourceContainer);

            return transaction;
        }

        IReadOnlyList<Transaction> CombineAndSortSplitTransactions(IEnumerable<Transaction> transactions)
        {
            var combinedTransactions = new List<Transaction>(transactions.Where(t => !t.IsSplit));

            var transactionGroups = transactions.Where(t => t.IsSplit).GroupBy(t2 => t2.SplitId);

            foreach (var transactionGroup in transactionGroups)
            {
                var combinedTransaction = transactionGroup.OrderBy(t => t.CreatedDateTime).FirstOrDefault();
                if (transactionGroup.Count() > 1)
                {
                    combinedTransaction.Id = combinedTransaction.SplitId.Value;
                    combinedTransaction.Amount = transactionGroup.Sum(t => t.Amount);

                    if (!transactionGroup.All(t => t.Account.Id == combinedTransaction.Account.Id))
                    {
                        combinedTransaction.Account = new Account() { Description = _resourceContainer.GetResourceString("Split") };
                    }

                    if (!transactionGroup.All(t => t.Envelope.Id == combinedTransaction.Envelope.Id))
                    {
                        combinedTransaction.Envelope = new Envelope() { Description = _resourceContainer.GetResourceString("Split") };
                    }

                    if (!transactionGroup.All(t => t.Payee.Id == combinedTransaction.Payee.Id))
                    {
                        combinedTransaction.Payee = new Payee() { Description = _resourceContainer.GetResourceString("Split") };
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

            combinedTransactions.Sort();

            return combinedTransactions;
        }

        async Task<Result> ValidateTransactionAsync(Transaction transaction)
        {
            var errors = new List<string>();

            if (!transaction.Amount.HasValue)
            {
                errors.Add(_resourceContainer.GetResourceString("TransactionValidAmountError"));
            }

            if (transaction.Payee == null)
            {
                errors.Add(_resourceContainer.GetResourceString("TransactionValidPayeeError"));
            }

            if (transaction.Account == null)
            {
                errors.Add(_resourceContainer.GetResourceString("TransactionValidAccountError"));
            }

            if (transaction.Envelope == null)
            {
                errors.Add(_resourceContainer.GetResourceString("TransactionValidEnvelopeError"));
            }

            if (!errors.Any())
            {
                // check for existance of payee
                var transactionPayee = await _dataAccess.ReadPayeeAsync(transaction.Payee.Id).ConfigureAwait(false);
                if (transactionPayee.IsNew)
                {
                    errors.Add(_resourceContainer.GetResourceString("TransactionValidPayeeExistError"));
                }

                // check for existance of account
                var transactionAccount = await _dataAccess.ReadAccountAsync(transaction.Account.Id).ConfigureAwait(false);
                if (transactionAccount.IsNew)
                {
                    errors.Add(_resourceContainer.GetResourceString("TransactionValidAccountExistError"));
                }

                // check for existance of envelope
                var transactionEnvelope = await _dataAccess.ReadEnvelopeAsync(transaction.Envelope.Id).ConfigureAwait(false);
                if (!transaction.Envelope.IsGenericDebtEnvelope && transactionEnvelope.IsNew)
                {
                    errors.Add(_resourceContainer.GetResourceString("TransactionValidEnvelopeExistError"));
                }

                var tempTransaction = await _dataAccess.ReadTransactionAsync(transaction.Id).ConfigureAwait(false);
                var existingTransaction = await GetPopulatedTransaction(tempTransaction).ConfigureAwait(false);

                if (existingTransaction.IsActive) // already exists need to compare
                {
                    if (transaction.Account.Id != existingTransaction.Account.Id)
                    {
                        if (transactionAccount.IsDeleted)
                        {
                            errors.Add(_resourceContainer.GetResourceString("TransactionValidAccountDeletedError"));
                        }

                        if (existingTransaction.Account.IsDeleted)
                        {
                            if (transaction.Amount != existingTransaction.Amount)
                            {
                                errors.Add(_resourceContainer.GetResourceString("TransactionValidAmountDeletedAccountError"));
                            }

                            if (transaction.ServiceDate != existingTransaction.ServiceDate)
                            {
                                errors.Add(_resourceContainer.GetResourceString("TransactionValidServiceDateDeletedAccountError"));
                            }

                            if (transaction.Account.Id != existingTransaction.Account.Id)
                            {
                                errors.Add(_resourceContainer.GetResourceString("TransactionValidAccountDeletedAccountError"));
                            }
                        }
                    }

                    if (transaction.Payee.Id != existingTransaction.Payee.Id)
                    {
                        if (transactionPayee.IsDeleted)
                        {
                            errors.Add(_resourceContainer.GetResourceString("TransactionValidPayeeDeletedError"));
                        }

                        if (existingTransaction.IsTransfer && existingTransaction.Payee.IsDeleted)
                        {
                            if (transaction.Amount != existingTransaction.Amount)
                            {
                                errors.Add(_resourceContainer.GetResourceString("TransactionValidAmountDeletedPayeeError"));
                            }

                            if (transaction.ServiceDate != existingTransaction.ServiceDate)
                            {
                                errors.Add(_resourceContainer.GetResourceString("TransactionValidServiceDateDeletedPayeeError"));
                            }

                            if (transaction.Payee.Id != existingTransaction.Payee.Id)
                            {
                                errors.Add(_resourceContainer.GetResourceString("TransactionValidPayeeDeletedPayeeError"));
                            }
                        }
                    }

                    if (transaction.Envelope.Id != existingTransaction.Envelope.Id)
                    {
                        if (transactionEnvelope.IsDeleted)
                        {
                            errors.Add(_resourceContainer.GetResourceString("TransactionValidEnvelopeDeletedError"));
                        }

                        if (existingTransaction.Envelope.IsDeleted)
                        {
                            if (transaction.Amount != existingTransaction.Amount)
                            {
                                errors.Add(_resourceContainer.GetResourceString("TransactionValidAmountDeletedEnvelopeError"));
                            }

                            if (transaction.ServiceDate != existingTransaction.ServiceDate)
                            {
                                errors.Add(_resourceContainer.GetResourceString("TransactionValidServiceDateDeletedEnvelopeError"));
                            }

                            if (transaction.Envelope.Id != existingTransaction.Envelope.Id)
                            {
                                errors.Add(_resourceContainer.GetResourceString("TransactionValidPayeeDeletedEnvelopeError"));
                            }
                        }
                    }
                }
                else
                {
                    if (transactionPayee.IsDeleted)
                    {
                        errors.Add(_resourceContainer.GetResourceString("TransactionValidPayeeDeletedError"));
                    }

                    if (transactionAccount.IsDeleted)
                    {
                        errors.Add(_resourceContainer.GetResourceString("TransactionValidAccountDeletedError"));
                    }

                    if (!transaction.Envelope.IsGenericDebtEnvelope && transactionEnvelope.IsDeleted)
                    {
                        errors.Add(_resourceContainer.GetResourceString("TransactionValidEnvelopeDeletedError"));
                    }
                }
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }

        //deleting these
        async Task<Result> ValidateDeleteTransactionAsync(Guid transactionId)
        {
            var errors = new List<string>();

            var tempTransaction = await _dataAccess.ReadTransactionAsync(transactionId).ConfigureAwait(false);
            var transaction = await GetPopulatedTransaction(tempTransaction).ConfigureAwait(false);

            if (transaction.IsTransfer && transaction.Payee.IsDeleted)
            {
                errors.Add(_resourceContainer.GetResourceString("TransactionDeleteTransferDeletedPayeeError"));
            }

            if (transaction.Account.IsDeleted)
            {
                errors.Add(_resourceContainer.GetResourceString("TransactionDeleteAccountDeletedError"));
            }

            if (transaction.Envelope.IsDeleted)
            {
                errors.Add(_resourceContainer.GetResourceString("TransactionDeleteEnvelopeDeletedError"));
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }

        public async Task<Result> DeleteTransactionAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var validationResult = await ValidateDeleteTransactionAsync(id).ConfigureAwait(false);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                var transactionToDelete = await _dataAccess.ReadTransactionAsync(id).ConfigureAwait(false);

                await CleanupRelatedSplitTransactions(transactionToDelete).ConfigureAwait(false);

                transactionToDelete.ModifiedDateTime = DateTime.Now;
                transactionToDelete.DeletedDateTime = DateTime.Now;
                await _dataAccess.UpdateTransactionAsync(transactionToDelete).ConfigureAwait(false);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
