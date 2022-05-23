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
    public class AccountLogic : IAccountLogic
    {
        private readonly IDataAccess _dataAccess;
        private readonly IResourceContainer _resourceContainer;

        public AccountLogic(IDataAccess dataAccess,
                            IResourceContainer resourceContainer)
        {
            _dataAccess = dataAccess;
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
                await _dataAccess.CreateAccountAsync(accountToUpsert).ConfigureAwait(false);

                var accountPayee = new Payee
                {
                    Id = accountToUpsert.Id,
                    Description = accountToUpsert.Description,
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow
                };
                await _dataAccess.CreatePayeeAsync(accountPayee).ConfigureAwait(false);

                //create a debt envelope for new accounts
                var debtEnvelopeGroup = await _dataAccess.ReadEnvelopeGroupAsync(Constants.DebtEnvelopeGroup.Id);
                var debtEnvelope = new Envelope
                {
                    Id = accountToUpsert.Id,
                    Description = accountToUpsert.Description,
                    IgnoreOverspend = true,
                    Group = debtEnvelopeGroup,
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow
                };
                await _dataAccess.CreateEnvelopeAsync(debtEnvelope).ConfigureAwait(false);

                // determine which envelope should be used
                Envelope startingBalanceEnvelope;
                if (accountToUpsert.OffBudget)
                {
                    startingBalanceEnvelope = await _dataAccess.ReadEnvelopeAsync(Constants.IgnoredEnvelope.Id);
                }
                else if (accountToUpsert.Balance < 0) // on budget and negative
                {
                    startingBalanceEnvelope = debtEnvelope;
                }
                else // on budget and positive
                {
                    startingBalanceEnvelope = await _dataAccess.ReadEnvelopeAsync(Constants.IncomeEnvelope.Id);
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
                    Payee = await _dataAccess.ReadPayeeAsync(Constants.StartingBalancePayee.Id),
                    Envelope = startingBalanceEnvelope
                };

                await _dataAccess.CreateTransactionAsync(startingBalance).ConfigureAwait(false);
            }
            else
            {
                // update linked payee
                var accountPayee = await _dataAccess.ReadPayeeAsync(accountToUpsert.Id).ConfigureAwait(false);
                accountPayee.ModifiedDateTime = dateTimeNow;
                accountPayee.Description = accountToUpsert.Description;
                await _dataAccess.UpdatePayeeAsync(accountPayee);

                // update the debt envelope name
                var debtEnvelope = await _dataAccess.ReadEnvelopeAsync(accountToUpsert.Id).ConfigureAwait(false);
                debtEnvelope.ModifiedDateTime = dateTimeNow;
                debtEnvelope.Description = accountToUpsert.Description;
                await _dataAccess.UpdateEnvelopeAsync(debtEnvelope);

                accountToUpsert.ModifiedDateTime = dateTimeNow;
                await _dataAccess.UpdateAccountAsync(accountToUpsert).ConfigureAwait(false);
            }

            return new Result<Account> { Success = true, Data = await GetPopulatedAccount(accountToUpsert).ConfigureAwait(false) };
        }

        public async Task<Result<int>> GetAccountsCountAsync()
        {
            var result = new Result<int>();

            try
            {
                var count = await _dataAccess.GetAccountsCountAsync();
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
                var account = await _dataAccess.ReadAccountAsync(id).ConfigureAwait(false);
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

            try
            {
                var allAccounts = await _dataAccess.ReadAccountsAsync().ConfigureAwait(false);
                var accounts = allAccounts.Where(a => FilterAccount(a, FilterType.Standard));

                var tasks = accounts.Select(GetPopulatedAccount);
                var accountsToReturn = (await Task.WhenAll(tasks)).ToList();

                if (allAccounts.Any(a => FilterAccount(a, FilterType.Hidden)))
                {
                    var hiddenAccountTasks = allAccounts.Where(a => FilterAccount(a, FilterType.Hidden)).Select(GetPopulatedAccount);
                    var hiddenAccounts = await Task.WhenAll(hiddenAccountTasks);

                    var genericHiddenAccount = GetGenericHiddenAccount(hiddenAccounts);
                    accountsToReturn.Add(genericHiddenAccount);
                }

                result.Success = true;
                result.Data = accountsToReturn;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Account>>> GetAccountsForSelectionAsync()
        {
            var result = new Result<IReadOnlyList<Account>>();

            try
            {
                var allAccounts = await _dataAccess.ReadAccountsAsync().ConfigureAwait(false);

                var accounts = allAccounts.Where(a => FilterAccount(a, FilterType.Selection));

                var tasks = accounts.Select(GetPopulatedAccount);

                var accountsToReturn = (await Task.WhenAll(tasks)).ToList();

                result.Success = true;
                result.Data = accountsToReturn;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Account>>> GetHiddenAccountsAsync()
        {
            var result = new Result<IReadOnlyList<Account>>();

            try
            { 
                var allAccounts = await _dataAccess.ReadAccountsAsync().ConfigureAwait(false);

                var accounts = allAccounts.Where(a => FilterAccount(a, FilterType.Hidden));

                var tasks = accounts.Select(GetPopulatedAccount);

                var accountsToReturn = (await Task.WhenAll(tasks)).ToList();
                accountsToReturn.Sort();

                result.Success = true;
                result.Data = accountsToReturn;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Account>> SoftDeleteAccountAsync(Guid id)
        {
            var result = new Result<Account>();

            try
            {
                var account = await _dataAccess.ReadAccountAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (account.IsNew || account.IsDeleted || account.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountDeleteNotHiddenError"));
                }

                if (account.IsGenericHiddenAccount)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountDeleteSystemError"));
                }

                var accountTransactions = await _dataAccess.ReadAccountTransactionsAsync(id).ConfigureAwait(false);
                var payeeTransactions = await _dataAccess.ReadPayeeTransactionsAsync(id).ConfigureAwait(false);

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

                var payee = await _dataAccess.ReadPayeeAsync(account.Id).ConfigureAwait(false);
                payee.DeletedDateTime = now;
                payee.ModifiedDateTime = now;
                await _dataAccess.UpdatePayeeAsync(payee).ConfigureAwait(false);

                var envelope = await _dataAccess.ReadEnvelopeAsync(account.Id).ConfigureAwait(false);
                envelope.DeletedDateTime = now;
                envelope.ModifiedDateTime = now;
                await _dataAccess.UpdateEnvelopeAsync(envelope).ConfigureAwait(false);

                account.DeletedDateTime = now;
                account.ModifiedDateTime = now;
                await _dataAccess.UpdateAccountAsync(account).ConfigureAwait(false);

                result.Success = true;
                result.Data = await GetPopulatedAccount(account).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Account>> HideAccountAsync(Guid id)
        {
            var result = new Result<Account>();

            try
            {
                var account = await _dataAccess.ReadAccountAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (!account.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountHideInactiveError"));
                }

                if (account.IsGenericHiddenAccount)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountHideSystemError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                var now = DateTime.Now;

                var payee = await _dataAccess.ReadPayeeAsync(account.Id).ConfigureAwait(false);
                payee.HiddenDateTime = now;
                payee.ModifiedDateTime = now;
                await _dataAccess.UpdatePayeeAsync(payee).ConfigureAwait(false);

                var envelope = await _dataAccess.ReadEnvelopeAsync(account.Id).ConfigureAwait(false);
                envelope.HiddenDateTime = now;
                envelope.ModifiedDateTime = now;
                await _dataAccess.UpdateEnvelopeAsync(envelope).ConfigureAwait(false);

                account.HiddenDateTime = now;
                account.ModifiedDateTime = now;
                await _dataAccess.UpdateAccountAsync(account).ConfigureAwait(false);

                result.Success = true;
                result.Data = await GetPopulatedAccount(account).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Account>> UnhideAccountAsync(Guid id)
        {
            var result = new Result<Account>();

            try
            {
                var account = await _dataAccess.ReadAccountAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (account.IsNew || account.IsActive || account.IsDeleted)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountUnhideNotHiddenError"));
                }

                if (account.IsGenericHiddenAccount)
                {
                    errors.Add(_resourceContainer.GetResourceString("AccountUnhideSystemError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                var now = DateTime.Now;

                var payee = await _dataAccess.ReadPayeeAsync(account.Id).ConfigureAwait(false);
                payee.HiddenDateTime = null;
                payee.ModifiedDateTime = now;
                await _dataAccess.UpdatePayeeAsync(payee).ConfigureAwait(false);

                var envelope = await _dataAccess.ReadEnvelopeAsync(account.Id).ConfigureAwait(false);
                envelope.HiddenDateTime = null;
                envelope.ModifiedDateTime = now;
                await _dataAccess.UpdateEnvelopeAsync(envelope).ConfigureAwait(false);

                account.HiddenDateTime = null;
                account.ModifiedDateTime = now;
                await _dataAccess.UpdateAccountAsync(account).ConfigureAwait(false);

                result.Success = true;
                result.Data = await GetPopulatedAccount(account).ConfigureAwait(false);
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
                var accountTransactions = await _dataAccess.ReadAccountTransactionsAsync(accountId).ConfigureAwait(false);
                var payeeTransactions = await _dataAccess.ReadPayeeTransactionsAsync(accountId).ConfigureAwait(false);
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
                        tasks.Add(_dataAccess.UpdateTransactionAsync(transaction));
                    }

                    foreach (var transaction in payeeTransactionsToReconcile)
                    {
                        transaction.Posted = true;
                        transaction.ModifiedDateTime = now;
                        transaction.ReconciledDateTime = transaction.ReconciledDateTime ?? dateTime;
                        tasks.Add(_dataAccess.UpdateTransactionAsync(transaction));
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
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }

            if (account != null)
            {
                return account.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        public bool FilterAccount(Account account, FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.Standard:
                case FilterType.Report:
                case FilterType.Selection:
                    return account.IsActive;
                case FilterType.Hidden:
                    return account.IsHidden && !account.IsDeleted && !account.IsGenericHiddenAccount;
                case FilterType.All:
                default:
                    return true;
            }
        }

        async Task<Account> GetPopulatedAccount(Account account)
        {
            var accountTransactions = await _dataAccess.ReadAccountTransactionsAsync(account.Id).ConfigureAwait(false);
            var payeeTransactions = await _dataAccess.ReadPayeeTransactionsAsync(account.Id).ConfigureAwait(false);
            var accountDebtBudgets = await _dataAccess.ReadBudgetsFromEnvelopeAsync(account.Id).ConfigureAwait(false);
            var debtTransactions = await _dataAccess.ReadEnvelopeTransactionsAsync(account.Id).ConfigureAwait(false);

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

            account.TranslateAccount(_resourceContainer);

            return account;
        }

        Task<Result> ValidateAccountAsync(Account account)
        {
            var errors = new List<string>();

            if (account.IsGenericHiddenAccount)
            {
                errors.Add(_resourceContainer.GetResourceString("AccountSaveSystemError"));
            }

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

        Account GetGenericHiddenAccount(IEnumerable<Account> hiddenAccounts = null)
        {
            var genericHiddenAccount = Constants.GenericHiddenAccount.DeepCopy();

            genericHiddenAccount.TranslateAccount(_resourceContainer);

            if (hiddenAccounts != null)
            {
                genericHiddenAccount.Pending = hiddenAccounts.Sum(a => a.Pending);
                genericHiddenAccount.Posted = hiddenAccounts.Sum(a => a.Posted);
                genericHiddenAccount.Balance = hiddenAccounts.Sum(a => a.Balance);
                genericHiddenAccount.Payment = hiddenAccounts.Sum(a => a.Payment);
            }

            return genericHiddenAccount;
        }
    }
}
