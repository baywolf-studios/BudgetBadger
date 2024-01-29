using System;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetBadger.Core.Utilities
{
    public static class ActionExtensions
    {
        public static Action Debounce(this Action action, int milliseconds = 300, CancellationToken cancellationToken = default)
        {
            var last = 0;
            return () =>
            {
                var current = Interlocked.Increment(ref last);
                Task.Delay(milliseconds).ContinueWith(t =>
                {
                    if (current == last && !cancellationToken.IsCancellationRequested)
                    {
                        action();
                    }
                    t.Dispose();
                });
            };
        }
    }
}

