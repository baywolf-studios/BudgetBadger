using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface ISyncLogic
    {
        Task<Result> SyncAccounts();
    }
}
