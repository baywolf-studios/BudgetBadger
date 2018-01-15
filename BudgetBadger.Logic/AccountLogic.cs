using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public class AccountLogic : IAccountLogic
    {
        readonly IAccountDataAccess AccountDataAccess;
        readonly ITransactionDataAccess TransactionDataAccess;
        readonly IPayeeDataAccess PayeeDataAccess;

        public AccountLogic(IAccountDataAccess accountDataAccess, ITransactionDataAccess transactionDataAccess, IPayeeDataAccess payeeDataAccess)
        {
            AccountDataAccess = accountDataAccess;
            TransactionDataAccess = transactionDataAccess;
            PayeeDataAccess = payeeDataAccess;

            var accountTypes = new List<AccountType>();
            accountTypes.Add(new AccountType
            {
                Id = new Guid("d4ec0d4e-e8c1-40ec-80d0-efb2071d56bb"),
                Description = "Checking"
            });
            accountTypes.Add(new AccountType
            {
                Id = new Guid("d6eb0a4f-bba5-491a-976b-504a3e15dcce"),
                Description = "Savings"
            });
            accountTypes.Add(new AccountType
            {
                Id = new Guid("c31201e4-b02a-4221-8ab0-03625641a622"),
                Description = "Credit Card"
            });
            foreach (var type in accountTypes)
            {
                accountDataAccess.CreateAccountTypeAsync(type);
            }
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

        public IEnumerable<GroupedList<Account>> GroupAccounts(IEnumerable<Account> accounts)
        {
            var groupedAccounts = new List<GroupedList<Account>>();
            var temp = accounts.GroupBy(a => a.OnBudget);
            foreach (var tempGroup in temp)
            {
                var description = tempGroup.Key ? "On Budget" : "Off Budget";
                var groupedList = new GroupedList<Account>(description, description);
                groupedList.AddRange(tempGroup);
                groupedAccounts.Add(groupedList);
            }

            return groupedAccounts;
        }

        public async Task<Result<Account>> SaveAccountAsync(Account account)
        {
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

                var startingBalance = new Transaction
                {
                    Id = Guid.NewGuid(),
                    CreatedDateTime = dateTimeNow,
                    ModifiedDateTime = dateTimeNow,
                    Amount = accountToUpsert.Balance,
                    ServiceDate = dateTimeNow,
                    Account = accountToUpsert,
                    Payee = Constants.StartingBalancePayee,
                    Envelope = accountToUpsert.OnBudget ? Constants.IncomeEnvelope : Constants.SystemEnvelope                 
                };
                await TransactionDataAccess.CreateTransactionAsync(startingBalance);
            }
            else
            {
                accountToUpsert.ModifiedDateTime = dateTimeNow;
                await AccountDataAccess.UpdateAccountAsync(accountToUpsert);
            }

            return new Result<Account> { Success = true, Data = accountToUpsert };
        }

        private async Task<Account> GetPopulatedAccount(Account account)
        {
            var accountTransactions = await TransactionDataAccess.ReadAccountTransactionsAsync(account.Id);

            account.Balance = accountTransactions.Sum(t => t.Amount);

            var payeeTransactions = await TransactionDataAccess.ReadPayeeTransactionsAsync(account.Id);

            account.Balance -= payeeTransactions.Sum(t => t.Amount);

            return account;
        }
    }
}
