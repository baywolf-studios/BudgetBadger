using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface ITransactionSyncLogic
    {
        Task<Result> SyncAsync();
        Task<Result> PullAsync();
        Task<Result> PushAsync();
    }
}
