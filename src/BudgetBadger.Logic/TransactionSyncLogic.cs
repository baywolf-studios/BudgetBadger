using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public class TransactionSyncLogic : ITransactionSyncLogic
    {
        readonly ITransactionDataAccess _localTransactionDataAccess;
        readonly ITransactionDataAccess _remoteTransactionDataAccess;

        public TransactionSyncLogic(ITransactionDataAccess localTransactionDataAccess,
                                   ITransactionDataAccess remoteTransactionDataAcces)
        {
            _localTransactionDataAccess = localTransactionDataAccess;
            _remoteTransactionDataAccess = remoteTransactionDataAcces;
        }

        public async Task<Result> PullAsync()
        {
            var result = new Result();

            try
            {
                await SyncTransactions(_remoteTransactionDataAccess, _localTransactionDataAccess);
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
                await SyncTransactions(_localTransactionDataAccess, _remoteTransactionDataAccess);
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

        async Task SyncTransactions(ITransactionDataAccess sourceTransactionDataAccess, ITransactionDataAccess targetTransactionDataAccess)
        {
            await sourceTransactionDataAccess.Init();
            await targetTransactionDataAccess.Init();

            var sourceTransactions = await sourceTransactionDataAccess.ReadTransactionsAsync();
            var targetTransactions = await targetTransactionDataAccess.ReadTransactionsAsync();

            var sourceTransactionsDictionary = sourceTransactions.ToDictionary(a => a.Id, a2 => a2);
            var targetTransactionsDictionary = targetTransactions.ToDictionary(a => a.Id, a2 => a2);

            var transactionsToAdd = sourceTransactionsDictionary.Keys.Except(targetTransactionsDictionary.Keys);
            foreach (var transactionId in transactionsToAdd)
            {
                var transactionToAdd = sourceTransactionsDictionary[transactionId];
                await targetTransactionDataAccess.CreateTransactionAsync(transactionToAdd);
            }

            var transactionsToUpdate = sourceTransactionsDictionary.Keys.Intersect(targetTransactionsDictionary.Keys);
            foreach (var transactionId in transactionsToUpdate)
            {
                var sourceTransaction = sourceTransactionsDictionary[transactionId];
                var targetTransaction = targetTransactionsDictionary[transactionId];

                if (sourceTransaction.ModifiedDateTime > targetTransaction.ModifiedDateTime)
                {
                    await targetTransactionDataAccess.UpdateTransactionAsync(sourceTransaction);
                }
            }
        }
    }
}
