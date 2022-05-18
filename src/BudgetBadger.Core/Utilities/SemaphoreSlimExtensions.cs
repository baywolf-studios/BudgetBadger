using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetBadger.Core.Utilities
{
    public static class SemaphoreSlimExtensions
    {
        public static async Task<IDisposable> UseWaitAsync(this SemaphoreSlim semaphoreSlim,
            CancellationToken cancelToken = default)
        {
            await semaphoreSlim.WaitAsync(cancelToken).ConfigureAwait(false);
            return new ReleaseWrapper(semaphoreSlim);
        }

        private class ReleaseWrapper : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            private bool _isDisposed;

            public ReleaseWrapper(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _semaphore.Release();
                _isDisposed = true;
            }
        }
    }
}
