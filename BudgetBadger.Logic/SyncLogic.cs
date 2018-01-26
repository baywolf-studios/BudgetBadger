using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public class SyncLogic : ISyncLogic
    {
        readonly IAccountDataAccess SourceAccountDataAccess;
        readonly IAccountDataAccess TargetAccountDataAccess;

        public SyncLogic(IAccountDataAccess sourceAccountDataAccess, IAccountDataAccess targetAccountDataAccess)
        {
            SourceAccountDataAccess = sourceAccountDataAccess;
            TargetAccountDataAccess = targetAccountDataAccess;
        }

        public async Task<Result> SyncAccounts()
        {
            var sourceAccountTypes = await SourceAccountDataAccess.ReadAccountTypesAsync();
            var sourceAccounts = await SourceAccountDataAccess.ReadAccountsAsync();

            var targetAccountTypes = await TargetAccountDataAccess.ReadAccountTypesAsync();
            var targetAccounts = await TargetAccountDataAccess.ReadAccountsAsync();

            //switch to using except but I'll have to override equality on all objects
            var accountTypesToAdd = sourceAccountTypes.Where(p => !targetAccountTypes.Any(p2 => p2.Id == p.Id));
            foreach(var accountType in accountTypesToAdd)
            {
                await TargetAccountDataAccess.CreateAccountTypeAsync(accountType);
            }

            //switch to using except but I'll have to override equality on all objects
            var accountsToAdd = sourceAccounts.Where(p => !targetAccounts.Any(p2 => p2.Id == p.Id));
            foreach (var account in accountsToAdd)
            {
                await TargetAccountDataAccess.CreateAccountAsync(account);
            }

            ////update stuff here
            //var accountTypesToUpdate = sourceAccountTypes.Where(a => a.ModifiedDateTime > targetAccountTypes.FirstOrDefault(t => t.Id == a.Id)?.ModifiedDateTime);
            //foreach (var accountType in accountTypesToUpdate)
            //{
            //    await TargetAccountDataAccess.UpdateAccountTypeAsync(accountType);
            //}

            //update stuff here
            var accountsToUpdate = sourceAccounts.Where(a => a.ModifiedDateTime > targetAccounts.FirstOrDefault(t => t.Id == a.Id)?.ModifiedDateTime);
            foreach(var account in accountsToUpdate)
            {
                await TargetAccountDataAccess.UpdateAccountAsync(account);
            }

            //not deleting yet
            //may do a purge later?

            return new Result();
        }
    }
}
