using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Sync;

namespace BudgetBadger.Forms
{
    public interface ISyncFactory
    {
        ISync GetSyncService();
        Task SetLastSyncDateTime(DateTime dateTime);
        string GetLastSyncDateTime();
    }
}
