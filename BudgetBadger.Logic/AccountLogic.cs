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

        public async Task<Result> DeleteAccountAsync(Account account)
        {
            var result = new Result();
            var newAccont = account.DeepCopy();
            newAccont.DeletedDateTime = DateTime.Now;

            try
            {
                await AccountDataAccess.UpdateAccountAsync(newAccont);
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

        public async Task<Result<IEnumerable<Account>>> GetAccountsAsync()
        {
            var result = new Result<IEnumerable<Account>>();

            var accounts = await AccountDataAccess.ReadAccountsAsync();

            var tasks = accounts.Select(a => GetPopulatedAccount(a));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Result<IEnumerable<AccountType>>> GetAccountTypesAsync()
        {
            return new Result<IEnumerable<AccountType>> { Success = true, Data = await AccountDataAccess.ReadAccountTypesAsync() };
        }

        public IEnumerable<Account> SearchAccounts(IEnumerable<Account> accounts, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return accounts;
            }

            return accounts.Where(a => a.Description.ToLower().Contains(searchText.ToLower()));
        }

        public ILookup<string, Account> GroupAccounts(IEnumerable<Account> accounts)
        {
            var groupedAccounts = accounts.ToLookup(a => a.OnBudget ? "On Budget" : "Off Budget");

            return groupedAccounts;
        }

        public async Task<Result<Account>> SaveAccountAsync(Account account)
        {
            if (!account.IsValid())
            {
                return account.Validate().ToResult<Account>();
            }

            //check for existence of account type
            var accountTypes = await AccountDataAccess.ReadAccountTypesAsync();
            if (!accountTypes.Any(a => a.Id == account.Type.Id))
            {
                return new Result<Account> { Success = false, Message = "Account Type is required" };
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

            account.Balance = accountTransactions.Sum(t => t.Amount);

            var payeeTransactions = await TransactionDataAccess.ReadPayeeTransactionsAsync(account.Id);

            account.Balance -= payeeTransactions.Sum(t => t.Amount);

            // payment 
            var dateTimeNow = DateTime.Now;

            var accountDebtBudgets = await EnvelopeDataAccess.ReadBudgetsFromEnvelopeAsync(account.Id);

            var amountBudgetedToPayDownDebt = accountDebtBudgets
                .Where(a => a.Schedule.BeginDate <= dateTimeNow)
                .Sum(a => a.Amount);

            var debtTransactions = new List<Transaction>();
            if (accountDebtBudgets.Any())
            {
                debtTransactions.AddRange(await TransactionDataAccess.ReadEnvelopeTransactionsAsync(accountDebtBudgets.FirstOrDefault().Envelope.Id));
            }

            var debtTransactionAmount = debtTransactions.Where(d => d.ServiceDate <= dateTimeNow).Sum(d => d.Amount);

            account.Payment = amountBudgetedToPayDownDebt + debtTransactionAmount - account.Balance;

            return account;
        }
    }
}
