using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public interface ISync
    {
        Task<Result> Sync();
        Task<Result> SyncDown();
        Task<Result> SyncUp();
    }
}
