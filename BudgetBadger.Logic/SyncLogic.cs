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

            //var accountTypeIdsToAdd = sourceAccountTypes.Select(a => a.Id).Except(targetAccountTypes.Select(a2 => a2.Id));

            //switch to using except but I'll have to override equality on all objects
            var accountTypesToAdd = sourceAccountTypes.Where(p => !targetAccountTypes.Any(p2 => p2.Id == p.Id));
            foreach(var accountType in accountTypesToAdd)
            {
                await targetAccountDataAccess.CreateAccountTypeAsync(accountType);
            }

            //switch to using except but I'll have to override equality on all objects
            var accountsToAdd = sourceAccounts.Where(p => !targetAccounts.Any(p2 => p2.Id == p.Id));
            foreach (var account in accountsToAdd)
            {
                await targetAccountDataAccess.CreateAccountAsync(account);
            }

            //update stuff here
            var accountTypesToUpdate = sourceAccountTypes.Where(a => a.ModifiedDateTime > targetAccountTypes.FirstOrDefault(t => t.Id == a.Id)?.ModifiedDateTime);
            foreach (var accountType in accountTypesToUpdate)
            {
                await targetAccountDataAccess.UpdateAccountTypeAsync(accountType);
            }

            //update stuff here
            var accountsToUpdate = sourceAccounts.Where(a => a.ModifiedDateTime > targetAccounts.FirstOrDefault(t => t.Id == a.Id)?.ModifiedDateTime);
            foreach(var account in accountsToUpdate)
            {
                await targetAccountDataAccess.UpdateAccountAsync(account);
            }

            //not deleting yet
            //may do a purge later?

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
