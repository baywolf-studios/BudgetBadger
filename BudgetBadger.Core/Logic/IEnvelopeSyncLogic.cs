using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IEnvelopeSyncLogic
    {
        Task<Result> SyncAsync();
        Task<Result> PullAsync();
        Task<Result> PushAsync();
    }
}
