using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetBadger.DataAccess.Sqlite
{
    public static class MultiThreadLock
    {
        public static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public static async Task<IDisposable> UseWaitAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            await _semaphoreSlim.WaitAsync(cancelToken).ConfigureAwait(false);
            return new ReleaseWrapper(_semaphoreSlim);
        }

        class ReleaseWrapper : IDisposable
        {
            readonly SemaphoreSlim _semaphore;

            bool _isDisposed;

            public ReleaseWrapper(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_isDisposed)
                    return;

                _semaphore.Release();
                _isDisposed = true;
            }
        }
    }
}
