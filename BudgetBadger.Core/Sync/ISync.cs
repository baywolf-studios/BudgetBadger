using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public interface ISync
    {
        Task<Result> FullSync();
        Task<Result> Pull();
        Task<Result> Push();
    }
}
