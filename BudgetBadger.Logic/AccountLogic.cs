﻿using System;
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

        public AccountLogic(IAccountDataAccess accountDataAccess, ITransactionDataAccess transactionDataAccess)
        {
            AccountDataAccess = accountDataAccess;
            TransactionDataAccess = transactionDataAccess;

            //var accountTypes = new List<AccountType>();
            //accountTypes.Add(new AccountType
            //{
            //    Id = new Guid("d4ec0d4e-e8c1-40ec-80d0-efb2071d56bb"),
            //    Description = "Checking"
            //});
            //accountTypes.Add(new AccountType
            //{
            //    Id = new Guid("d6eb0a4f-bba5-491a-976b-504a3e15dcce"),
            //    Description = "Savings"
            //});
            //accountTypes.Add(new AccountType
            //{
            //    Id = new Guid("c31201e4-b02a-4221-8ab0-03625641a622"),
            //    Description = "Credit Card"
            //});
            //foreach (var type in accountTypes)
            //{
            //    accountDataAccess.CreateAccountTypeAsync(type);
            //}
        }

        public async Task<Result> DeleteAccountAsync(Account account)
        {
            var newAccont = account.DeepCopy();
            newAccont.DeletedDateTime = DateTime.Now;
            await AccountDataAccess.UpdateAccountAsync(newAccont);

            return new Result { Success = true };
        }

        public async Task<Result<Account>> GetAccountAsync(Guid id)
        {
            var account = await AccountDataAccess.ReadAccountAsync(id);

            return new Result<Account> { Success = true, Data = account };
        }

        public async Task<Result<IEnumerable<Account>>> GetAccountsAsync()
        {
            var result = new Result<IEnumerable<Account>>();

            var accounts = await AccountDataAccess.ReadAccountsAsync();

            var tasks = accounts.Select(a => FillAccount(a));

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

            return accounts.Where(a => a.Description.Contains(searchText));
        }

        public IEnumerable<GroupedList<Account>> GroupAccounts(IEnumerable<Account> accounts, bool includeDeleted = false)
        {
            var groupedAccounts = new List<GroupedList<Account>>();
            var temp = accounts.GroupBy(a => a.Type.Description);
            foreach (var tempGroup in temp)
            {
                var groupedList = new GroupedList<Account>(tempGroup.Key, tempGroup.Key);
                groupedList.AddRange(tempGroup);
                groupedAccounts.Add(groupedList);
            }

            return groupedAccounts;
        }

        public async Task<Result<Account>> UpsertAccountAsync(Account account)
        {
            var newAccount = account.DeepCopy();

            if (newAccount.CreatedDateTime == null)
            {
                newAccount.CreatedDateTime = DateTime.Now;
                newAccount.ModifiedDateTime = DateTime.Now;
                await AccountDataAccess.CreateAccountAsync(newAccount);
            }
            else
            {
                newAccount.ModifiedDateTime = DateTime.Now;
                await AccountDataAccess.UpdateAccountAsync(newAccount);
            }

            return new Result<Account> { Success = true, Data = newAccount };
        }

        public async Task<Account> FillAccount(Account account)
        {
            var accountTransactions = await TransactionDataAccess.ReadAccountTransactionsAsync(account.Id);

            account.Balance = accountTransactions.Sum(t => t.Amount);

            return account;
        }
    }
}
