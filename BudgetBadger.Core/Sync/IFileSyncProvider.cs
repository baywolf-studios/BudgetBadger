using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public interface IFileSyncProvider
    {
        Task<Result> GetLatest(string pathToPutLatest);
        Task<Result> Commit(string pathToCommit);
    }
}
