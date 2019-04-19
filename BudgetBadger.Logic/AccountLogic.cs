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

        async Task<Result> ValidateDeleteAccountAsync(Guid accountId)
        {
            var errors = new List<string>();

            var tempAccount = await _accountDataAccess.ReadAccountAsync(accountId).ConfigureAwait(false);
            var account = await GetPopulatedAccount(tempAccount).ConfigureAwait(false);

            if (account.IsNew)
            {
                errors.Add("Cannot delete an inactive account");
            }

            if (account.Balance != 0)
            {
                errors.Add("Cannot delete account with balance");
            }

            var accountTransactions = await _transactionDataAccess.ReadAccountTransactionsAsync(account.Id).ConfigureAwait(false);
            var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(account.Id).ConfigureAwait(false);

            if (accountTransactions.Any(t => t.IsActive && t.ServiceDate > DateTime.Now)
                || payeeTransactions.Any(t => t.IsActive && t.ServiceDate > DateTime.Now))
            {
                errors.Add("Cannot delete account with future transactions");
            }

            if (accountTransactions.Any(t => t.IsActive && t.Pending)
                || payeeTransactions.Any(t => t.IsActive && t.Pending))
            {
                errors.Add("Cannot delete account with pending transactions");
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

                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = "Account is not deleted";
                }
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
            IEnumerable<Account> accounts;

            accounts = allAccounts.Where(a => a.IsActive);

            var tasks = accounts.Select(GetPopulatedAccount);

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result<IReadOnlyList<Account>>> GetAccountsForSelectionAsync()
        {
            var result = new Result<IReadOnlyList<Account>>();

            var allAccounts = await _accountDataAccess.ReadAccountsAsync().ConfigureAwait(false);

            var accounts = allAccounts.Where(a => a.IsActive);

            var tasks = accounts.Select(GetPopulatedAccount);

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result<IReadOnlyList<Account>>> GetDeletedAccountsAsync()
        {
            var result = new Result<IReadOnlyList<Account>>();

            var allAccounts = await _accountDataAccess.ReadAccountsAsync().ConfigureAwait(false);

            var accounts = allAccounts.Where(a => a.IsDeleted);

            var tasks = accounts.Select(GetPopulatedAccount);

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

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

		//public IReadOnlyList<Account> OrderAccounts(IEnumerable<Account> accounts)
        //{
        //    return accounts.OrderBy(a => a.Type ).ThenBy(a => a.Description).ToList();
        //}

        public Task<Result> ValidateAccountAsync(Account account)
        {
            var errors = new List<string>();

            if (account.IsNew && !account.Balance.HasValue)
            {
                errors.Add(_resourceContainer.GetString("AccountBalanceRequiredError"));
            }

            if (string.IsNullOrEmpty(account.Description))
            {
                errors.Add("Account description is required");
            }

            return Task.FromResult<Result>(new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) });
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
                var debtEnvelope = new Envelope
                {
                    Id = accountToUpsert.Id,
                    Description = accountToUpsert.Description,
                    IgnoreOverspend = true,
                    Group = Constants.DebtEnvelopeGroup,
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow
                };
                await _envelopeDataAccess.CreateEnvelopeAsync(debtEnvelope).ConfigureAwait(false);

                // determine which envelope should be used
                Envelope startingBalanceEnvelope;
                if (accountToUpsert.OffBudget)
                {
                    startingBalanceEnvelope = Constants.IgnoredEnvelope;
                }
                else if (accountToUpsert.Balance < 0) // on budget and negative
                {
                    startingBalanceEnvelope = debtEnvelope;
                }
                else // on budget and positive
                {
                    startingBalanceEnvelope = Constants.IncomeEnvelope;
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
                    Payee = Constants.StartingBalancePayee,
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

        async Task<Account> GetPopulatedAccount(Account account)
        {
            
            var accountTransactions = await _transactionDataAccess.ReadAccountTransactionsAsync(account.Id).ConfigureAwait(false);
            var payeeTransactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(account.Id).ConfigureAwait(false);
            var accountDebtBudgets = await _envelopeDataAccess.ReadBudgetsFromEnvelopeAsync(account.Id).ConfigureAwait(false);
            var debtTransactions = await _transactionDataAccess.ReadEnvelopeTransactionsAsync(account.Id).ConfigureAwait(false);

            return await Task.Run(() => PopulateAccount(account, accountTransactions, payeeTransactions, accountDebtBudgets, debtTransactions));
        }

        protected Account PopulateAccount(Account account,
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

            return account;
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
                    result.Message = "The reconciled amounts do not match";
                }

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
