using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public class SyncLogic : ISyncLogic
    {
        readonly IAccountDataAccess LocalAccountDataAccess;
        readonly IAccountDataAccess RemoteAccountDataAccess;
        readonly IPayeeDataAccess LocalPayeeDataAccess;
        readonly IPayeeDataAccess RemotePayeeDataAccess;
        readonly IEnvelopeDataAccess LocalEnvelopeDataAccess;
        readonly IEnvelopeDataAccess RemoteEnvelopeDataAccess;
        readonly ITransactionDataAccess LocalTransactionDataAccess;
        readonly ITransactionDataAccess RemoteTransactionDataAccess;

        public SyncLogic(IAccountDataAccess localAccountDataAccess,
                         IAccountDataAccess remoteAccountDataAccess)
        {
            LocalAccountDataAccess = localAccountDataAccess;
            RemoteAccountDataAccess = remoteAccountDataAccess;
        }

        public async Task<Result> Sync()
        {
            var result = new Result();

            // sync to remote
            result = await SyncToRemote();

            // sync to local
            result = await SyncToLocal();

            return result;
        }

        public async Task<Result> SyncToRemote()
        {
            var result = new Result();

            // sync to remote
            result = await SyncAccounts(LocalAccountDataAccess, RemoteAccountDataAccess);
            result = await SyncPayees(LocalPayeeDataAccess, RemotePayeeDataAccess);
            result = await SyncEnvelopes(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
            result = await SyncTransactions(LocalTransactionDataAccess, RemoteTransactionDataAccess);

            return result;
        }

        public async Task<Result> SyncToLocal()
        {
            var result = new Result();

            // sync to local
            result = await SyncAccounts(RemoteAccountDataAccess, LocalAccountDataAccess);
            result = await SyncPayees(RemotePayeeDataAccess, LocalPayeeDataAccess);
            result = await SyncEnvelopes(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
            result = await SyncTransactions(RemoteTransactionDataAccess, LocalTransactionDataAccess);

            return result;
        }

        public async Task<Result> SyncAccounts(IAccountDataAccess sourceAccountDataAccess, IAccountDataAccess targetAccountDataAccess)
        {
            var sourceAccountTypes = await sourceAccountDataAccess.ReadAccountTypesAsync();
            var sourceAccounts = await sourceAccountDataAccess.ReadAccountsAsync();

            var targetAccountTypes = await targetAccountDataAccess.ReadAccountTypesAsync();
            var targetAccounts = await targetAccountDataAccess.ReadAccountsAsync();

            var sourceAccountTypesDictionary = sourceAccountTypes.ToDictionary(a => a.Id, a2 => a2);
            var sourceAccountsDictionary = sourceAccounts.ToDictionary(a => a.Id, a2 => a2);

            var targetAccountTypesDictionary = targetAccountTypes.ToDictionary(a => a.Id, a2 => a2);
            var targetAccountsDictionary = targetAccounts.ToDictionary(a => a.Id, a2 => a2);

            //add new account types
            var accountTypesToAdd = sourceAccountTypesDictionary.Keys.Except(targetAccountTypesDictionary.Keys);
            foreach(var accountTypeId in accountTypesToAdd)
            {
                var accountTypeToAdd = sourceAccountTypesDictionary[accountTypeId];
                await targetAccountDataAccess.CreateAccountTypeAsync(accountTypeToAdd);
            }

            //add new accounts
            var accountsToAdd = sourceAccountsDictionary.Keys.Except(targetAccountsDictionary.Keys);
            foreach (var accountId in accountsToAdd)
            {
                var accountToAdd = sourceAccountsDictionary[accountId];
                await targetAccountDataAccess.CreateAccountAsync(accountToAdd);
            }

            //update account types
            var accountTypesToUpdate = sourceAccountTypesDictionary.Keys.Intersect(targetAccountTypesDictionary.Keys);
            foreach(var accountTypeId in accountTypesToUpdate)
            {
                var sourceAccountType = sourceAccountTypesDictionary[accountTypeId];
                var targetAccountType = targetAccountTypesDictionary[accountTypeId];

                if (sourceAccountType.ModifiedDateTime > targetAccountType.ModifiedDateTime)
                {
                    await targetAccountDataAccess.UpdateAccountTypeAsync(sourceAccountType);
                }
            }

            //update accounts
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

            return new Result();
        }

        public async Task<Result> SyncEnvelopes(IEnvelopeDataAccess sourceEnvelopeDataAccess, IEnvelopeDataAccess targetEnvelopeDataAccess)
        {
            return new Result();
        }

        public async Task<Result> SyncPayees(IPayeeDataAccess sourcePayeeDataAccess, IPayeeDataAccess targetPayeeDataAccess)
        {
            return new Result();
        }

        public async Task<Result> SyncTransactions(ITransactionDataAccess sourceTransactionDataAccess, ITransactionDataAccess targetTransactionDataAccess)
        {
            return new Result();
        }
    }
}
