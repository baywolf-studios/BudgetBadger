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
        readonly IDataAccess _localDataAccess;
        readonly IDataAccess _remoteDataAccess;

        public TransactionSyncLogic(IDataAccess localDataAccess,
                                   IDataAccess remoteDataAccess)
        {
            _localDataAccess = localDataAccess;
            _remoteDataAccess = remoteDataAccess;
        }

        public async Task<Result> PullAsync()
        {
            var result = new Result();

            try
            {
                await SyncTransactions(_remoteDataAccess, _localDataAccess);
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
                await SyncTransactions(_localDataAccess, _remoteDataAccess);
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

        async Task SyncTransactions(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            await sourceDataAccess.Init();
            await targetDataAccess.Init();

            var sourceTransactions = await sourceDataAccess.ReadTransactionsAsync();
            var targetTransactions = await targetDataAccess.ReadTransactionsAsync();

            var sourceTransactionsDictionary = sourceTransactions.ToDictionary(a => a.Id, a2 => a2);
            var targetTransactionsDictionary = targetTransactions.ToDictionary(a => a.Id, a2 => a2);

            var transactionsToAdd = sourceTransactionsDictionary.Keys.Except(targetTransactionsDictionary.Keys);
            foreach (var transactionId in transactionsToAdd)
            {
                var transactionToAdd = sourceTransactionsDictionary[transactionId];
                await targetDataAccess.CreateTransactionAsync(transactionToAdd);
            }

            var transactionsToUpdate = sourceTransactionsDictionary.Keys.Intersect(targetTransactionsDictionary.Keys);
            foreach (var transactionId in transactionsToUpdate)
            {
                var sourceTransaction = sourceTransactionsDictionary[transactionId];
                var targetTransaction = targetTransactionsDictionary[transactionId];

                if (sourceTransaction.ModifiedDateTime > targetTransaction.ModifiedDateTime)
                {
                    await targetDataAccess.UpdateTransactionAsync(sourceTransaction);
                }
            }
        }
    }
}
