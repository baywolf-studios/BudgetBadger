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
        readonly IDataAccess _localDataAcces;
        readonly IDataAccess _remoteDataAccess;

        public AccountSyncLogic(IDataAccess localDataAccess,
                                IDataAccess remoteDataAccess)
        {
            _localDataAcces = localDataAccess;
            _remoteDataAccess = remoteDataAccess;
        }

        public async Task<Result> PullAsync()
        {
            var result = new Result();

            try
            {
                await SyncAccounts(_remoteDataAccess, _localDataAcces);
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
                await SyncAccounts(_localDataAcces, _remoteDataAccess);
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

        async Task SyncAccounts(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            await sourceDataAccess.Init();
            await targetDataAccess.Init();

            var sourceAccounts = await sourceDataAccess.ReadAccountsAsync();
            var targetAccounts = await targetDataAccess.ReadAccountsAsync();

            var sourceAccountsDictionary = sourceAccounts.ToDictionary(a => a.Id, a2 => a2);
            var targetAccountsDictionary = targetAccounts.ToDictionary(a => a.Id, a2 => a2);

            var accountsToAdd = sourceAccountsDictionary.Keys.Except(targetAccountsDictionary.Keys);
            foreach (var accountId in accountsToAdd)
            {
                var accountToAdd = sourceAccountsDictionary[accountId];
                await targetDataAccess.CreateAccountAsync(accountToAdd);
            }

            var accountsToUpdate = sourceAccountsDictionary.Keys.Intersect(targetAccountsDictionary.Keys);
            foreach (var accountId in accountsToUpdate)
            {
                var sourceAccount = sourceAccountsDictionary[accountId];
                var targetAccount = targetAccountsDictionary[accountId];

                if (sourceAccount.ModifiedDateTime > targetAccount.ModifiedDateTime)
                {
                    await targetDataAccess.UpdateAccountAsync(sourceAccount);
                }
            }
        }
    }
}
