using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public class NullSync : ISync
    {
        public Task<Result> FullSync()
        {
            return Task.FromResult(new Result { Success = true });
        }

        public Task<Result> Pull()
        {
            return Task.FromResult(new Result { Success = true });
        }

        public Task<Result> Push()
        {
            return Task.FromResult(new Result { Success = true });
        }
    }
}
