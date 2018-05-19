﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.Logic
{
    public class AccountLogic : IAccountLogic
    {
        readonly IAccountDataAccess AccountDataAccess;
        readonly ITransactionDataAccess TransactionDataAccess;
        readonly IPayeeDataAccess PayeeDataAccess;
        readonly IEnvelopeDataAccess EnvelopeDataAccess;

        public AccountLogic(IAccountDataAccess accountDataAccess,
                            ITransactionDataAccess transactionDataAccess,
                            IPayeeDataAccess payeeDataAccess,
                            IEnvelopeDataAccess envelopeDataAccess)
        {
            AccountDataAccess = accountDataAccess;
            TransactionDataAccess = transactionDataAccess;
            PayeeDataAccess = payeeDataAccess;
            EnvelopeDataAccess = envelopeDataAccess;
        }

        async Task<Result> ValidateDeleteAccountAsync(Account account)
        {
            if (account.OnBudget)
            {
                if (account.Balance != 0)
                {
                    return new Result { Success = false, Message = "Cannot delete account with balance" };
                }

                var accountTransactions = await TransactionDataAccess.ReadAccountTransactionsAsync(account.Id);
                if (accountTransactions.Any(t => t.IsActive && t.ServiceDate > DateTime.Now))
                {
                    return new Result { Success = false, Message = "Cannot delete account with future transactions" }; 
                }
            }

            return new Result { Success = true };
        }

        public async Task<Result> DeleteAccountAsync(Guid id)
        {
            //some validation logic?
            var result = new Result();

            try
            {
                var account = await AccountDataAccess.ReadAccountAsync(id);
                var populatedAccount = await GetPopulatedAccount(account);
                var validationResult = await ValidateDeleteAccountAsync(populatedAccount);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

				account.ModifiedDateTime = DateTime.Now;
                account.DeletedDateTime = DateTime.Now;
                await AccountDataAccess.UpdateAccountAsync(account);
                result.Success = true;
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
                var account = await AccountDataAccess.ReadAccountAsync(id);
                var populatedAccount = await GetPopulatedAccount(account);
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

            var allAccounts = await AccountDataAccess.ReadAccountsAsync();
            IEnumerable<Account> accounts;

            if (false) // show deleted
            {
                accounts = allAccounts;
            }
            else
            {
                accounts = allAccounts.Where(a => a.IsActive);
            }

            var tasks = accounts.Select(a => GetPopulatedAccount(a));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result<IReadOnlyList<Account>>> GetAccountsForSelectionAsync()
        {
            var result = new Result<IReadOnlyList<Account>>();

            var allAccounts = await AccountDataAccess.ReadAccountsAsync();

            var accounts = allAccounts.Where(a => a.IsActive);

            var tasks = accounts.Select(a => GetPopulatedAccount(a));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public IReadOnlyList<string> GetAccountTypes()
        {
            return new List<string> { "Budget", "Reporting" };
        }

        public IReadOnlyList<Account> SearchAccounts(IEnumerable<Account> accounts, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return accounts.ToList();
            }

            return accounts.Where(a => a.Description.ToLower().Contains(searchText.ToLower())).ToList();
        }

        public IReadOnlyList<IGrouping<string, Account>> GroupAccounts(IEnumerable<Account> accounts)
        {
            var groupedAccounts = accounts.GroupBy(a => a.Type).ToList();

            return groupedAccounts;
        }

        public async Task<Result> ValidateAccountAsync(Account account)
        {
            if (!account.IsValid())
            {
                return account.Validate().ToResult<Account>();
            }

            return new Result { Success = true }; 
        }

        public async Task<Result<Account>> SaveAccountAsync(Account account)
        {
            var validationResult = await ValidateAccountAsync(account);

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
                await AccountDataAccess.CreateAccountAsync(accountToUpsert);

                var accountPayee = new Payee
                {
                    Id = accountToUpsert.Id,
                    Description = accountToUpsert.Description,
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow
                };
                await PayeeDataAccess.CreatePayeeAsync(accountPayee);

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
                await EnvelopeDataAccess.CreateEnvelopeAsync(debtEnvelope);

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

                await TransactionDataAccess.CreateTransactionAsync(startingBalance);
            }
            else
            {
                // update linked payee
                var accountPayee = await PayeeDataAccess.ReadPayeeAsync(accountToUpsert.Id);
                accountPayee.ModifiedDateTime = dateTimeNow;
                accountPayee.Description = accountToUpsert.Description;
                await PayeeDataAccess.UpdatePayeeAsync(accountPayee);

                // update the debt envelope name
                var debtEnvelope = await EnvelopeDataAccess.ReadEnvelopeAsync(accountToUpsert.Id);
                debtEnvelope.ModifiedDateTime = dateTimeNow;
                debtEnvelope.Description = accountToUpsert.Description;
                await EnvelopeDataAccess.UpdateEnvelopeAsync(debtEnvelope);

                accountToUpsert.ModifiedDateTime = dateTimeNow;
                await AccountDataAccess.UpdateAccountAsync(accountToUpsert);
            }

            return new Result<Account> { Success = true, Data = accountToUpsert };
        }

        private async Task<Account> GetPopulatedAccount(Account account)
        {
            // balance
            var accountTransactions = await TransactionDataAccess.ReadAccountTransactionsAsync(account.Id);

            account.Balance = accountTransactions.Where(t => t.IsActive).Sum(t => t.Amount ?? 0);

            var payeeTransactions = await TransactionDataAccess.ReadPayeeTransactionsAsync(account.Id);

            account.Balance -= payeeTransactions.Where(t => t.IsActive).Sum(t => t.Amount ?? 0);

            // payment 
            var dateTimeNow = DateTime.Now;

            var accountDebtBudgets = await EnvelopeDataAccess.ReadBudgetsFromEnvelopeAsync(account.Id);

            var amountBudgetedToPayDownDebt = accountDebtBudgets
                .Where(a => a.Schedule.BeginDate <= dateTimeNow)
                .Sum(a => a.Amount);

            var debtTransactions = await TransactionDataAccess.ReadEnvelopeTransactionsAsync(account.Id);

            var debtTransactionAmount = debtTransactions
                .Where(d => d.IsActive && d.ServiceDate <= dateTimeNow)
                .Sum(d => d.Amount ?? 0);

            account.Payment = amountBudgetedToPayDownDebt + debtTransactionAmount - account.Balance ?? 0;

            return account;
        }
    }
}
