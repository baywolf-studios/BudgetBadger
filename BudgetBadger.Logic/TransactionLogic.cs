using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.Logic
{
    public class TransactionLogic : ITransactionLogic
    {
        readonly ITransactionDataAccess _transactionDataAccess;
        readonly IAccountDataAccess _accountDataAccess;
        readonly IPayeeDataAccess _payeeDataAccess;
        readonly IEnvelopeDataAccess _envelopeDataAccess;
        readonly IResourceContainer _resouceContainer;

        public TransactionLogic(ITransactionDataAccess transactionDataAccess, 
            IAccountDataAccess accountDataAccess, 
            IPayeeDataAccess payeeDataAccess, 
            IEnvelopeDataAccess envelopeDataAccess,
            IResourceContainer resourceContainer)
        {
            _transactionDataAccess = transactionDataAccess;
            _accountDataAccess = accountDataAccess;
            _payeeDataAccess = payeeDataAccess;
            _envelopeDataAccess = envelopeDataAccess;
            _resouceContainer = resourceContainer;
        }

        async Task<Result> ValidateDeleteTransactionAsync(Guid transactionId)
        {
            var errors = new List<string>();

            var tempTransaction = await _transactionDataAccess.ReadTransactionAsync(transactionId).ConfigureAwait(false);
            var transaction = await GetPopulatedTransaction(tempTransaction).ConfigureAwait(false);

            if (transaction.IsTransfer && transaction.Payee.IsDeleted)
            {
                errors.Add(_resouceContainer.GetResourceString("TransactionDeleteTransferDeletedPayeeError"));
            }

            if (transaction.Account.IsDeleted)
            {
                errors.Add(_resouceContainer.GetResourceString("TransactionDeleteAccountDeletedError"));
            }

            if (transaction.Envelope.IsDeleted)
            {
                errors.Add(_resouceContainer.GetResourceString("TransactionDeleteEnvelopeDeletedError"));
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

                var transactionToDelete = await _transactionDataAccess.ReadTransactionAsync(id).ConfigureAwait(false);

                await CleanupRelatedSplitTransactions(transactionToDelete).ConfigureAwait(false);

                transactionToDelete.ModifiedDateTime = DateTime.Now;
                transactionToDelete.DeletedDateTime = DateTime.Now;
                await _transactionDataAccess.UpdateTransactionAsync(transactionToDelete).ConfigureAwait(false);
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

            transactions.AddRange(await _transactionDataAccess.ReadAccountTransactionsAsync(account.Id).ConfigureAwait(false));

            var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(account.Id).ConfigureAwait(false);

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

            var transactions = await _transactionDataAccess.ReadEnvelopeTransactionsAsync(envelope.Id).ConfigureAwait(false);

			var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            result.Success = true;
			result.Data = CombineAndSortSplitTransactions(await Task.WhenAll(tasks));

            return result;
        }

        public async Task<Result<IReadOnlyList<Transaction>>> GetPayeeTransactionsAsync(Payee payee)
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(payee.Id).ConfigureAwait(false);

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);
                     
            result.Success = true;
			result.Data = CombineAndSortSplitTransactions(await Task.WhenAll(tasks));

            return result;
        }

        public async Task<Result<Transaction>> GetTransactionAsync(Guid id)
        {
            var result = new Result<Transaction>();

            try
            {
                var transaction = await _transactionDataAccess.ReadTransactionAsync(id).ConfigureAwait(false);
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

        public async Task<Result<IReadOnlyList<Transaction>>> GetTransactionsAsync()
        {
            var result = new Result<IReadOnlyList<Transaction>>();

            var transactions = await _transactionDataAccess.ReadTransactionsAsync().ConfigureAwait(false);

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            result.Success = true;
			result.Data = CombineAndSortSplitTransactions(await Task.WhenAll(tasks));

            return result;
        }

        public bool FilterTransaction(Transaction transaction, string searchText)
        {
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

        public async Task<Result> ValidateTransactionAsync(Transaction transaction)
        {
            var errors = new List<string>();

            if (!transaction.Amount.HasValue)
            {
                errors.Add(_resouceContainer.GetResourceString("TransactionValidAmountError"));
            }

            if (transaction.Payee == null)
            {
                errors.Add(_resouceContainer.GetResourceString("TransactionValidPayeeError"));
            }

            if (transaction.Account == null)
            {
                errors.Add(_resouceContainer.GetResourceString("TransactionValidAccountError"));
            }

            if (transaction.Envelope == null)
            {
                errors.Add(_resouceContainer.GetResourceString("TransactionValidEnvelopeError"));
            }

            if (!errors.Any())
            {
                // check for existance of payee
                var transactionPayee = await _payeeDataAccess.ReadPayeeAsync(transaction.Payee.Id).ConfigureAwait(false);
                if (transactionPayee.IsNew)
                {
                    errors.Add(_resouceContainer.GetResourceString("TransactionValidPayeeExistError"));
                }

                // check for existance of account
                var transactionAccount = await _accountDataAccess.ReadAccountAsync(transaction.Account.Id).ConfigureAwait(false);
                if (transactionAccount.IsNew)
                {
                    errors.Add(_resouceContainer.GetResourceString("TransactionValidAccountExistError"));
                }

                // check for existance of envelope
                var transactionEnvelope = await _envelopeDataAccess.ReadEnvelopeAsync(transaction.Envelope.Id).ConfigureAwait(false);
                if (!transaction.Envelope.IsGenericDebtEnvelope && transactionEnvelope.IsNew)
                {
                    errors.Add(_resouceContainer.GetResourceString("TransactionValidEnvelopeExistError"));
                }

                var tempTransaction = await _transactionDataAccess.ReadTransactionAsync(transaction.Id).ConfigureAwait(false);
                var existingTransaction = await GetPopulatedTransaction(tempTransaction).ConfigureAwait(false);

                if (existingTransaction.IsActive) // already exists need to compare
                {
                    if (transaction.Account.Id != existingTransaction.Account.Id)
                    {
                        if (transactionAccount.IsDeleted)
                        {
                            errors.Add(_resouceContainer.GetResourceString("TransactionValidAccountDeletedError"));
                        }

                        if (existingTransaction.Account.IsDeleted)
                        {
                            if (transaction.Amount != existingTransaction.Amount)
                            {
                                errors.Add(_resouceContainer.GetResourceString("TransactionValidAmountDeletedAccountError"));
                            }

                            if (transaction.ServiceDate != existingTransaction.ServiceDate)
                            {
                                errors.Add(_resouceContainer.GetResourceString("TransactionValidServiceDateDeletedAccountError"));
                            }

                            if (transaction.Account.Id != existingTransaction.Account.Id)
                            {
                                errors.Add(_resouceContainer.GetResourceString("TransactionValidAccountDeletedAccountError"));
                            }
                        }
                    }

                    if (transaction.Payee.Id != existingTransaction.Payee.Id)
                    {
                        if (transactionPayee.IsDeleted)
                        {
                            errors.Add(_resouceContainer.GetResourceString("TransactionValidPayeeDeletedError"));
                        }

                        if (existingTransaction.IsTransfer && existingTransaction.Payee.IsDeleted)
                        {
                            if (transaction.Amount != existingTransaction.Amount)
                            {
                                errors.Add(_resouceContainer.GetResourceString("TransactionValidAmountDeletedPayeeError"));
                            }

                            if (transaction.ServiceDate != existingTransaction.ServiceDate)
                            {
                                errors.Add(_resouceContainer.GetResourceString("TransactionValidServiceDateDeletedPayeeError"));
                            }

                            if (transaction.Payee.Id != existingTransaction.Payee.Id)
                            {
                                errors.Add(_resouceContainer.GetResourceString("TransactionValidPayeeDeletedPayeeError"));
                            }
                        }
                    }

                    if (transaction.Envelope.Id != existingTransaction.Envelope.Id)
                    {
                        if (transactionEnvelope.IsDeleted)
                        {
                            errors.Add(_resouceContainer.GetResourceString("TransactionValidEnvelopeDeletedError"));
                        }

                        if (existingTransaction.Envelope.IsDeleted)
                        {
                            if (transaction.Amount != existingTransaction.Amount)
                            {
                                errors.Add(_resouceContainer.GetResourceString("TransactionValidAmountDeletedEnvelopeError"));
                            }

                            if (transaction.ServiceDate != existingTransaction.ServiceDate)
                            {
                                errors.Add(_resouceContainer.GetResourceString("TransactionValidServiceDateDeletedEnvelopeError"));
                            }

                            if (transaction.Envelope.Id != existingTransaction.Envelope.Id)
                            {
                                errors.Add(_resouceContainer.GetResourceString("TransactionValidPayeeDeletedEnvelopeError"));
                            }
                        }
                    }
                }
                else
                {
                    if (transactionPayee.IsDeleted)
                    {
                        errors.Add(_resouceContainer.GetResourceString("TransactionValidPayeeDeletedError"));
                    }

                    if (transactionAccount.IsDeleted)
                    {
                        errors.Add(_resouceContainer.GetResourceString("TransactionValidAccountDeletedError"));
                    }

                    if (!transaction.Envelope.IsGenericDebtEnvelope && transactionEnvelope.IsDeleted)
                    {
                        errors.Add(_resouceContainer.GetResourceString("TransactionValidEnvelopeDeletedError"));
                    }
                }
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
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

            if(transactionToUpsert.IsTransfer)
            {
                // business rule forces account to be the on budget one
                try
                {
                    var payeeAccount = await _accountDataAccess.ReadAccountAsync(transactionToUpsert.Payee.Id).ConfigureAwait(false);
                    if (transactionToUpsert.Account.OffBudget && payeeAccount.OnBudget)
                    {
                        transactionToUpsert.Payee = await _payeeDataAccess.ReadPayeeAsync(transactionToUpsert.Account.Id).ConfigureAwait(false);
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
                    var accountDebtEnvelope = await _envelopeDataAccess.ReadEnvelopeAsync(transactionToUpsert.Account.Id).ConfigureAwait(false);
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
                    await _transactionDataAccess.CreateTransactionAsync(transactionToUpsert).ConfigureAwait(false);
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
                    await _transactionDataAccess.UpdateTransactionAsync(transactionToUpsert).ConfigureAwait(false);
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

            var transactions = await _transactionDataAccess.ReadSplitTransactionsAsync(splitId).ConfigureAwait(false);

            var tasks = transactions.Where(t => t.IsActive).Select(GetPopulatedTransaction);

            var transactionsToReturn = (await Task.WhenAll(tasks)).ToList();
            transactionsToReturn.Sort();

            result.Success = true;
            result.Data = transactionsToReturn;

            return result;
        }

        public async Task<Result> SaveSplitTransactionAsync(IEnumerable<Transaction> transactions)
        {
            var result = new Result();

            if (!transactions.Any())
            {
                result.Success = false;
                result.Message = _resouceContainer.GetResourceString("SplitTransactionValidTransactionsError");
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

        public async Task<Result> UpdateSplitTransactionPostedAsync(Guid splitId, bool posted)
        {
            var result = new Result();

            try
            {
                var transactions = await _transactionDataAccess.ReadSplitTransactionsAsync(splitId).ConfigureAwait(false);

                foreach (var transaction in transactions)
                {
                    transaction.Posted = posted;
                    transaction.ModifiedDateTime = DateTime.Now;
                    await _transactionDataAccess.UpdateTransactionAsync(transaction).ConfigureAwait(false);
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
                var relatedTransactions = await _transactionDataAccess.ReadSplitTransactionsAsync(transaction.SplitId.Value).ConfigureAwait(false);
                if (relatedTransactions.Count() == 2) //need to unsplit, need better logic here!!!
                {
                    var relatedTransaction = relatedTransactions.FirstOrDefault(t => t.Id != transaction.Id);
                    relatedTransaction.SplitId = null;
                    relatedTransaction.ModifiedDateTime = DateTime.Now;
                    await _transactionDataAccess.UpdateTransactionAsync(relatedTransaction).ConfigureAwait(false);
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
                    var payeeAccount = await _accountDataAccess.ReadAccountAsync(transactionToPopulate.Payee.Id).ConfigureAwait(false);
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
                var debtAccount = await _accountDataAccess.ReadAccountAsync(transactionToPopulate.Envelope.Id).ConfigureAwait(false);
                transactionToPopulate.Account = debtAccount;
                transactionToPopulate.Envelope = Constants.GenericDebtEnvelope;
            }

            return new Result<Transaction> { Success = true, Data = transactionToPopulate };
        }

        public async Task<Transaction> GetPopulatedTransaction(Transaction transaction)
        {
            transaction.Payee = await _payeeDataAccess.ReadPayeeAsync(transaction.Payee.Id).ConfigureAwait(false);
            var payeeAccount = await _accountDataAccess.ReadAccountAsync(transaction.Payee.Id).ConfigureAwait(false);
            transaction.Payee.IsAccount = payeeAccount.IsActive;

            transaction.Envelope = await _envelopeDataAccess.ReadEnvelopeAsync(transaction.Envelope.Id).ConfigureAwait(false);

            transaction.Account = await _accountDataAccess.ReadAccountAsync(transaction.Account.Id).ConfigureAwait(false);

            return transaction;
        }

        public IReadOnlyList<Transaction> CombineAndSortSplitTransactions(IEnumerable<Transaction> transactions)
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
                        combinedTransaction.Account = new Account() { Description = _resouceContainer.GetResourceString("Split") };
                    }

                    if (!transactionGroup.All(t => t.Envelope.Id == combinedTransaction.Envelope.Id))
                    {
                        combinedTransaction.Envelope = new Envelope() { Description = _resouceContainer.GetResourceString("Split") };
                    }

                    if (!transactionGroup.All(t => t.Payee.Id == combinedTransaction.Payee.Id))
                    {
                        combinedTransaction.Payee = new Payee() { Description = _resouceContainer.GetResourceString("Split") };
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
    }
}
