using System;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public class Sync : ISync
    {
        readonly IAccountSyncLogic _accountSyncLogic;
        readonly IPayeeSyncLogic _payeeSyncLogic;
        readonly IEnvelopeSyncLogic _envelopeSyncLogic;
        readonly ITransactionSyncLogic _transactionSyncLogic;

        public Sync(IAccountSyncLogic accountSyncLogic,
                    IPayeeSyncLogic payeeSyncLogic,
                    IEnvelopeSyncLogic envelopeSyncLogic,
                    ITransactionSyncLogic transactionSyncLogic)
        {
            _accountSyncLogic = accountSyncLogic;
            _payeeSyncLogic = payeeSyncLogic;
            _envelopeSyncLogic = envelopeSyncLogic;
            _transactionSyncLogic = transactionSyncLogic;
        }

        public virtual async Task<Result> FullSync()
        {
            var result = new Result();

            result = await _accountSyncLogic.SyncAsync();

            if (result.Success)
            {
                result = await _payeeSyncLogic.SyncAsync();

                if (result.Success)
                {
                    result = await _envelopeSyncLogic.SyncAsync();

                    if (result.Success)
                    {
                        result = await _transactionSyncLogic.SyncAsync();
                    }
                }
            }

            return result;
        }

        public virtual async Task<Result> Push()
        {
            var result = new Result();

            result = await _accountSyncLogic.PushAsync();

            if (result.Success)
            {
                result = await _payeeSyncLogic.PushAsync();

                if (result.Success)
                {
                    result = await _envelopeSyncLogic.PushAsync();

                    if (result.Success)
                    {
                        result = await _transactionSyncLogic.PushAsync();
                    }
                }
            }

            return result;
        }

        public virtual async Task<Result> Pull()
        {
            var result = new Result();

            result = await _accountSyncLogic.PullAsync();

            if (result.Success)
            {
                result = await _payeeSyncLogic.PullAsync();

                if (result.Success)
                {
                    result = await _envelopeSyncLogic.PullAsync();

                    if (result.Success)
                    {
                        result = await _transactionSyncLogic.PullAsync();
                    }
                }
            }

            return result;
        }
    }
}
