using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IPayeeSyncLogic
    {
        Task<Result> SyncAsync();
        Task<Result> PullAsync();
        Task<Result> PushAsync();
    }
}
