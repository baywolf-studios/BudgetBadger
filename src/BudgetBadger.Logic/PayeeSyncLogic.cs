using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public class PayeeSyncLogic : IPayeeSyncLogic
    {
        readonly IDataAccess _localDataAccess;
        readonly IDataAccess _remoteDataAccess;

        public PayeeSyncLogic(IDataAccess localDataAccess,
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
                await SyncPayees(_remoteDataAccess, _localDataAccess);
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
                await SyncPayees(_localDataAccess, _remoteDataAccess);
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

        async Task SyncPayees(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            await sourceDataAccess.Init();
            await targetDataAccess.Init();

            var sourcePayees = await sourceDataAccess.ReadPayeesAsync();
            var targetPayees = await targetDataAccess.ReadPayeesAsync();

            var sourcePayeesDictionary = sourcePayees.ToDictionary(a => a.Id, a2 => a2);
            var targetPayeesDictionary = targetPayees.ToDictionary(a => a.Id, a2 => a2);

            var payeesToAdd = sourcePayeesDictionary.Keys.Except(targetPayeesDictionary.Keys);
            foreach (var payeeId in payeesToAdd)
            {
                var payeeToAdd = sourcePayeesDictionary[payeeId];
                await targetDataAccess.CreatePayeeAsync(payeeToAdd);
            }

            var payeesToUpdate = sourcePayeesDictionary.Keys.Intersect(targetPayeesDictionary.Keys);
            foreach (var payeeId in payeesToUpdate)
            {
                var sourcePayee = sourcePayeesDictionary[payeeId];
                var targetPayee = targetPayeesDictionary[payeeId];

                if (sourcePayee.ModifiedDateTime > targetPayee.ModifiedDateTime)
                {
                    await targetDataAccess.UpdatePayeeAsync(sourcePayee);
                }
            }
        }
    }
}
