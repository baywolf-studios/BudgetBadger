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
    public class AccountLogic : IAccountLogic
    {
        readonly IAccountDataAccess _accountDataAccess;
        readonly ITransactionDataAccess _transactionDataAccess;
        readonly IPayeeDataAccess _payeeDataAccess;
        readonly IEnvelopeDataAccess _envelopeDataAccess;
        readonly IResourceContainer _resourceContainer;

        public AccountLogic(IAccountDataAccess accountDataAccess,
                            ITransactionDataAccess transactionDataAccess,
                            IPayeeDataAccess payeeDataAccess,
                            IEnvelopeDataAccess envelopeDataAccess,
                            IResourceContainer resourceContainer)
        {
            _accountDataAccess = accountDataAccess;
            _transactionDataAccess = transactionDataAccess;
            _payeeDataAccess = payeeDataAccess;
            _envelopeDataAccess = envelopeDataAccess;
            _resourceContainer = resourceContainer;
        }


        public async Task<Result<Account>> SaveAccountAsync(Account account)
        {
            var validationResult = await ValidateAccountAsync(account).ConfigureAwait(false);

            if (!validationResult.Success)
            {
                return validationResult.ToResult<Account>();
            }

            var accountToUpsert = account.DeepCopy();
            var dateTimeNow = DateTime.Now;

            if (accountToUpsert.IsNew)
            {
                accountToUpsert.Id = Guid.NewGuid();
                accountToUpsert.CreatedDateTime = dateTimeNow;
                accountToUpsert.ModifiedDateTime = dateTimeNow;
                await _accountDataAccess.CreateAccountAsync(accountToUpsert).ConfigureAwait(false);

                var accountPayee = new Payee
                {
                    Id = accountToUpsert.Id,
                    Description = accountToUpsert.Description,
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow
                };
                await _payeeDataAccess.CreatePayeeAsync(accountPayee).ConfigureAwait(false);

                //create a debt envelope for new accounts
                var debtEnvelopeGroup = await _envelopeDataAccess.ReadEnvelopeGroupAsync(Constants.DebtEnvelopeGroup.Id);
                var debtEnvelope = new Envelope
                {
                    Id = accountToUpsert.Id,
                    Description = accountToUpsert.Description,
                    IgnoreOverspend = true,
                    Group = debtEnvelopeGroup,
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow
                };
                await _envelopeDataAccess.CreateEnvelopeAsync(debtEnvelope).ConfigureAwait(false);

                // determine which envelope should be used
                Envelope startingBalanceEnvelope;
                if (accountToUpsert.OffBudget)
                {
                    startingBalanceEnvelope = await _envelopeDataAccess.ReadEnvelopeAsync(Constants.IgnoredEnvelope.Id);
                }
                else if (accountToUpsert.Balance < 0) // on budget and negative
                {
                    startingBalanceEnvelope = debtEnvelope;
                }
                else // on budget and positive
                {
                    startingBalanceEnvelope = await _envelopeDataAccess.ReadEnvelopeAsync(Constants.IncomeEnvelope.Id);
                }

                var startingBalance = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow,
                    Amount = accountToUpsert.Balance,
                    ServiceDate = dateTimeNow,
                    Posted = true,
                    Account = accountToUpsert,
                    Payee = await _payeeDataAccess.ReadPayeeAsync(Constants.StartingBalancePayee.Id),
                    Envelope = startingBalanceEnvelope
                };

                await _transactionDataAccess.CreateTransactionAsync(startingBalance).ConfigureAwait(false);
            }
            else
            {
                // update linked payee
                var accountPayee = await _payeeDataAccess.ReadPayeeAsync(accountToUpsert.Id).ConfigureAwait(false);
                accountPayee.ModifiedDateTime = dateTimeNow;
                accountPayee.Description = accountToUpsert.Description;
                await _payeeDataAccess.UpdatePayeeAsync(accountPayee);

                // update the debt envelope name
                var debtEnvelope = await _envelopeDataAccess.ReadEnvelopeAsync(accountToUpsert.Id).ConfigureAwait(false);
                debtEnvelope.ModifiedDateTime = dateTimeNow;
                debtEnvelope.Description = accountToUpsert.Description;
                await _envelopeDataAccess.UpdateEnvelopeAsync(debtEnvelope);

                accountToUpsert.ModifiedDateTime = dateTimeNow;
                await _accountDataAccess.UpdateAccountAsync(accountToUpsert).ConfigureAwait(false);
            }

            return new Result<Account> { Success = true, Data = accountToUpsert };
        }

        public async Task<Result<int>> GetAccountsCountAsync()
        {
            var result = new Result<int>();

            try
            {
                var count = await _accountDataAccess.GetAccountsCountAsync();
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

        public async Task<Result<Account>> GetAccountAsync(Guid id)
        {
            var result = new Result<Account>();

            try
            {
                var account = await _accountDataAccess.ReadAccountAsync(id).ConfigureAwait(false);
                var populatedAccount = await GetPopulatedAccount(account).ConfigureAwait(false);
                result.Success = true;
                result.Data = populatedAccount;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Account>>> GetAccountsAsync()
        {
            var result = new Result<IReadOnlyList<Account>>();

            var allAccounts = await _accountDataAccess.ReadAccountsAsync().ConfigureAwait(false);
            var activeAndHiddenAccounts = allAccounts.Where(a => a.IsActive || a.IsHidden);

            var tasks = activeAndHiddenAccounts.Select(GetPopulatedAccount);
            var populatedAccounts = (await Task.WhenAll(tasks)).ToList();

            var accountsToReturn = populatedAccounts.Where(p => p.IsActive).ToList();

            if (populatedAccounts.Any(a => a.IsHidden))
            {
                var hiddenAccounts = populatedAccounts.Where(a => a.IsHidden);

                var genericHiddenAccount = PopulateGenericHiddenAccount(hiddenAccounts);
                accountsToReturn.Add(genericHiddenAccount);
            }

            accountsToReturn.Sort();

            result.Success = true;
            result.Data = accountsToReturn;

            return result;
        }

        public async Task<Result<IReadOnlyList<Account>>> GetAccountsForSelectionAsync()
        {
            var result = new Result<IReadOnlyList<Account>>();

            var allAccounts = await _accountDataAccess.ReadAccountsAsync().ConfigureAwait(false);

            var accounts = allAccounts.Where(a => a.IsActive);

            var tasks = accounts.Select(GetPopulatedAccount);

            var accountsToReturn = (await Task.WhenAll(tasks)).ToList();
            accountsToReturn.Sort();

            result.Success = true;
            result.Data = accountsToReturn;

            return result;
        }

        public async Task<Result<IReadOnlyList<Account>>> GetHiddenAccountsAsync()
        {
            var result = new Result<IReadOnlyList<Account>>();

            var allAccounts = await _accountDataAccess.ReadAccountsAsync().ConfigureAwait(false);

            var accounts = allAccounts.Where(a => a.IsHidden);

            var tasks = accounts.Select(GetPopulatedAccount);

            var accountsToReturn = (await Task.WhenAll(tasks)).ToList();
            accountsToReturn.Sort();

            result.Success = true;
            result.Data = accountsToReturn;

            return result;
        }

        public async Task<Result> SoftDeleteAccountAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var account = await _accountDataAccess.ReadAccountAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (account.IsNew)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountDeleteNewError"));
                }

                if (account.IsDeleted)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountDeleteDeletedError"));
                }

                if (account.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountDeleteActiveError"));
                }

                var accountTransactions = await _transactionDataAccess.ReadAccountTransactionsAsync(id).ConfigureAwait(false);
                var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(id).ConfigureAwait(false);

                if (accountTransactions.Any(t => t.IsActive) || payeeTransactions.Any(t => t.IsActive))
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountDeleteActiveTransactionsError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                var now = DateTime.Now;

                var payee = await _payeeDataAccess.ReadPayeeAsync(account.Id).ConfigureAwait(false);
                payee.DeletedDateTime = now;
                payee.ModifiedDateTime = now;
                await _payeeDataAccess.UpdatePayeeAsync(payee).ConfigureAwait(false);

                var envelope = await _envelopeDataAccess.ReadEnvelopeAsync(account.Id).ConfigureAwait(false);
                envelope.DeletedDateTime = now;
                envelope.ModifiedDateTime = now;
                await _envelopeDataAccess.UpdateEnvelopeAsync(envelope).ConfigureAwait(false);

                account.DeletedDateTime = now;
                account.ModifiedDateTime = now;
                await _accountDataAccess.UpdateAccountAsync(account).ConfigureAwait(false);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> HideAccountAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var account = await _accountDataAccess.ReadAccountAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (!account.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountHideInactiveError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                var now = DateTime.Now;

                var payee = await _payeeDataAccess.ReadPayeeAsync(account.Id).ConfigureAwait(false);
                payee.HiddenDateTime = now;
                payee.ModifiedDateTime = now;
                await _payeeDataAccess.UpdatePayeeAsync(payee).ConfigureAwait(false);

                var envelope = await _envelopeDataAccess.ReadEnvelopeAsync(account.Id).ConfigureAwait(false);
                envelope.HiddenDateTime = now;
                envelope.ModifiedDateTime = now;
                await _envelopeDataAccess.UpdateEnvelopeAsync(envelope).ConfigureAwait(false);

                account.HiddenDateTime = now;
                account.ModifiedDateTime = now;
                await _accountDataAccess.UpdateAccountAsync(account).ConfigureAwait(false);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> UnhideAccountAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var account = await _accountDataAccess.ReadAccountAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (account.IsNew)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountUnhideNewError"));
                }

                if (account.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountUnhideActiveError"));
                }

                if (account.IsDeleted)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountUnhideDeletedError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                account.HiddenDateTime = null;
                account.ModifiedDateTime = DateTime.Now;

                await _accountDataAccess.UpdateAccountAsync(account);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> ReconcileAccount(Guid accountId, DateTime dateTime, decimal amount)
        {
            var now = DateTime.Now;
            var result = new Result();

            try
            {
                var accountTransactions = await _transactionDataAccess.ReadAccountTransactionsAsync(accountId).ConfigureAwait(false);
                var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(accountId).ConfigureAwait(false);
                var accountTransactionsToReconcile = accountTransactions.Where(t => t.IsActive
                                                                       && t.ServiceDate <= dateTime
                                                                       && t.Posted);

                var payeeTransactionsToReconcile = payeeTransactions.Where(t => t.IsActive
                                                                           && t.ServiceDate <= dateTime
                                                                           && t.Posted);

                var accountTransactionsSum = accountTransactionsToReconcile.Sum(t => t.Amount ?? 0);
                var payeeTransactionsSum = -1 * payeeTransactionsToReconcile.Sum(t => t.Amount ?? 0);

                if ((accountTransactionsSum + payeeTransactionsSum) == amount)
                {
                    var tasks = new List<Task>();
                    foreach (var transaction in accountTransactionsToReconcile)
                    {
                        transaction.Posted = true;
                        transaction.ModifiedDateTime = now;
                        transaction.ReconciledDateTime = transaction.ReconciledDateTime ?? dateTime;
                        tasks.Add(_transactionDataAccess.UpdateTransactionAsync(transaction));
                    }

                    foreach (var transaction in payeeTransactionsToReconcile)
                    {
                        transaction.Posted = true;
                        transaction.ModifiedDateTime = now;
                        transaction.ReconciledDateTime = transaction.ReconciledDateTime ?? dateTime;
                        tasks.Add(_transactionDataAccess.UpdateTransactionAsync(transaction));
                    }

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = _resourceContainer.GetResourceString("AccountReconcileAmountsDoNotMatchError");
                }

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public bool FilterAccount(Account account, string searchText)
        {
            if (account != null)
            {
                return account.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        async Task<Account> GetPopulatedAccount(Account account)
        {

            var accountTransactions = await _transactionDataAccess.ReadAccountTransactionsAsync(account.Id).ConfigureAwait(false);
            var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(account.Id).ConfigureAwait(false);
            var accountDebtBudgets = await _envelopeDataAccess.ReadBudgetsFromEnvelopeAsync(account.Id).ConfigureAwait(false);
            var debtTransactions = await _transactionDataAccess.ReadEnvelopeTransactionsAsync(account.Id).ConfigureAwait(false);

            return await Task.Run(() => PopulateAccount(account, accountTransactions, payeeTransactions, accountDebtBudgets, debtTransactions));
        }

        Account PopulateAccount(Account account,
                                IEnumerable<Transaction> accountTransactions,
                                IEnumerable<Transaction> payeeTransactions,
                                IEnumerable<Budget> accountDebtBudgets,
                                IEnumerable<Transaction> accountDebtTransactions)
        {
            var activeAccountTransactions = accountTransactions.Where(t => t.IsActive);
            var activePayeeTransactions = payeeTransactions.Where(t => t.IsActive);

            // pending
            account.Pending = activeAccountTransactions.Where(a => a.Pending).Sum(t => t.Amount ?? 0);
            account.Pending -= activePayeeTransactions.Where(t => t.Pending).Sum(t => t.Amount ?? 0);

            // posted
            account.Posted = activeAccountTransactions.Where(a => a.Posted).Sum(t => t.Amount ?? 0);
            account.Posted -= activePayeeTransactions.Where(t => t.Posted).Sum(t => t.Amount ?? 0);

            // balance
            account.Balance = activeAccountTransactions.Sum(t => t.Amount ?? 0);
            account.Balance -= activePayeeTransactions.Sum(t => t.Amount ?? 0);

            // payment 
            var dateTimeNow = DateTime.Now;

            var amountBudgetedToPayDownDebt = accountDebtBudgets
                .Where(a => a.Schedule.BeginDate <= dateTimeNow)
                .Sum(a => a.Amount);

            var debtTransactionAmount = accountDebtTransactions
                .Where(d => d.IsActive && d.ServiceDate <= dateTimeNow)
                .Sum(d => d.Amount ?? 0);

            account.Payment = amountBudgetedToPayDownDebt + debtTransactionAmount - account.Balance ?? 0;

            // group
            if (account.IsHidden)
            {
                account.Group = _resourceContainer.GetResourceString("Hidden");
            }
            else
            {
                account.Group = _resourceContainer.GetResourceString(Enum.GetName(typeof(AccountType), account.Type));
            }

            if (account.IsGenericHiddenAccount)
            {
                account.Description = _resourceContainer.GetResourceString(nameof(Constants.GenericHiddenAccount));
            }

            return account;
        }

        Task<Result> ValidateAccountAsync(Account account)
        {
            var errors = new List<string>();

            if (account.IsNew && !account.Balance.HasValue)
            {
                errors.Add(_resourceContainer.GetResourceString("AccountValidBalanceError"));
            }

            if (string.IsNullOrEmpty(account.Description))
            {
                errors.Add(_resourceContainer.GetResourceString("AccountValidDescriptionError"));
            }

            return Task.FromResult<Result>(new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) });
        }

        Account PopulateGenericHiddenAccount(IEnumerable<Account> hiddenAccounts)
        {
            var genericHiddenAccount = Constants.GenericHiddenAccount.DeepCopy();

            genericHiddenAccount.Pending = hiddenAccounts.Sum(a => a.Pending);
            genericHiddenAccount.Posted = hiddenAccounts.Sum(a => a.Posted);
            genericHiddenAccount.Balance = hiddenAccounts.Sum(a => a.Balance);
            genericHiddenAccount.Payment = hiddenAccounts.Sum(a => a.Payment);
            genericHiddenAccount.Description = _resourceContainer.GetResourceString("Hidden");
            genericHiddenAccount.Group = _resourceContainer.GetResourceString("Hidden");

            return genericHiddenAccount;
        }


        async Task<Result> ValidateDeleteAccountAsync(Guid accountId)
        {
            var errors = new List<string>();

            var tempAccount = await _accountDataAccess.ReadAccountAsync(accountId).ConfigureAwait(false);
            var account = await GetPopulatedAccount(tempAccount).ConfigureAwait(false);

            if (account.IsNew)
            {
                errors.Add(_resourceContainer.GetResourceString("AccountDeleteInactiveError"));
            }

            if (account.Balance != 0)
            {
                errors.Add(_resourceContainer.GetResourceString("AccountDeleteBalanceError"));
            }

            var accountTransactions = await _transactionDataAccess.ReadAccountTransactionsAsync(account.Id).ConfigureAwait(false);
            var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(account.Id).ConfigureAwait(false);

            if (accountTransactions.Any(t => t.IsActive && t.ServiceDate > DateTime.Now)
                || payeeTransactions.Any(t => t.IsActive && t.ServiceDate > DateTime.Now))
            {
                errors.Add(_resourceContainer.GetResourceString("AccountDeleteFutureTransactionsError"));
            }

            if (accountTransactions.Any(t => t.IsActive && t.Pending)
                || payeeTransactions.Any(t => t.IsActive && t.Pending))
            {
                errors.Add(_resourceContainer.GetResourceString("AccountDeletePendingTransactionsError"));
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }

        public async Task<Result> DeleteAccountAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var validationResult = await ValidateDeleteAccountAsync(id).ConfigureAwait(false);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                var account = await _accountDataAccess.ReadAccountAsync(id).ConfigureAwait(false);
                account.ModifiedDateTime = DateTime.Now;
                account.DeletedDateTime = DateTime.Now;
                await _accountDataAccess.UpdateAccountAsync(account).ConfigureAwait(false);

                var payee = await _payeeDataAccess.ReadPayeeAsync(id).ConfigureAwait(false);
                payee.ModifiedDateTime = DateTime.Now;
                payee.DeletedDateTime = DateTime.Now;
                await _payeeDataAccess.UpdatePayeeAsync(payee).ConfigureAwait(false);

                var debtEnvelope = await _envelopeDataAccess.ReadEnvelopeAsync(id).ConfigureAwait(false);
                debtEnvelope.ModifiedDateTime = DateTime.Now;
                debtEnvelope.DeletedDateTime = DateTime.Now;
                await _envelopeDataAccess.UpdateEnvelopeAsync(debtEnvelope).ConfigureAwait(false);

                var reconcileResult = await ReconcileAccount(account.Id, DateTime.Now, 0);

                return reconcileResult;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> UndoDeleteAccountAsync(Guid id)
        {
            var result = new Result();

            try
            {
                var account = await _accountDataAccess.ReadAccountAsync(id).ConfigureAwait(false);
                if (account.IsDeleted)
                {
                    account.ModifiedDateTime = DateTime.Now;
                    account.DeletedDateTime = null;
                    await _accountDataAccess.UpdateAccountAsync(account).ConfigureAwait(false);

                    var payee = await _payeeDataAccess.ReadPayeeAsync(id).ConfigureAwait(false);
                    payee.ModifiedDateTime = DateTime.Now;
                    payee.DeletedDateTime = null;
                    await _payeeDataAccess.UpdatePayeeAsync(payee).ConfigureAwait(false);

                    var debtEnvelope = await _envelopeDataAccess.ReadEnvelopeAsync(id).ConfigureAwait(false);
                    debtEnvelope.ModifiedDateTime = DateTime.Now;
                    debtEnvelope.DeletedDateTime = null;
                    await _envelopeDataAccess.UpdateEnvelopeAsync(debtEnvelope).ConfigureAwait(false);

                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = _resourceContainer.GetResourceString("AccountUndoDeleteNotDeletedError");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Account>>> GetDeletedAccountsAsync()
        {
            var result = new Result<IReadOnlyList<Account>>();

            var allAccounts = await _accountDataAccess.ReadAccountsAsync().ConfigureAwait(false);

            var accounts = allAccounts.Where(a => a.IsDeleted);

            var tasks = accounts.Select(GetPopulatedAccount);

            var accountsToReturn = (await Task.WhenAll(tasks)).ToList();
            accountsToReturn.Sort();

            result.Success = true;
            result.Data = accountsToReturn;

            return result;
        }
    }
}
