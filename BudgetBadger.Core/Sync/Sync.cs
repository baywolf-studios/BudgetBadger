using System;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public class Sync : ISync
    {
        readonly IAccountDataAccess LocalAccountDataAccess;
        readonly IAccountDataAccess RemoteAccountDataAccess;
        readonly IPayeeDataAccess LocalPayeeDataAccess;
        readonly IPayeeDataAccess RemotePayeeDataAccess;
        readonly IEnvelopeDataAccess LocalEnvelopeDataAccess;
        readonly IEnvelopeDataAccess RemoteEnvelopeDataAccess;
        readonly ITransactionDataAccess LocalTransactionDataAccess;
        readonly ITransactionDataAccess RemoteTransactionDataAccess;

        public Sync(IAccountDataAccess localAccountDataAccess,
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

        public virtual async Task<Result> FullSync()
        {
            var result = new Result();

            try
            {
                await SyncHelper.SyncAccountTypes(LocalAccountDataAccess, RemoteAccountDataAccess);
                await SyncHelper.SyncAccounts(LocalAccountDataAccess, RemoteAccountDataAccess);
                await SyncHelper.SyncPayees(LocalPayeeDataAccess, RemotePayeeDataAccess);
                await SyncHelper.SyncEnvelopeGroups(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncHelper.SyncEnvelopes(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncHelper.SyncBudgetSchedules(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncHelper.SyncBudgets(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncHelper.SyncTransactions(LocalTransactionDataAccess, RemoteTransactionDataAccess);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            try
            {
                await SyncHelper.SyncAccountTypes(RemoteAccountDataAccess, LocalAccountDataAccess);
                await SyncHelper.SyncAccounts(RemoteAccountDataAccess, LocalAccountDataAccess);
                await SyncHelper.SyncPayees(RemotePayeeDataAccess, LocalPayeeDataAccess);
                await SyncHelper.SyncEnvelopeGroups(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncHelper.SyncEnvelopes(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncHelper.SyncBudgetSchedules(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncHelper.SyncBudgets(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncHelper.SyncTransactions(RemoteTransactionDataAccess, LocalTransactionDataAccess);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public virtual async Task<Result> Push()
        {
            var result = new Result();

            // sync to remote
            try
            {
                await SyncHelper.SyncAccountTypes(LocalAccountDataAccess, RemoteAccountDataAccess);
                await SyncHelper.SyncAccounts(LocalAccountDataAccess, RemoteAccountDataAccess);
                await SyncHelper.SyncPayees(LocalPayeeDataAccess, RemotePayeeDataAccess);
                await SyncHelper.SyncEnvelopeGroups(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncHelper.SyncEnvelopes(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncHelper.SyncBudgetSchedules(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncHelper.SyncBudgets(LocalEnvelopeDataAccess, RemoteEnvelopeDataAccess);
                await SyncHelper.SyncTransactions(LocalTransactionDataAccess, RemoteTransactionDataAccess);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public virtual async Task<Result> Pull()
        {
            var result = new Result();

            // sync to local
            try
            {
                await SyncHelper.SyncAccountTypes(RemoteAccountDataAccess, LocalAccountDataAccess);
                await SyncHelper.SyncAccounts(RemoteAccountDataAccess, LocalAccountDataAccess);
                await SyncHelper.SyncPayees(RemotePayeeDataAccess, LocalPayeeDataAccess);
                await SyncHelper.SyncEnvelopeGroups(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncHelper.SyncEnvelopes(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncHelper.SyncBudgetSchedules(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncHelper.SyncBudgets(RemoteEnvelopeDataAccess, LocalEnvelopeDataAccess);
                await SyncHelper.SyncTransactions(RemoteTransactionDataAccess, LocalTransactionDataAccess);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
