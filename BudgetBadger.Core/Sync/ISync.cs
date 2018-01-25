using System;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Sync
{
    public interface ISync
    {
        Result Sync();
        Result SyncDown();
        Result SyncUp();
    }
}
