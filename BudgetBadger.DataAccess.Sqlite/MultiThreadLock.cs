using System;
using System.Threading;

namespace BudgetBadger.DataAccess.Sqlite
{
    public static class MultiThreadLock
    {
        public static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
    }
}
