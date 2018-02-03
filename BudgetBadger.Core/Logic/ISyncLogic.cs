using System;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface ISyncLogic
    {
        Task<Result> Sync();
        Task<Result> SyncToRemote();
        Task<Result> SyncToLocal();
    }
}
