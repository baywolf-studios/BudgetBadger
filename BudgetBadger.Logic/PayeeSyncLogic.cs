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
        readonly IPayeeDataAccess _localPayeeDataAccess;
        readonly IPayeeDataAccess _remotePayeeDataAccess;

        public PayeeSyncLogic(IPayeeDataAccess localPayeeDataAcces,
                             IPayeeDataAccess remotePayeeDataAccess)
        {
            _localPayeeDataAccess = localPayeeDataAcces;
            _remotePayeeDataAccess = remotePayeeDataAccess;
        }

        public async Task<Result> PullAsync()
        {
            var result = new Result();

            try
            {
                await SyncPayees(_remotePayeeDataAccess, _localPayeeDataAccess);
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
                await SyncPayees(_localPayeeDataAccess, _remotePayeeDataAccess);
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

        async Task SyncPayees(IPayeeDataAccess sourcePayeeDataAccess, IPayeeDataAccess targetPayeeDataAccess)
        {
            await sourcePayeeDataAccess.Init();
            await targetPayeeDataAccess.Init();

            var sourcePayees = await sourcePayeeDataAccess.ReadPayeesAsync();
            var targetPayees = await targetPayeeDataAccess.ReadPayeesAsync();

            var sourcePayeesDictionary = sourcePayees.ToDictionary(a => a.Id, a2 => a2);
            var targetPayeesDictionary = targetPayees.ToDictionary(a => a.Id, a2 => a2);

            var payeesToAdd = sourcePayeesDictionary.Keys.Except(targetPayeesDictionary.Keys);
            foreach (var payeeId in payeesToAdd)
            {
                var payeeToAdd = sourcePayeesDictionary[payeeId];
                await targetPayeeDataAccess.CreatePayeeAsync(payeeToAdd);
            }

            var payeesToUpdate = sourcePayeesDictionary.Keys.Intersect(targetPayeesDictionary.Keys);
            foreach (var payeeId in payeesToUpdate)
            {
                var sourcePayee = sourcePayeesDictionary[payeeId];
                var targetPayee = targetPayeesDictionary[payeeId];

                if (sourcePayee.ModifiedDateTime > targetPayee.ModifiedDateTime)
                {
                    await targetPayeeDataAccess.UpdatePayeeAsync(sourcePayee);
                }
            }
        }
    }
}
