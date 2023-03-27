using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.DataAccess;
using BudgetBadger.Core.Localization;
using BudgetBadger.Logic.Models;
using BudgetBadger.Logic.Converters;
using BudgetBadger.DataAccess.Dtos;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Logic
{
    public interface IAccountLogic
    {
        Task<ItemsResponse<Account>> SearchAccountsAsync(bool? hidden = null);
        Task<DataResponse<AccountId>> CreateAccountAsync(string description, string notes, AccountType accountType, decimal balance, bool hidden);
        Task<DataResponse<Account>> ReadAccountAsync(AccountId accountId);
        Task<Response> UpdateAccountAsync(AccountId accountId, string description, string notes, bool hidden);
        Task<Response> DeleteAccountAsync(AccountId accountId);
        Task<Response> ReconcileAccountAsync(AccountId accountId, DateTime cutoffDate, decimal postedBalance);
    }

    public class AccountLogic : IAccountLogic
    {
        private readonly IDataAccess _dataAccess;

        public AccountLogic(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<ItemsResponse<Account>> SearchAccountsAsync(bool? hidden = null)
        {
            try
            {
                // get all non-deleted payeeDtos
                IEnumerable<AccountDto> accountDtos = await _dataAccess.ReadAccountDtosAsync().ConfigureAwait(false);
                accountDtos = accountDtos.Where(p => !p.Deleted).ToList();

                if (hidden == true)
                {
                    accountDtos = accountDtos.AsParallel().Where(p => p.Hidden);
                }
                else if (hidden == false)
                {
                    accountDtos = accountDtos.AsParallel().Where(p => !p.Hidden);
                }

                accountDtos = accountDtos.ToList();
                var accountIds = new HashSet<Guid>(accountDtos.Select(a => a.Id));
                var accountTransactions = await _dataAccess.ReadTransactionDtosAsync(accountIds: accountIds).ConfigureAwait(false); ;
                var payeeTransactions = await _dataAccess.ReadTransactionDtosAsync(payeeIds: accountIds).ConfigureAwait(false);
                var envelopeTransactions = await _dataAccess.ReadTransactionDtosAsync(envelopeIds: accountIds).ConfigureAwait(false);
                var debtBudgets = await _dataAccess.ReadBudgetDtosAsync(envelopeIds: accountIds).ConfigureAwait(false); ;
                var debtBudgetPeriods = await _dataAccess.ReadBudgetPeriodDtosAsync(debtBudgets?.Select(b => b.BudgetPeriodId) ?? Enumerable.Empty<Guid>()).ConfigureAwait(false); ;

                var accounts = accountDtos
                    .AsParallel()
                    .Select(p => AccountConverter.Convert(p,
                        accountTransactions.Concat(payeeTransactions).Concat(envelopeTransactions),
                        debtBudgets,
                        debtBudgetPeriods))
                    .ToList();

                return ItemsResponse.OK(accounts, accounts.Count);
            }
            catch (Exception ex)
            {
                return ItemsResponse.InternalError<Account>(ex.Message);
            }
        }

        public async Task<DataResponse<AccountId>> CreateAccountAsync(string description, string notes, AccountType accountType, decimal balance, bool hidden)
        {
            try
            {
                if (string.IsNullOrEmpty(description))
                {
                    return DataResponse.BadRequest<AccountId>(AppResources.AccountValidDescriptionError);
                }

                var accountId = new AccountId();
                var dateTimeNow = DateTime.Now;
                var dateNow = dateTimeNow.Date;

                // determine which envelope should be used
                Guid startingBalanceEnvelope;
                if (accountType == AccountType.Reporting)
                {
                    startingBalanceEnvelope = Constants.IgnoredEnvelopeId;
                }
                else if (balance < 0) // on budget and negative
                {
                    startingBalanceEnvelope = accountId;
                }
                else // on budget and positive
                {
                    startingBalanceEnvelope = Constants.IncomeEnvelopeId;
                }

                await _dataAccess.CreateAccountDtoAsync(new AccountDto()
                {
                    Id = accountId,
                    Description = description,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    OnBudget = accountType == AccountType.Budget,
                    Hidden = hidden,
                    Deleted = false,
                    ModifiedDateTime = dateTimeNow
                }).ConfigureAwait(false);

                await _dataAccess.CreatePayeeDtoAsync(new PayeeDto()
                {
                    Id = accountId,
                    Description = description,
                    Notes = null,
                    Hidden = hidden,
                    Deleted = false,
                    ModifiedDateTime = dateTimeNow
                }).ConfigureAwait(false);

                await _dataAccess.CreateEnvelopeDtoAsync(new EnvelopeDto()
                {
                    Id = accountId,
                    Description = description,
                    Notes = null,
                    EnvelopGroupId = Constants.DebtEnvelopeGroupId,
                    IgnoreOverspend = true,
                    Hidden = hidden,
                    Deleted = false,
                    ModifiedDateTime = dateTimeNow
                }).ConfigureAwait(false);

                await _dataAccess.CreateTransactionDtoAsync(new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    Amount = balance,
                    Deleted = false,
                    EnvelopeId = startingBalanceEnvelope,
                    ModifiedDateTime = dateTimeNow,
                    Notes = null,
                    PayeeId = Constants.StartingBalancePayeeId,
                    Posted = true,
                    Reconciled = true,
                    ServiceDate = dateNow,
                    SplitId = null
                }).ConfigureAwait(false);

                return DataResponse.OK(accountId);
            }
            catch (Exception ex)
            {
                return DataResponse.InternalError<AccountId>(ex.Message);
            }
        }

        public async Task<DataResponse<Account>> ReadAccountAsync(AccountId accountId)
        {
            var accountDtos = await _dataAccess.ReadAccountDtosAsync(new List<Guid>() { accountId }).ConfigureAwait(false); ;
            var accountDto = accountDtos.FirstOrDefault();
            if (accountDto == null)
            {
                return DataResponse.NotFound<Account>(AppResources.NotFoundError);
            }
            else if (accountDto.Deleted)
            {
                return DataResponse.Gone<Account>(AppResources.GoneError);
            }

            var accountIds = new List<Guid>() { accountId };
            var accountTransactions = await _dataAccess.ReadTransactionDtosAsync(accountIds: accountIds).ConfigureAwait(false); ;
            var payeeTransactions = await _dataAccess.ReadTransactionDtosAsync(payeeIds: accountIds).ConfigureAwait(false);
            var envelopeTransactions = await _dataAccess.ReadTransactionDtosAsync(envelopeIds: accountIds).ConfigureAwait(false);
            var debtBudgets = await _dataAccess.ReadBudgetDtosAsync(envelopeIds: accountIds).ConfigureAwait(false); ;
            var debtBudgetPeriods = await _dataAccess.ReadBudgetPeriodDtosAsync(debtBudgets?.Select(b => b.BudgetPeriodId) ?? Enumerable.Empty<Guid>()).ConfigureAwait(false); ;

            var account = AccountConverter.Convert(accountDto,
                accountTransactions.Concat(payeeTransactions).Concat(envelopeTransactions),
                debtBudgets,
                debtBudgetPeriods);

            return DataResponse.OK(account);
        }

        public async Task<Response> UpdateAccountAsync(AccountId accountId, string description, string notes, bool hidden)
        {
            try
            {
                if (accountId == Guid.Empty)
                {
                    return Response.BadRequest(AppResources.AccountSaveSystemError);
                }

                if (string.IsNullOrEmpty(description))
                {
                    return Response.BadRequest(AppResources.AccountValidDescriptionError);
                }

                var existingAccountDtos = await _dataAccess.ReadAccountDtosAsync(new List<Guid>() { accountId }).ConfigureAwait(false);
                var existingDto = existingAccountDtos.FirstOrDefault();
                if (existingDto == null)
                {
                    return Response.NotFound(AppResources.TransactionValidAccountExistError);
                }
                if (existingDto.Deleted)
                {
                    return Response.Gone(AppResources.TransactionValidAccountDeletedError); //TODO use better strings
                }

                var dateTimeNow = DateTime.Now;

                await _dataAccess.UpdateEnvelopeDtoAsync(new EnvelopeDto()
                {
                    Id = accountId,
                    Description = description,
                    Notes = null,
                    EnvelopGroupId = Constants.DebtEnvelopeGroupId,
                    IgnoreOverspend = true,
                    Hidden = hidden,
                    Deleted = false,
                    ModifiedDateTime = dateTimeNow
                }).ConfigureAwait(false);

                await _dataAccess.UpdatePayeeDtoAsync(new PayeeDto()
                {
                    Id = accountId,
                    Description = description,
                    Notes = null,
                    Hidden = hidden,
                    Deleted = false,
                    ModifiedDateTime = dateTimeNow
                }).ConfigureAwait(false);

                await _dataAccess.UpdateAccountDtoAsync(new AccountDto()
                {
                    Id = accountId,
                    Description = description,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    OnBudget = existingDto.OnBudget,
                    Hidden = hidden,
                    Deleted = false,
                    ModifiedDateTime = dateTimeNow
                }).ConfigureAwait(false);

                return Response.OK();
            }
            catch (Exception ex)
            {
                return Response.InternalError(ex.Message);
            }
        }

        public async Task<Response> DeleteAccountAsync(AccountId accountId)
        {
            try
            {
                if (accountId == Guid.Empty)
                {
                    return Response.BadRequest(AppResources.AccountDeleteNotHiddenError);
                }

                var accounts = await _dataAccess.ReadAccountDtosAsync(new List<Guid>() { accountId }).ConfigureAwait(false);
                var accountDto = accounts.FirstOrDefault();
                if (accountDto == null)
                {
                    return Response.NotFound(AppResources.AccountDeleteNotHiddenError);
                }
                else if (accountDto.Deleted)
                {
                    return Response.Gone(AppResources.AccountDeleteNotHiddenError);
                }
                else if (!accountDto.Hidden)
                {
                    return Response.Conflict(AppResources.AccountDeleteInactiveError);
                }

                var accountTransactionDtos = await _dataAccess.ReadTransactionDtosAsync(accountIds: new List<Guid>() { accountId }).ConfigureAwait(false);
                if (accountTransactionDtos.Any(t => !t.Deleted))
                {
                    return Response.Conflict(AppResources.AccountDeleteActiveTransactionsError);
                }

                var payeeTransactionDtos = await _dataAccess.ReadTransactionDtosAsync(payeeIds: new List<Guid>() { accountId }).ConfigureAwait(false);
                if (payeeTransactionDtos.Any(t => !t.Deleted))
                {
                    return Response.Conflict(AppResources.AccountDeleteActiveTransactionsError);
                }

                var envelopeTransactionDtos = await _dataAccess.ReadTransactionDtosAsync(envelopeIds: new List<Guid>() { accountId }).ConfigureAwait(false);
                if (envelopeTransactionDtos.Any(t => !t.Deleted))
                {
                    return Response.Conflict(AppResources.AccountDeleteActiveTransactionsError);
                }

                var budgets = await _dataAccess.ReadBudgetDtosAsync(envelopeIds: new List<Guid>() { accountId }).ConfigureAwait(false);
                if (budgets.Any(b => b.Amount != 0))
                {
                    return Response.Conflict(AppResources.AccountDeleteSystemError); //TODO make better
                }

                await _dataAccess.UpdateAccountDtoAsync(accountDto with { Deleted = true, ModifiedDateTime = DateTime.Now }).ConfigureAwait(false);

                var payeeDtos = await _dataAccess.ReadPayeeDtosAsync(new List<Guid>() { accountId }).ConfigureAwait(false);
                var payeeDto = payeeDtos.FirstOrDefault();
                await _dataAccess.UpdatePayeeDtoAsync(payeeDto with { Deleted = true, ModifiedDateTime = DateTime.Now }).ConfigureAwait(false);

                var envelopeDtos = await _dataAccess.ReadEnvelopeDtosAsync(new List<Guid>() { accountId }).ConfigureAwait(false);
                var envelopeDto = envelopeDtos.FirstOrDefault();
                await _dataAccess.UpdateEnvelopeDtoAsync(envelopeDto with { Deleted = true, ModifiedDateTime = DateTime.Now }).ConfigureAwait(false);

                return Response.OK();
            }
            catch (Exception ex)
            {
                return Response.InternalError(ex.Message);
            }
        }

        public async Task<Response> ReconcileAccountAsync(AccountId accountId, DateTime cutoffDate, decimal postedBalance)
        {
            if (accountId == Guid.Empty)
            {
                return Response.BadRequest(AppResources.AccountSaveSystemError); //TODO
            }

            var accounts = await _dataAccess.ReadAccountDtosAsync(new List<Guid>() { accountId }).ConfigureAwait(false);
            var accountDto = accounts.FirstOrDefault();
            if (accountDto == null)
            {
                return Response.NotFound(AppResources.TransactionValidAccountExistError); //TODO
            }
            else if (accountDto.Deleted)
            {
                return Response.Gone(AppResources.TransactionValidAccountDeletedError); //TODO
            }

            var accountIds = new List<Guid>() { accountId };
            var accountTransactions = await _dataAccess.ReadTransactionDtosAsync(accountIds: accountIds).ConfigureAwait(false);
            var payeeTransactions = await _dataAccess.ReadTransactionDtosAsync(payeeIds: accountIds).ConfigureAwait(false);

            var accountTransactionsToReconcile = accountTransactions.Where(t => !t.Deleted
                                                                       && t.Posted
                                                                       && t.ServiceDate.Date <= cutoffDate.Date).ToList();

            var payeeTransactionsToReconcile = payeeTransactions.Where(t => !t.Deleted
                                                                       && t.Posted
                                                                       && t.ServiceDate <= cutoffDate.Date).ToList();

            var actualPostedBalance = accountTransactionsToReconcile.Sum(t => t.Amount)
                - payeeTransactionsToReconcile.Sum(t => t.Amount);

            if (actualPostedBalance  == postedBalance)
            {
                var now = DateTime.Now;
                var tasks = accountTransactionsToReconcile
                    .Concat(payeeTransactionsToReconcile)
                    .Select(transactionDto => _dataAccess.UpdateTransactionDtoAsync(transactionDto with { Reconciled = true, ModifiedDateTime = now }));

                await Task.WhenAll(tasks).ConfigureAwait(false);

                return Response.OK();
            }
            else
            {
                return Response.Conflict(AppResources.AccountReconcileAmountsDoNotMatchError);
            }
        }
    }
}
