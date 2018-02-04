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
                         IAccountDataAccess remoteAccountDataAccess,
                         IPayeeDataAccess localPayeeDataAccess,
                         IPayeeDataAccess remotePayeeDataAccess,
                         IEnvelopeDataAccess localEnvelopeDataAccess,
                         IEnvelopeDataAccess remoteEnvelopeDataAccess,
                         ITransactionDataAccess localTransactionDataAccess,
                         ITransactionDataAccess remoteTransactionDataAccess)
        {
            LocalAccountDataAccess = localAccountDataAccess;
            RemoteAccountDataAccess = remoteAccountDataAccess;
            LocalPayeeDataAccess = localPayeeDataAccess;
            RemotePayeeDataAccess = remotePayeeDataAccess;
            LocalEnvelopeDataAccess = localEnvelopeDataAccess;
            RemoteEnvelopeDataAccess = remoteEnvelopeDataAccess;
            LocalTransactionDataAccess = localTransactionDataAccess;
            RemoteTransactionDataAccess = remoteTransactionDataAccess;
        }

        public async Task<Result> Sync()
        {
            var result = new Result();

            // sync to remote
            result = await SyncToRemote();

            if (result.Success)
            {
                // sync to local
                result = await SyncToLocal();
            }

            return result;
        }

        public async Task<Result> SyncToRemote()
        {
            var result = new Result();

            // sync to remote
            try
            {
                await SyncLogicHelper.SyncAccountTypes(LocalAccountDataAccess, RemoteAccountDataAccess);
                await SyncLogicHelper.SyncAccounts(LocalAccountDataAccess, RemoteAccountDataAccess);
                await SyncLogicHelper.SyncPayees(LocalPayeeDataAccess, RemotePayeeDataAccess);
                await SyncLogicHelper.SyncEnvelopeGroups(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncLogicHelper.SyncEnvelopes(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncLogicHelper.SyncBudgetSchedules(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncLogicHelper.SyncBudgets(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncLogicHelper.SyncTransactions(LocalTransactionDataAccess, RemoteTransactionDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result> SyncToLocal()
        {
            var result = new Result();

            // sync to local
            try
            {
                await SyncLogicHelper.SyncAccountTypes(RemoteAccountDataAccess, LocalAccountDataAccess);
                await SyncLogicHelper.SyncAccounts(RemoteAccountDataAccess, LocalAccountDataAccess);
                await SyncLogicHelper.SyncPayees(RemotePayeeDataAccess, LocalPayeeDataAccess);
                await SyncLogicHelper.SyncEnvelopeGroups(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncLogicHelper.SyncEnvelopes(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncLogicHelper.SyncBudgetSchedules(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncLogicHelper.SyncBudgets(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncLogicHelper.SyncTransactions(RemoteTransactionDataAccess, LocalTransactionDataAccess);
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

    }
}
