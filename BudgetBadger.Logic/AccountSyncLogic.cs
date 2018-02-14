using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public class AccountSyncLogic : IAccountSyncLogic
    {
        readonly IAccountDataAccess _localAccountDataAcces;
        readonly IAccountDataAccess _remoteAccountDataAccess;

        public AccountSyncLogic(IAccountDataAccess localAccountDataAccess,
                                IAccountDataAccess remoteAccountDataAccess)
        {
            _localAccountDataAcces = localAccountDataAccess;
            _remoteAccountDataAccess = remoteAccountDataAccess;
        }

        public async Task<Result> PullAsync()
        {
            var result = new Result();

            try
            {
                await SyncAccountTypes(_remoteAccountDataAccess, _localAccountDataAcces);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncAccounts(_remoteAccountDataAccess, _localAccountDataAcces);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            result.Success = true;
            return result;
        }

        public async Task<Result> PushAsync()
        {
            var result = new Result();

            try
            {
                await SyncAccountTypes(_localAccountDataAcces, _remoteAccountDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncAccounts(_localAccountDataAcces, _remoteAccountDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            result.Success = true;
            return result;
        }

        public async Task<Result> SyncAsync()
        {
            var result = new Result();

            result = await PullAsync();

            if (result.Success)
            {
                result = await PushAsync();
            }

            return result;
        }

        async Task SyncAccountTypes(IAccountDataAccess sourceAccountDataAccess, IAccountDataAccess targetAccountDataAccess)
        {
            var sourceAccountTypes = await sourceAccountDataAccess.ReadAccountTypesAsync();
            var targetAccountTypes = await targetAccountDataAccess.ReadAccountTypesAsync();

            var sourceAccountTypesDictionary = sourceAccountTypes.ToDictionary(a => a.Id, a2 => a2);
            var targetAccountTypesDictionary = targetAccountTypes.ToDictionary(a => a.Id, a2 => a2);

            var accountTypesToAdd = sourceAccountTypesDictionary.Keys.Except(targetAccountTypesDictionary.Keys);
            foreach (var accountTypeId in accountTypesToAdd)
            {
                var accountTypeToAdd = sourceAccountTypesDictionary[accountTypeId];
                await targetAccountDataAccess.CreateAccountTypeAsync(accountTypeToAdd);
            }

            var accountTypesToUpdate = sourceAccountTypesDictionary.Keys.Intersect(targetAccountTypesDictionary.Keys);
            foreach (var accountTypeId in accountTypesToUpdate)
            {
                var sourceAccountType = sourceAccountTypesDictionary[accountTypeId];
                var targetAccountType = targetAccountTypesDictionary[accountTypeId];

                if (sourceAccountType.ModifiedDateTime > targetAccountType.ModifiedDateTime)
                {
                    await targetAccountDataAccess.UpdateAccountTypeAsync(sourceAccountType);
                }
            }
        }

        async Task SyncAccounts(IAccountDataAccess sourceAccountDataAccess, IAccountDataAccess targetAccountDataAccess)
        {
            var sourceAccounts = await sourceAccountDataAccess.ReadAccountsAsync();
            var targetAccounts = await targetAccountDataAccess.ReadAccountsAsync();

            var sourceAccountsDictionary = sourceAccounts.ToDictionary(a => a.Id, a2 => a2);
            var targetAccountsDictionary = targetAccounts.ToDictionary(a => a.Id, a2 => a2);

            var accountsToAdd = sourceAccountsDictionary.Keys.Except(targetAccountsDictionary.Keys);
            foreach (var accountId in accountsToAdd)
            {
                var accountToAdd = sourceAccountsDictionary[accountId];
                await targetAccountDataAccess.CreateAccountAsync(accountToAdd);
            }

            var accountsToUpdate = sourceAccountsDictionary.Keys.Intersect(targetAccountsDictionary.Keys);
            foreach (var accountId in accountsToUpdate)
            {
                var sourceAccount = sourceAccountsDictionary[accountId];
                var targetAccount = targetAccountsDictionary[accountId];

                if (sourceAccount.ModifiedDateTime > targetAccount.ModifiedDateTime)
                {
                    await targetAccountDataAccess.UpdateAccountAsync(sourceAccount);
                }
            }
        }
    }
}
